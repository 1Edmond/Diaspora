using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Domain.Enums;

public enum ListingStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Suspended = 3
}