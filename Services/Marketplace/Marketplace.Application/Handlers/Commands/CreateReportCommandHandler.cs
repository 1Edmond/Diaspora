using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marketplace.Application.Handlers.Commands;

public class CreateReportCommandHandler(
    IReportRepository _reportRepository,
    IServiceListingRepository _listingRepository,
    IReviewRepository _reviewRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result<Guid>> Handle(CreateReportCommand command)
    {
        // Validation de l'existence de la cible quand elle est vérifiable dans ce service.
        // Les cibles de type User sont gérées par le Profile Service — non vérifiables ici.
        switch (command.TargetType)
        {
            case ReportTargetType.Listing:
                var listing = await _listingRepository.GetByIdAsync(command.TargetId);
                if (listing is null)
                    return Result.Failure<Guid>("Listing not found.");
                break;

            case ReportTargetType.Review:
                var review = await _reviewRepository.GetByIdAsync(command.TargetId);
                if (review is null)
                    return Result.Failure<Guid>("Review not found.");
                break;
        }

        // Empêche les signalements en doublon tant qu'un signalement précédent est encore en attente
        var existingPending = await _reportRepository.FindAsync(
            r => r.TargetType == command.TargetType
                 && r.TargetId == command.TargetId
                 && r.ReporterId == command.ReporterId
                 && r.Status == ReportStatus.Pending);

        if (existingPending.Any())
            return Result.Failure<Guid>("You already have a pending report for this target.");

        Report report;
        try
        {
            report = Report.Create(command.TargetType, command.TargetId, command.ReporterId, command.Reason);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<Guid>(ex.Message);
        }

        await _reportRepository.AddAsync(report);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success(report.Id);
    }
}