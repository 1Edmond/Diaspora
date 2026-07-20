using System;
using System.Collections.Generic;
using System.Text;

namespace Wallet.Domain.Enums;

public enum TransactionDirection
{
    Credit = 0, // Money coming into the wallet
    Debit = 1   // Money going out of the wallet
}
