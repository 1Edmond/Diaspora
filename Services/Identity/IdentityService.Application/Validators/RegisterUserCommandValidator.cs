using FluentValidation;
using IdentityService.Application.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Validators;

public class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(4);





        RuleForEach(x => x.Role)
            .NotEmpty()
            .When(x => x.Role != null);

        RuleFor(x => x.FirstName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.LastName));
    }
}
