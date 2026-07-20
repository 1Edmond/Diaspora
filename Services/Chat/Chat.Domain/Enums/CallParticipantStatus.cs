using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Domain.Enums;

public enum CallParticipantStatus
{
    Invited = 0,
    Joined = 1,
    Left = 2,
    Declined = 3
}