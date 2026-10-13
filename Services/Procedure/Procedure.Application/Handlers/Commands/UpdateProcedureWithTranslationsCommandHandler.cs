using Procedure.Application.Commands;
using Procedure.Application.DTOs;
using Procedure.Application.Interfaces;
using Procedure.Domain.Entities;
using Procedure.Domain.Enums;
using Shared.Application.Abstractions;
using Shared.Application.Handlers.Commands;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using Shared.Domain.ValueObjects;
using System;

namespace Procedure.Application.Handlers.Commands;

public class UpdateProcedureWithTranslationsCommandHandler(
    IProcedureRepository procedureRepo,
    ITranslationRepository<Procedure.Domain.Entities.Procedure> translationRepo,
    IUnitOfWork uow)
    : UpdateWithTranslationsCommandHandler<Procedure.Domain.Entities.Procedure, ProcedureDto>(procedureRepo, translationRepo, uow)
{
    protected override void UpdateEntity(Procedure.Domain.Entities.Procedure entity, ProcedureDto dto)
    {
        var cost = Money.Create(dto.CostAmount, dto.CostCurrency);
        if (cost.IsFailure)
            throw new InvalidOperationException(cost.Error);

        entity.Update(
            dto.Title,
            dto.Description,
            cost.Value,
            dto.EstimatedDurationDays);
    }
}