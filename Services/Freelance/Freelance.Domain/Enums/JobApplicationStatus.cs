using System;
using System.Collections.Generic;
using System.Text;

namespace Freelance.Domain.Enums;

public enum JobApplicationStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2,
    Withdrawn = 3,
    Completed = 4,
    NoShow = 5
}