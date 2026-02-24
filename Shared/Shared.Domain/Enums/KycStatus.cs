using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Domain.Enums;

public enum KYCStatus
{
    Draft = 1,
    Pending = 2,
    UnderReview = 3,
    MoreInfoRequired = 4 ,
    Approved = 5,
    Rejected = 6,
    Expired = 7,
    RequiresUpdate = 8
}
