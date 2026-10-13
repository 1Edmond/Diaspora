using Marketplace.Domain.Enums;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Commands;

public record CreateReportCommand(
    ReportTargetType TargetType,
    Guid TargetId,
    Guid ReporterId,
    string Reason
) : ICommand<Result<Guid>>;