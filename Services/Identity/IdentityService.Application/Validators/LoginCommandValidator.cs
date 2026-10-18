using FluentValidation;
using IdentityService.Application.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Validators;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}
