using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using Wallet.Application.Dtos;
using Wallet.Domain.Enums;

namespace Wallet.Application.Queries;

public sealed record GetTransactionsAdminQuery(
    Guid? WalletId = null,
    TransactionType? Type = null,
    TransactionStatus? Status = null,
    string? Currency = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? Search = null
) : PagedQuery<Result<PagedList<TransactionDto>>>(PageNumber, PageSize);
