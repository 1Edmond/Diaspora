using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Wallet;

public sealed record WalletCreatedIntegrationEvent(
    Guid WalletId,
    Guid ExternalProfileId,
    Guid UserId,
    string Currency
) : IntegrationEvent;