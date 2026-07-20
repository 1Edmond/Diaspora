using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Application.Requests;

public sealed record CreateDocumentTypeRequest(
    Guid ProfileTypeId,
    string Name,
    string Code,
    string? Description,
    bool IsRenewable,
    int RenewalAlertDays,
    bool IsRequired
);
