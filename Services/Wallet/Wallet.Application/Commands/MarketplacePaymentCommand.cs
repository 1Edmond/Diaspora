using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wallet.Application.Commands;

public sealed record MarketplacePaymentCommand(
    Guid BuyerExternalProfileId,
    Guid SellerExternalProfileId,
    Guid ServiceId,
    decimal Amount,
    string Description
) : ICommand<Result>;