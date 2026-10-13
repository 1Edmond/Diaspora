using System;
using System.Collections.Generic;
using System.Text;

namespace Freelance.Domain.Enums;

public enum CheckInMethod
{
    None = 0,
    QRCode = 1,
    GeoLocation = 2,
    Manual = 3
}