using Shared.Application.Models;
using Shared.Domain.Commons;
using Wallet.Application.Dtos;

namespace Wallet.Application.Queries;

public sealed record GetWalletsAdminQuery(
    string? Currency = null,
    bool? IsActive = null,
    int PageNumber = 1,
    int PageSize = 20
) : PagedQuery<Result<PagedList<WalletDto>>>(PageNumber, PageSize);
