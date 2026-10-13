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
using System.Collections.Generic;

namespace Procedure.Application.Handlers.Commands;

public class CreateProcedureWithTranslationsCommandHandler(
    IProcedureRepository procedureRepo,
    ITranslationRepository<Procedure.Domain.Entities.Procedure> translationRepo,
    IUnitOfWork uow)
    : CreateWithTranslationsCommandHandler<Procedure.Domain.Entities.Procedure, ProcedureDto>(procedureRepo, translationRepo, uow)
{
    protected override Procedure.Domain.Entities.Procedure CreateEntity(ProcedureDto dto)
    {
        var cost = Money.Create(dto.CostAmount, dto.CostCurrency);
        if (cost.IsFailure)
            throw new InvalidOperationException(cost.Error);

        var result = Procedure.Domain.Entities.Procedure.Create(
            dto.Title,
            dto.Description,
            cost.Value,
            dto.ProfileType,
            dto.ProfileTypeId,
            dto.EstimatedDurationDays);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error);

        return result.Value;
    }

    protected override Dictionary<string, string> GetTranslatableFields(Procedure.Domain.Entities.Procedure entity)
        => new()
        {
            ["Title"] = entity.Title,
            ["Description"] = entity.Description
        };
}