using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.DTOs;

public sealed record ProcedureProgressDto(
    Guid ProfileId,
    int Total,
    int Completed,
    int InProgress,
    int NotStarted,
    int Skipped,
    double CompletionPercentage
);
