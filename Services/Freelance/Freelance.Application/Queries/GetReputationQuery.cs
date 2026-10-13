using Freelance.Application.Dtos;
using Freelance.Domain.Enums;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Queries;

public record GetReputationQuery(Guid SubjectId, ReputationRole Role) : IQuery<Result<ReputationDto>>;