using FluentValidation;
using Profile.Application.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Validators.Commands;

public sealed class CreateInternalProfileCommandValidator
    : AbstractValidator<CreateInternalProfileCommand>
{
    public CreateInternalProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?[1-9]\d{7,14}$").WithMessage("Invalid phone number format.");

        RuleFor(x => x.InternalProfileTypeId)
            .NotEmpty()
            .WithMessage("InternalProfileTypeId is required.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .LessThan(DateTime.UtcNow.AddYears(-15))
            .WithMessage("User must be at least 15 years old.");
    }
}
