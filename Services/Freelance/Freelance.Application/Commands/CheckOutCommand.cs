using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Commands;

public record CheckOutCommand(
    Guid JobApplicationId,
    Guid WorkerId,
    double? Latitude,
    double? Longitude,
    string? Notes
) : ICommand<Result>;