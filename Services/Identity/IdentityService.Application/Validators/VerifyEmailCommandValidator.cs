using FluentValidation;
using IdentityService.Application.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Validators;

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty();

        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(6);
    }
}