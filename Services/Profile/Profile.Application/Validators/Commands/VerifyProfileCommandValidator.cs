using FluentValidation;
using Profile.Application.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Validators.Commands;

public sealed class VerifyProfileCommandValidator
    : AbstractValidator<VerifyProfileCommand>
{
    public VerifyProfileCommandValidator()
    {
        RuleFor(x => x.ProfileId).NotEmpty();
        RuleFor(x => x.AdminId).NotEmpty();
    }
}