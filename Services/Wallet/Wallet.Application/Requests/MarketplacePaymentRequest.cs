using System;
using System.Collections.Generic;
using System.Text;

namespace Wallet.Application.Requests;

public sealed record MarketplacePaymentRequest(
    Guid BuyerExternalProfileId,
    Guid SellerExternalProfileId,
    Guid ServiceId,
    decimal Amount,
    string Description
);