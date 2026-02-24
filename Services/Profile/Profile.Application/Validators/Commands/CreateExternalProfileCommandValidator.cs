using FluentValidation;
using Profile.Application.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Validators.Commands;

public sealed class CreateExternalProfileCommandValidator
    : AbstractValidator<CreateExternalProfileCommand>
{
    public CreateExternalProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ExternalProfileTypeId).NotEmpty();
        RuleFor(x => x.Kind).IsInEnum();
    }
}
