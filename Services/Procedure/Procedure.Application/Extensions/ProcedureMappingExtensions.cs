using Procedure.Application.DTOs;
using Procedure.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Extensions;

public static class ProcedureMappingExtensions
{
    public static ProcedureDto ToDto(this Domain.Entities.Procedure p) =>
        new(
            p.Id,
            p.Title,
            p.Description,
            p.Cost.Amount,
            p.Cost.Currency,
            p.ProfileType,
            p.ProfileTypeId,
            p.EstimatedDurationDays,
            p.IsActive,
            p.Locations.Select(pl => pl.Location.ToDto()).ToList(),
            p.Dependencies.Select(d => d.DependsOnProcedureId).ToList(),
            p.RequiredDocuments.Select(d => d.DocumentTypeId).ToList(),
            p.CreatedAt);

    public static LocationDto ToDto(this Location l) =>
        new(
            l.Id,
            l.Name,
            l.Address.Street,
            l.Address.City,
            l.Address.State,
            l.Address.PostalCode,
            l.Address.Country,
            l.Latitude,
            l.Longitude,
            l.PhoneNumber,
            l.Website,
            l.OpeningHours.Schedule.Select(s => new DayScheduleDto(
                s.Day, s.IsClosed, s.OpenTime, s.CloseTime)).ToList());

    public static UserProcedureDto ToDto(this UserProcedure up) =>
        new(
            up.Id,
            up.ProfileId,
            up.ProcedureId,
            up.Procedure?.Title ?? string.Empty,
            up.Procedure?.Cost.Amount,
            up.Procedure?.Cost.Currency,
            up.Status,
            up.CompletedByUserId,
            up.CompletedByRole,
            up.CompletedAt,
            up.Notes,
            up.CreatedAt);
}
