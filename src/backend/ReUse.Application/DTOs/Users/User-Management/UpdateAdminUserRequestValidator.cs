using FluentValidation;

using ReUse.Application.Enums;

namespace ReUse.Application.DTOs.Users.User_Management;

public class UpdateAdminUserRequestValidator : AbstractValidator<UpdateAdminUserRequest>
{
    public UpdateAdminUserRequestValidator()
    {
        RuleFor(x => x.FullName)
            .MaximumLength(100)
            .Matches(@"^[a-zA-Z\s'-]+$")
            .WithMessage("Full name can only contain letters, spaces, hyphens, and apostrophes.")
            .When(x => x.FullName is not null);

        RuleFor(x => x.PhoneNumber)
            .Must(BeValidPhone!)
            .WithMessage("Phone number must be valid (Egyptian local or international format).")
            .When(x => x.PhoneNumber is not null);

        RuleFor(x => x.Bio)
            .MaximumLength(500)
            .When(x => x.Bio is not null);

        RuleFor(x => x.AddressLine1)
            .MaximumLength(200)
            .When(x => x.AddressLine1 is not null);

        RuleFor(x => x.City)
            .MaximumLength(100)
            .Matches(@"^[a-zA-Z\s\-]+$")
            .WithMessage("City can only contain letters, spaces, and hyphens.")
            .When(x => x.City is not null);

        RuleFor(x => x.StateProvince)
            .MaximumLength(100)
            .When(x => x.StateProvince is not null);

        RuleFor(x => x.PostalCode)
            .MaximumLength(20)
            .Matches(@"^[a-zA-Z0-9\s\-]+$")
            .WithMessage("Postal code can only contain letters, numbers, spaces, and hyphens.")
            .When(x => x.PostalCode is not null);

        RuleFor(x => x.Country)
            .MaximumLength(100)
            .Matches(@"^[a-zA-Z\s\-]+$")
            .WithMessage("Country can only contain letters, spaces, and hyphens.")
            .When(x => x.Country is not null);

        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage($"Role must be one of: {string.Join(", ", Enum.GetNames<UserRole>())}.")
            .When(x => x.Role.HasValue);
    }

    private static bool BeValidPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return false;
        phone = phone.Trim();
        if (phone.StartsWith("01"))
            return phone.Length == 11 && phone.All(char.IsDigit);
        if (phone.StartsWith("+"))
        {
            var withoutPlus = phone[1..];
            return withoutPlus.Length >= 8 && withoutPlus.Length <= 15 && withoutPlus.All(char.IsDigit);
        }
        return false;
    }
}