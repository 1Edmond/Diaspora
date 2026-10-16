using KYC.Application.Dtos;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace KYC.Application.Queries;

public sealed record GetKycComplianceQuery(
    Guid UserId,
    Guid? ProfileTypeId = null
) : IQuery<Result<KycComplianceDto>>;
