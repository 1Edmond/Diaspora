using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Application.DTOs;

public sealed record DocumentTypeDto(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    bool IsRenewable,
    int RenewalAlertDays,
    bool IsRequired,
    bool IsActive,
    Guid ProfileTypeId = default
);