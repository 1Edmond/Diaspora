using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Commands;

public record SetWorkerJobPreferenceCommand(
    Guid WorkerId,
    IEnumerable<Guid>? CategoryIds = null,
    string? City = null,
    string? Country = null,
    double? Latitude = null,
    double? Longitude = null,
    int? RadiusKm = null,
    bool? OnlyRemote = null,
    bool? EmailNotifications = null
) : ICommand<Result>;