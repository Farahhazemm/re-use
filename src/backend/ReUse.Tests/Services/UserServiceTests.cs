using AutoMapper;

using Microsoft.AspNetCore.Http;

using Moq;

using ReUse.Application.DTOs.Users.UserProfile;
using ReUse.Application.Enums;
using ReUse.Application.Interfaces;
using ReUse.Application.Interfaces.Repository;
using ReUse.Application.Interfaces.Services.External;
using ReUse.Application.Mappers;
using ReUse.Application.Services;
using ReUse.Domain.Entities;

namespace ReUse.Tests.Services;

public class UserServiceTests
{
    // ── Mocks ────────────────────────────────────────────────────────────────
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IUserRepository> _userRepo;
    private readonly Mock<IImageValidator> _imageValidator;
    private readonly Mock<ICloudinaryService> _cloudinary;
    private readonly IMapper _mapper;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _unitOfWork = new Mock<IUnitOfWork>();
        _userRepo = new Mock<IUserRepository>();
        _imageValidator = new Mock<IImageValidator>();
        _cloudinary = new Mock<ICloudinaryService>();

        // Wire UnitOfWork.User to our mocked repository
        _unitOfWork.Setup(u => u.User).Returns(_userRepo.Object);

        // Real AutoMapper with the real production profile
        // so mapping bugs are caught, not hidden
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new UserProfileMappingProfile());
        });
        _mapper = config.CreateMapper();

        _sut = new UserService(
            _unitOfWork.Object,
            _imageValidator.Object,
            _cloudinary.Object,
            _mapper);
    }

    // ── Shared factory ───────────────────────────────────────────────────────
    private static User CreateUser(
        Guid? id = null,
        string fullName = "Ahmed Mordi",
        string email = "ahmed@example.com",
        string? profileImageUrl = null,
        string? profilePublicId = null,
        string? coverImageUrl = null,
        string? coverPublicId = null,
        int followersCount = 0,
        int followingCount = 0)
    {
        var user = new User
        {
            Id = id ?? Guid.NewGuid(),
            IdentityUserId = Guid.NewGuid().ToString(),
            FullName = fullName,
            Email = email,
            ProfileImageUrl = profileImageUrl,
            ProfileImagePublicId = profilePublicId,
            CoverImageUrl = coverImageUrl,
            CoverImagePublicId = coverPublicId,
        };

        for (int i = 0; i < followersCount; i++) user.Followers.Add(new Follow());
        for (int i = 0; i < followingCount; i++) user.Following.Add(new Follow());

        return user;
    }

    private static IFormFile CreateFormFile(string name = "photo.jpg")
    {
        var mock = new Mock<IFormFile>();
        mock.Setup(f => f.FileName).Returns(name);
        mock.Setup(f => f.Length).Returns(1024);
        return mock.Object;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GetUserProfileAsync
    //
    //  What the service actually does:
    //    1. Calls GetProfileByIdAsync(userId)
    //    2. Maps User → UserProfileResponse via AutoMapper
    //       (FollowersCount and FollowingCount come from collection .Count)
    //
    //  NOTE: There is no null-check in the service — by design the user is
    //  guaranteed to be authenticated. So "user not found" is not a code path
    //  we can test without the service throwing a NullReferenceException.
    //  We document that here so the gap is visible, not hidden.
    // ════════════════════════════════════════════════════════════════════════

    // HAPPY PATH — all fields mapped correctly
    [Fact]
    public async Task GetUserProfileAsync_ReturnsCorrectDto_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(id: userId, fullName: "Ahmed Mordi",
                                email: "ahmed@example.com",
                                followersCount: 5, followingCount: 3);
        user.Bio = "Engineer";
        user.City = "Cairo";
        user.RatingsAverage = 4.5m;
        user.RatingsCount = 10;

        _userRepo
            .Setup(r => r.GetProfileByIdAsync(It.Is<Guid>(id => id == userId)))
            .ReturnsAsync(user);

        var result = await _sut.GetUserProfileAsync(userId);

        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("Ahmed Mordi", result.FullName);
        Assert.Equal("ahmed@example.com", result.Email);
        Assert.Equal("Engineer", result.Bio);
        Assert.Equal("Cairo", result.City);
        Assert.Equal(5, result.FollowersCount);
        Assert.Equal(3, result.FollowingCount);
        Assert.Equal(4.5m, result.RatingsAverage);
        Assert.Equal(10, result.RatingsCount);

        _userRepo.Verify(r => r.GetProfileByIdAsync(userId), Times.Once);
    }

    // EDGE CASE — user with zero followers/following maps to 0, not null or exception
    [Fact]
    public async Task GetUserProfileAsync_ReturnZeroCounts_WhenNoFollowersOrFollowing()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(id: userId, followersCount: 0, followingCount: 0);

        _userRepo.Setup(r => r.GetProfileByIdAsync(userId)).ReturnsAsync(user);

        var result = await _sut.GetUserProfileAsync(userId);

        Assert.Equal(0, result.FollowersCount);
        Assert.Equal(0, result.FollowingCount);
    }

    // EDGE CASE — nullable fields are null in the DTO when not set on entity
    [Fact]
    public async Task GetUserProfileAsync_ReturnsNullForOptionalFields_WhenNotSet()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(id: userId);
        // Bio, City, ProfileImageUrl etc. are all null on the factory default

        _userRepo.Setup(r => r.GetProfileByIdAsync(userId)).ReturnsAsync(user);

        var result = await _sut.GetUserProfileAsync(userId);

        Assert.Null(result.Bio);
        Assert.Null(result.City);
        Assert.Null(result.ProfileImageUrl);
        Assert.Null(result.CoverImageUrl);
        Assert.Null(result.PhoneNumber);
    }

    // EDGE CASE — the correct userId is forwarded to the repository
    // (guards against bugs like passing a hardcoded Guid or the wrong variable)
    [Fact]
    public async Task GetUserProfileAsync_ForwardsExactUserId_ToRepository()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(id: userId);

        _userRepo.Setup(r => r.GetProfileByIdAsync(userId)).ReturnsAsync(user);

        await _sut.GetUserProfileAsync(userId);

        // Times.Once AND the exact id — both matter
        _userRepo.Verify(r => r.GetProfileByIdAsync(userId), Times.Once);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  UpdateUserProfileAsync
    //
    //  What the service actually does:
    //    1. GetByIdAsync(userId)
    //    2. _mapper.Map(request, user)  — merges non-null fields only
    //       because of: opts.Condition((src, dest, srcMember) => srcMember != null)
    //    3. _unitOfWork.User.Update(user)
    //    4. SaveChangesAsync()
    //
    //  AUTHORIZATION NOTE:
    //  The service receives the userId that the controller extracted from the
    //  JWT claim. The service itself has no ownership check — it trusts that
    //  the controller only passes the authenticated user's own id. There is
    //  nothing to test here at the service layer on that front; the right
    //  place to test it is the controller layer.
    // ════════════════════════════════════════════════════════════════════════

    // HAPPY PATH — provided fields are written to the entity
    [Fact]
    public async Task UpdateUserProfileAsync_AppliesProvidedFields_ToEntity()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(id: userId, fullName: "Old Name");

        _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new UpdateUserProfileRequest
        {
            FullName = "Ahmed Mordi",
            Bio = "Engineer",
            City = "Cairo"
        };

        await _sut.UpdateUserProfileAsync(userId, request);

        Assert.Equal("Ahmed Mordi", user.FullName);
        Assert.Equal("Engineer", user.Bio);
        Assert.Equal("Cairo", user.City);

        _userRepo.Verify(r => r.Update(user), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // EDGE CASE — null fields in the request must NOT overwrite existing values
    // This directly tests the ForAllMembers null-condition in UserProfileMappingProfile
    [Fact]
    public async Task UpdateUserProfileAsync_DoesNotOverwrite_WhenRequestFieldIsNull()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(id: userId, fullName: "Existing Name");
        user.City = "Cairo";
        user.PhoneNumber = "+201001234567";
        user.Bio = "Existing bio";

        _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Only Country is set; every other field is null
        var request = new UpdateUserProfileRequest { Country = "Egypt" };

        await _sut.UpdateUserProfileAsync(userId, request);

        Assert.Equal("Existing Name", user.FullName);
        Assert.Equal("Cairo", user.City);
        Assert.Equal("+201001234567", user.PhoneNumber);
        Assert.Equal("Existing bio", user.Bio);
        Assert.Equal("Egypt", user.Country);  // only this changed
    }

    // EDGE CASE — fully empty request (all nulls) — nothing changes, but
    // Update() and SaveChangesAsync() are still called (service doesn't short-circuit)
    [Fact]
    public async Task UpdateUserProfileAsync_StillPersists_WhenRequestIsAllNull()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(id: userId, fullName: "Ahmed Mordi");

        _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.UpdateUserProfileAsync(userId, new UpdateUserProfileRequest());

        // Entity unchanged
        Assert.Equal("Ahmed Mordi", user.FullName);

        // But the service still hit Update + Save (it doesn't short-circuit)
        _userRepo.Verify(r => r.Update(user), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  UpdateImageProfileAsync
    //
    //  What the service actually does:
    //    1. _imageValidator.Validate(image)  — throws on invalid file
    //    2. GetByIdAsync(userId)
    //    3. Builds folder = "userImages/{userId}/{profile|cover}"
    //    4. Cloudinary.UpdateAsync(file, folder) → new {Url, PublicId}
    //    5. Writes new url+publicId to the correct fields
    //    6. Update(user) + SaveChangesAsync()
    //    7. If oldPublicId was not null/whitespace → DeleteAsync(oldPublicId)
    //
    //  AUTHORIZATION NOTE:
    //  Same as UpdateUserProfileAsync — the userId comes from the JWT claim
    //  via the controller. The service trusts it. No ownership check to test here.
    // ════════════════════════════════════════════════════════════════════════

    // HAPPY PATH — profile image updated, old one deleted
    [Fact]
    public async Task UpdateImageProfileAsync_UpdatesProfileImage_AndDeletesOldOne()
    {
        var userId = Guid.NewGuid();
        var oldPublicId = "old_profile_abc";
        var user = CreateUser(id: userId,
                              profileImageUrl: "https://old.com/img.jpg",
                              profilePublicId: oldPublicId);

        _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _cloudinary
            .Setup(c => c.UpdateAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync(new ImageUpdatedResponse("https://new.com/img.jpg", "new_profile_xyz"));

        await _sut.UpdateImageProfileAsync(userId,
            new UpdateImageRequest { Image = CreateFormFile() },
            ProfileImageOptions.Profile);

        Assert.Equal("https://new.com/img.jpg", user.ProfileImageUrl);
        Assert.Equal("new_profile_xyz", user.ProfileImagePublicId);

        // Save happens BEFORE delete (service order: Update → Save → Delete)
        _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _cloudinary.Verify(c => c.DeleteAsync(oldPublicId), Times.Once);
    }

    // HAPPY PATH — cover image updated, old one deleted
    [Fact]
    public async Task UpdateImageProfileAsync_UpdatesCoverImage_AndDeletesOldOne()
    {
        var userId = Guid.NewGuid();
        var oldPublicId = "old_cover_abc";
        var user = CreateUser(id: userId,
                              coverImageUrl: "https://old.com/cover.jpg",
                              coverPublicId: oldPublicId);

        _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _cloudinary
            .Setup(c => c.UpdateAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync(new ImageUpdatedResponse("https://new.com/cover.jpg", "new_cover_xyz"));

        await _sut.UpdateImageProfileAsync(userId,
            new UpdateImageRequest { Image = CreateFormFile() },
            ProfileImageOptions.Cover);

        Assert.Equal("https://new.com/cover.jpg", user.CoverImageUrl);
        Assert.Equal("new_cover_xyz", user.CoverImagePublicId);
        _cloudinary.Verify(c => c.DeleteAsync(oldPublicId), Times.Once);
    }

    // EDGE CASE — first-time upload: no old image → DeleteAsync must NOT be called
    [Fact]
    public async Task UpdateImageProfileAsync_DoesNotCallDelete_WhenNoOldImageExists()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(id: userId); // profilePublicId is null

        _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _cloudinary
            .Setup(c => c.UpdateAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync(new ImageUpdatedResponse("https://new.com/img.jpg", "new_id"));

        await _sut.UpdateImageProfileAsync(userId,
            new UpdateImageRequest { Image = CreateFormFile() },
            ProfileImageOptions.Profile);

        _cloudinary.Verify(c => c.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    // EDGE CASE — whitespace publicId (e.g. "   ") treated as "no image"
    [Fact]
    public async Task UpdateImageProfileAsync_DoesNotCallDelete_WhenOldPublicIdIsWhitespace()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(id: userId,
                                profileImageUrl: "https://old.com/img.jpg",
                                profilePublicId: "   "); // whitespace only

        _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _cloudinary
            .Setup(c => c.UpdateAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync(new ImageUpdatedResponse("https://new.com/img.jpg", "new_id"));

        await _sut.UpdateImageProfileAsync(userId,
            new UpdateImageRequest { Image = CreateFormFile() },
            ProfileImageOptions.Profile);

        _cloudinary.Verify(c => c.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    // EDGE CASE — folder path must encode the userId and image type exactly
    // Pins the contract so a refactor can't silently break the Cloudinary path
    [Fact]
    public async Task UpdateImageProfileAsync_BuildsCorrectFolderPath_ForProfileImage()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(id: userId);
        string? capturedFolder = null;

        _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _cloudinary
            .Setup(c => c.UpdateAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .Callback<IFormFile, string>((_, folder) => capturedFolder = folder)
            .ReturnsAsync(new ImageUpdatedResponse("https://url.com", "pub_id"));

        await _sut.UpdateImageProfileAsync(userId,
            new UpdateImageRequest { Image = CreateFormFile() },
            ProfileImageOptions.Profile);

        Assert.Equal($"userImages/{userId}/profile", capturedFolder);
    }

    [Fact]
    public async Task UpdateImageProfileAsync_BuildsCorrectFolderPath_ForCoverImage()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(id: userId);
        string? capturedFolder = null;

        _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _cloudinary
            .Setup(c => c.UpdateAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .Callback<IFormFile, string>((_, folder) => capturedFolder = folder)
            .ReturnsAsync(new ImageUpdatedResponse("https://url.com", "pub_id"));

        await _sut.UpdateImageProfileAsync(userId,
            new UpdateImageRequest { Image = CreateFormFile() },
            ProfileImageOptions.Cover);

        Assert.Equal($"userImages/{userId}/cover", capturedFolder);
    }

    // EDGE CASE — validation runs first; if it throws, nothing else is touched
    [Fact]
    public async Task UpdateImageProfileAsync_ThrowsAndTouchesNothing_WhenValidationFails()
    {
        var userId = Guid.NewGuid();

        _imageValidator
            .Setup(v => v.Validate(It.IsAny<IFormFile>()))
            .Throws(new InvalidOperationException("Invalid file type."));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.UpdateImageProfileAsync(userId,
                new UpdateImageRequest { Image = CreateFormFile("virus.exe") },
                ProfileImageOptions.Profile));

        // If validation failed, the repo and Cloudinary must never be reached
        _userRepo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _cloudinary.Verify(c => c.UpdateAsync(
            It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    // EDGE CASE — cover image type must NOT touch profile fields and vice versa
    [Fact]
    public async Task UpdateImageProfileAsync_DoesNotTouchProfileFields_WhenUpdatingCover()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(id: userId,
                                profileImageUrl: "https://keep.com/profile.jpg",
                                profilePublicId: "keep_me");

        _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _cloudinary
            .Setup(c => c.UpdateAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync(new ImageUpdatedResponse("https://new.com/cover.jpg", "new_cover"));

        await _sut.UpdateImageProfileAsync(userId,
            new UpdateImageRequest { Image = CreateFormFile() },
            ProfileImageOptions.Cover);

        // Profile fields untouched
        Assert.Equal("https://keep.com/profile.jpg", user.ProfileImageUrl);
        Assert.Equal("keep_me", user.ProfileImagePublicId);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  DeleteProfileImageAsync
    //
    //  What the service actually does:
    //    1. GetByIdAsync(userId)
    //    2. Reads publicId for the requested image type
    //    3. If null or whitespace → return early (no save, no Cloudinary call)
    //    4. Clears url + publicId on the entity
    //    5. Update(user) + SaveChangesAsync()
    //    6. Cloudinary.DeleteAsync(publicId)
    //
    //  AUTHORIZATION NOTE:
    //  Same pattern — userId is the authenticated user's own id, passed from
    //  the controller. Nothing to test here at the service layer.
    // ════════════════════════════════════════════════════════════════════════

    // HAPPY PATH — profile image cleared and deleted from Cloudinary
    [Fact]
    public async Task DeleteProfileImageAsync_ClearsProfileFields_AndDeletesFromCloudinary()
    {
        var userId = Guid.NewGuid();
        var publicId = "profile_to_delete";
        var user = CreateUser(id: userId,
                              profileImageUrl: "https://cdn.com/img.jpg",
                              profilePublicId: publicId);

        _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.DeleteProfileImageAsync(userId, ProfileImageOptions.Profile);

        Assert.Null(user.ProfileImageUrl);
        Assert.Null(user.ProfileImagePublicId);

        _userRepo.Verify(r => r.Update(user), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _cloudinary.Verify(c => c.DeleteAsync(publicId), Times.Once);
    }

    // HAPPY PATH — cover image cleared and deleted from Cloudinary
    [Fact]
    public async Task DeleteProfileImageAsync_ClearsCoverFields_AndDeletesFromCloudinary()
    {
        var userId = Guid.NewGuid();
        var publicId = "cover_to_delete";
        var user = CreateUser(id: userId,
                              coverImageUrl: "https://cdn.com/cover.jpg",
                              coverPublicId: publicId);

        _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.DeleteProfileImageAsync(userId, ProfileImageOptions.Cover);

        Assert.Null(user.CoverImageUrl);
        Assert.Null(user.CoverImagePublicId);
        _cloudinary.Verify(c => c.DeleteAsync(publicId), Times.Once);
    }

    // EDGE CASE — no image exists → early return, nothing persisted or deleted
    [Fact]
    public async Task DeleteProfileImageAsync_ReturnsEarly_WhenNoImageExists()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(id: userId); // publicId = null

        _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        await _sut.DeleteProfileImageAsync(userId, ProfileImageOptions.Profile);

        _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
        _cloudinary.Verify(c => c.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    // EDGE CASE — whitespace publicId also triggers early return
    [Fact]
    public async Task DeleteProfileImageAsync_ReturnsEarly_WhenPublicIdIsWhitespace()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(id: userId,
                                profileImageUrl: "https://cdn.com/img.jpg",
                                profilePublicId: "   ");

        _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        await _sut.DeleteProfileImageAsync(userId, ProfileImageOptions.Profile);

        _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
        _cloudinary.Verify(c => c.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    // EDGE CASE — deleting cover image must NOT clear profile fields
    [Fact]
    public async Task DeleteProfileImageAsync_DoesNotTouchProfileFields_WhenDeletingCover()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(id: userId,
                                profileImageUrl: "https://cdn.com/profile.jpg",
                                profilePublicId: "keep_profile",
                                coverImageUrl: "https://cdn.com/cover.jpg",
                                coverPublicId: "delete_cover");

        _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.DeleteProfileImageAsync(userId, ProfileImageOptions.Cover);

        // Profile untouched
        Assert.Equal("https://cdn.com/profile.jpg", user.ProfileImageUrl);
        Assert.Equal("keep_profile", user.ProfileImagePublicId);

        // Cover cleared
        Assert.Null(user.CoverImageUrl);
        Assert.Null(user.CoverImagePublicId);
    }

    // EDGE CASE — save happens BEFORE Cloudinary delete
    // If SaveChangesAsync fails, the image should NOT be orphaned in Cloudinary
    // (the service already does this correctly — this test pins the order)
    [Fact]
    public async Task DeleteProfileImageAsync_SavesBeforeCloudinaryDelete()
    {
        var userId = Guid.NewGuid();
        var publicId = "order_test_id";
        var user = CreateUser(id: userId,
                                  profileImageUrl: "https://cdn.com/img.jpg",
                                  profilePublicId: publicId);

        var callOrder = new List<string>();

        _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync())
            .Callback(() => callOrder.Add("save"))
            .ReturnsAsync(1);
        _cloudinary
            .Setup(c => c.DeleteAsync(publicId))
            .Callback(() => callOrder.Add("delete"))
            .Returns(Task.CompletedTask);

        await _sut.DeleteProfileImageAsync(userId, ProfileImageOptions.Profile);

        Assert.Equal(new[] { "save", "delete" }, callOrder);
    }
}