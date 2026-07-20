using System;
using System.Collections.Generic;
using System.Text;

namespace Wallet.Domain.Enums;

public enum TransactionType
{
    Deposit = 0,
    TransferOut = 1,
    TransferIn = 2,
    TransferFee = 3,
    ProcedurePayment = 4,
    MarketplacePayment = 5,
    MarketplaceReceipt = 6,
    Refund = 7
}
