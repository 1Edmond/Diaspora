using Marketplace.Application.Dtos;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Marketplace.Application.Queries;

public record GetPaymentPolicyByProfileTypeQuery(Guid ProfileTypeId) : IQuery<Result<ProfileTypePaymentPolicyDto>>;
