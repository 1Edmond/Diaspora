using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Domain.Enums;

public enum InternalProfileStatus
{
    PendingVerification = 0,
    Verified = 1,
    Suspended = 2
}