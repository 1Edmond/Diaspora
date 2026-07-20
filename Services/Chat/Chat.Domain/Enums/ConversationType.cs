using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Domain.Enums;

public enum ConversationType
{
    DirectMessage = 0,
    Group = 1,
    Community = 2,
    Broadcast = 3
}