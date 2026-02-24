using FluentValidation;
using Procedure.Application.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Validators;

public sealed class CompleteUserProcedureCommandValidator
    : AbstractValidator<CompleteUserProcedureCommand>
{
    public CompleteUserProcedureCommandValidator()
    {
        RuleFor(x => x.UserProcedureId).NotEmpty();
        RuleFor(x => x.CompletedByUserId).NotEmpty();
        RuleFor(x => x.CompletedByRole).NotEmpty();
    }
}