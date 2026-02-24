using FluentValidation;
using Procedure.Application.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Validators;

public sealed class UpdateProcedureCommandValidator : AbstractValidator<UpdateProcedureCommand>
{
    public UpdateProcedureCommandValidator()
    {
        RuleFor(x => x.ProcedureId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CostAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CostCurrency).NotEmpty();
    }
}
