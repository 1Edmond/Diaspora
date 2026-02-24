using FluentValidation;
using Procedure.Application.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Validators;

public sealed class CreateProcedureCommandValidator : AbstractValidator<CreateProcedureCommand>
{
    public CreateProcedureCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.CostAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CostCurrency).NotEmpty().MaximumLength(10);
        RuleFor(x => x.ProfileType).IsInEnum();
        RuleFor(x => x.ProfileTypeId).NotEmpty();
        RuleFor(x => x.EstimatedDurationDays).GreaterThanOrEqualTo(0);
    }
}
