using Freelance.Application.Dtos;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Freelance.Application.Queries;

public record GetJobApplicationByIdQuery(Guid Id, Guid RequestedBy) : IQuery<Result<JobApplicationDto>>;