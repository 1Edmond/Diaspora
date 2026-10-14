using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events.Marketplace;
using System;
using System.Collections.Generic;
using System.Text;
using Wolverine;

namespace Marketplace.Application.Handlers.Commands;

public class ResolveReportCommandHandler(
    IReportRepository _reportRepository,
    IUnitOfWork _unitOfWork,
    IMessageBus? _messageBus = null)
{
    public async Task<Result> Handle(ResolveReportCommand command)
    {
        var report = await _reportRepository.GetByIdAsync(command.ReportId);
        if (report is null)
            return Result.Failure("Report not found.");

        try
        {
            report.Resolve(command.ReviewedBy, command.NewStatus, command.ResolutionNotes);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        await _reportRepository.UpdateAsync(report);
        await _unitOfWork.SaveChangesAsync();

        if (_messageBus is not null)
        {
            await _messageBus.PublishAsync(new ReportResolvedIntegrationEvent(
                report.Id,
                report.ReporterId,
                report.TargetType.ToString(),
                report.TargetId,
                command.ResolutionNotes ?? string.Empty));
        }

        return Result.Success();
    }
}