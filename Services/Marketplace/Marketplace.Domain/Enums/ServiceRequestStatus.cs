using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Domain.Enums;

public enum ServiceRequestStatus
{
    Pending = 0,
    Accepted = 1,
    Declined = 2,
    Completed = 3,
    Cancelled = 4
}