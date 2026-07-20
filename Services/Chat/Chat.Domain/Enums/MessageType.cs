using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Domain.Enums;

public enum MessageType
{
    Text = 0,
    Image = 1,
    File = 2,
    Voice = 3,
    System = 4
}