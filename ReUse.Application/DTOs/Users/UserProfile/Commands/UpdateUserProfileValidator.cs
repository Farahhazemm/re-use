using FluentValidation;

namespace ReUse.Application.DTOs.Users.UserProfile.Commands;

public class UpdateUserProfileValidator : AbstractValidator<UpdateUserProfile>
{
    public UpdateUserProfileValidator()
    {
        // FullName
        RuleFor(x => x.FullName)
            .MaximumLength(100).WithMessage("Full name must not exceed 100 characters.");

        // PhoneNumber 
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[0-9]{7,15}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Invalid phone number format.");

        // Bio
        RuleFor(x => x.Bio)
            .MaximumLength(500)
            .WithMessage("Bio must not exceed 500 characters.");

        // Address
        RuleFor(x => x.AddressLine1)
            .MaximumLength(200);

        RuleFor(x => x.City)
            .MaximumLength(100);

        RuleFor(x => x.StateProvince)
            .MaximumLength(100);

        RuleFor(x => x.PostalCode)
            .MaximumLength(20);

        RuleFor(x => x.Country)
            .MaximumLength(100);
    }
}