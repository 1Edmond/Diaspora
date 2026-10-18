using Marketplace.Application.Dtos;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Marketplace.Application.Queries;

public record ResolvePaymentPolicyQuery(Guid? ProfileTypeId = null, string? CountryCode = null)
    : IQuery<Result<ProfileTypePaymentPolicyDto>>;
