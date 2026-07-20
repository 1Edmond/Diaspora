using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Domain.Enums;

public enum CallStatus
{
    Ringing = 0,
    Active = 1,
    Ended = 2,
    Missed = 3,
    Rejected = 4
}