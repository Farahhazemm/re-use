//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using FluentValidation;

//namespace ReUse.Application.DTOs.Users.UserProfile.Commands;


//public class UpdateUserProfileValidator : AbstractValidator<UpdateUserProfileCommand>
//{
//    public UpdateUserProfileValidator()
//    {
//        RuleFor(x => x.FullName)
//            .MaximumLength(100)
//            .WithMessage("Full name must not exceed 100 characters.")
//            .Matches(@"^[a-zA-Z\s'-]+$")
//            .WithMessage("Full name can only contain letters, spaces, hyphens, and apostrophes.")
//            .When(x => x.FullName is not null);

//        RuleFor(x => x.PhoneNumber)
//            .Matches(@"^\+?[1-9]\d{6,14}$")
//            .WithMessage("Phone number must be a valid international format (e.g. +201234567890).")
//            .When(x => x.PhoneNumber is not null);

//        RuleFor(x => x.Bio)
//            .MaximumLength(500)
//            .WithMessage("Bio must not exceed 500 characters.")
//            .When(x => x.Bio is not null);

//        RuleFor(x => x.AddressLine1)
//            .MaximumLength(200)
//            .WithMessage("Address line must not exceed 200 characters.")
//            .When(x => x.AddressLine1 is not null);

//        RuleFor(x => x.City)
//            .MaximumLength(100)
//            .WithMessage("City must not exceed 100 characters.")
//            .Matches(@"^[a-zA-Z\s\-]+$")
//            .WithMessage("City can only contain letters, spaces, and hyphens.")
//            .When(x => x.City is not null);

//        RuleFor(x => x.StateProvince)
//            .MaximumLength(100)
//            .WithMessage("State/Province must not exceed 100 characters.")
//            .When(x => x.StateProvince is not null);

//        RuleFor(x => x.PostalCode)
//            .MaximumLength(20)
//            .WithMessage("Postal code must not exceed 20 characters.")
//            .Matches(@"^[a-zA-Z0-9\s\-]+$")
//            .WithMessage("Postal code can only contain letters, numbers, spaces, and hyphens.")
//            .When(x => x.PostalCode is not null);

//        RuleFor(x => x.Country)
//            .MaximumLength(100)
//            .WithMessage("Country must not exceed 100 characters.")
//            .Matches(@"^[a-zA-Z\s\-]+$")
//            .WithMessage("Country can only contain letters, spaces, and hyphens.")
//            .When(x => x.Country is not null);
//    }
//}