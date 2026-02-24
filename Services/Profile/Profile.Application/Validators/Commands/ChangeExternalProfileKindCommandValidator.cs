using FluentValidation;
using Profile.Application.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Validators.Commands;

public sealed class ChangeExternalProfileKindCommandValidator
    : AbstractValidator<ChangeExternalProfileKindCommand>
{
    public ChangeExternalProfileKindCommandValidator()
    {
        RuleFor(x => x.ExternalProfileId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.NewKind).IsInEnum();
    }
}