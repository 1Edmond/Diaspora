using FluentValidation;
using Profile.Application.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Validators.Commands;

public sealed class CreateProfileCommandValidator
    : AbstractValidator<CreateProfileCommand>
{
    public CreateProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ProfileTypeId).NotEmpty();
        RuleFor(x => x.Kind).IsInEnum();
    }
}
