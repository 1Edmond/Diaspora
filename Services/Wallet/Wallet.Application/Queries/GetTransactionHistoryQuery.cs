using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using Wallet.Application.Dtos;
using Wallet.Domain.Enums;

namespace Wallet.Application.Queries;

public sealed record GetTransactionHistoryQuery(
    Guid ExternalProfileId,
    int PageNumber = 1,
    int PageSize = 20,
    TransactionType? Type = null,
    TransactionStatus? Status = null,
    string? Search = null
) : PagedQuery<Result<PagedList<TransactionDto>>>(PageNumber, PageSize);
