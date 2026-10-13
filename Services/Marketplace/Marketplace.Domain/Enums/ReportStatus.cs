using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Domain.Enums;

public enum ReportStatus
{
    Pending = 0,
    Reviewed = 1,
    Dismissed = 2,
    ActionTaken = 3
}