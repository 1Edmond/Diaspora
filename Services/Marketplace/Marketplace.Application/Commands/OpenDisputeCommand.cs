using System;
using System.Collections.Generic;

namespace Marketplace.Application.Commands;

public record OpenDisputeCommand(
    Guid ServiceRequestId,
    Guid InitiatorUserId,
    string Reason,
    string Description,
    List<string>? EvidenceUrls = null
);
