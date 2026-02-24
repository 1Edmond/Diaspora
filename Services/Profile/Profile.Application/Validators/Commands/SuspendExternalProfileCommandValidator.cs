using FluentValidation;
using Profile.Application.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Validators.Commands;

public sealed class SuspendExternalProfileCommandValidator
    : AbstractValidator<SuspendExternalProfileCommand>
{
    public SuspendExternalProfileCommandValidator()
    {
        RuleFor(x => x.ExternalProfileId).NotEmpty();
    }
}
