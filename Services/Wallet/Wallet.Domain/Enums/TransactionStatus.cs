using System;
using System.Collections.Generic;
using System.Text;

namespace Wallet.Domain.Enums;

public enum TransactionStatus
{
    Pending = 0,   // Transaction is initiated but not yet completed
    Completed = 1, // Transaction has been successfully completed
    Failed = 2     // Transaction failed due to an error or insufficient funds
}
