using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using Wallet.Application.Dtos;

namespace Wallet.Application.Queries;

public sealed record GetPlatformRevenueQuery : IQuery<Result<PlatformRevenueDto>>;
