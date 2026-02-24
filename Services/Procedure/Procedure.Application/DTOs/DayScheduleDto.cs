using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.DTOs;

public sealed record DayScheduleDto(
    DayOfWeek Day,
    bool IsClosed,
    TimeOnly? OpenTime,
    TimeOnly? CloseTime
);