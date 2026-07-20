using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Application.Dtos;

namespace Wallet.Application.Queries;

public sealed record GetWalletByProfileQuery(Guid ExternalProfileId)
    : IQuery<Result<WalletDto>>;