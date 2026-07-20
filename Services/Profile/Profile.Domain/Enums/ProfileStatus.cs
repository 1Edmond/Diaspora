using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Domain.Enums;

public enum ProfileStatus
{
    PendingVerification = 0,
    Verified = 1,
    Suspended = 2,
    Activate = 3,
}