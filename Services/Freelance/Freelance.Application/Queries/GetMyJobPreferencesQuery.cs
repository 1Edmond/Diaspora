using Freelance.Application.Dtos;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Queries;

public record GetMyJobPreferencesQuery(Guid WorkerId) : IQuery<Result<WorkerJobPreferenceDto>>;