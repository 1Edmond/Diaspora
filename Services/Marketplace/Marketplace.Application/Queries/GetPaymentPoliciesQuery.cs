using Marketplace.Application.Dtos;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System.Collections.Generic;

namespace Marketplace.Application.Queries;

public record GetPaymentPoliciesQuery() : IQuery<Result<List<ProfileTypePaymentPolicyDto>>>;
