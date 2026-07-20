using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Requests;

public sealed record DayScheduleRequest(
    DayOfWeek Day,
    bool IsClosed,
    TimeOnly? OpenTime,
    TimeOnly? CloseTime
);