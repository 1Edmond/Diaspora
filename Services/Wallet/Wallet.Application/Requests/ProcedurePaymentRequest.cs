using System;
using System.Collections.Generic;
using System.Text;

namespace Wallet.Application.Requests;

public sealed record ProcedurePaymentRequest(
    Guid ExternalProfileId,
    Guid ProcedureId,
    decimal Amount,
    string Description
);
