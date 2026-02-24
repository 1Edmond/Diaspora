using FluentValidation;
using Procedure.Application.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Validators;

public sealed class StartUserProcedureCommandValidator
    : AbstractValidator<StartUserProcedureCommand>
{
    public StartUserProcedureCommandValidator()
    {
        RuleFor(x => x.ProfileId).NotEmpty();
        RuleFor(x => x.ProcedureId).NotEmpty();
    }
}