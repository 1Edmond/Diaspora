using Document.Application.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Application.Validators.Commands;

public sealed class ValidateDocumentCommandValidator : AbstractValidator<ValidateDocumentCommand>
{
    public ValidateDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentId).NotEmpty();
        RuleFor(x => x.AdminId).NotEmpty();
    }
}