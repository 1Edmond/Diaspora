using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Domain.Enums;

public enum PaymentStatus
{
    NotApplicable = 0,
    Pending = 1,
    Completed = 2,
    Refunded = 3,
    Failed = 4
}