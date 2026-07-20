using System;
using System.Collections.Generic;
using System.Text;

namespace Wallet.Application.Responses;

public sealed record TransferResponse(
    Guid TransactionId,
    decimal AmountSent,
    decimal Fee,
    decimal SenderNewBalance
);