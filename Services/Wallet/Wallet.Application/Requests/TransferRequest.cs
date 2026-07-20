using System;
using System.Collections.Generic;
using System.Text;

namespace Wallet.Application.Requests;

public sealed record TransferRequest(
    Guid SenderExternalProfileId,
    Guid ReceiverExternalProfileId,
    decimal Amount,
    string Description
);