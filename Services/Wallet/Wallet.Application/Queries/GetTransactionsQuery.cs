using Shared.Application.Abstractions;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Application.Dtos;
using Wallet.Domain.Enums;

namespace Wallet.Application.Queries;

public sealed record GetTransactionsQuery(
    Guid ExternalProfileId,
    int PageNumber = 1,
    int PageSize = 20,
    TransactionType? Type = null,
    TransactionStatus? Status = null
) : PagedQuery<Result<PagedList<TransactionDto>>>(PageNumber, PageSize);