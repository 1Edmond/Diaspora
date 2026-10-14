using System;

namespace Marketplace.Domain.Enums;

public enum PaymentStatus
{
    NotApplicable = 0,
    Pending = 1,
    Processing = 2,
    Held = 3,
    Released = 4,
    Refunded = 5,
    Failed = 6,
    Completed = Released
}