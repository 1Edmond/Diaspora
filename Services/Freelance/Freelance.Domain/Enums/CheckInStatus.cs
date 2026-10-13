using System;
using System.Collections.Generic;
using System.Text;

namespace Freelance.Domain.Enums;

public enum CheckInStatus
{
    Pending = 0,
    CheckedIn = 1,
    CheckedOut = 2,
    NoShow = 3
}