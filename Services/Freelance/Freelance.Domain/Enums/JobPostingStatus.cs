using System;
using System.Collections.Generic;
using System.Text;

namespace Freelance.Domain.Enums;

public enum JobPostingStatus
{
    Draft = 0,
    Open = 1,
    RegistrationClosed = 2,
    InProgress = 3,
    Completed = 4,
    Cancelled = 5
}