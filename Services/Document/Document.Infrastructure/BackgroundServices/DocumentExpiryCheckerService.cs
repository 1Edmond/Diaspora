using Document.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Infrastructure.BackgroundServices;

public sealed class DocumentExpiryCheckerService(
    IServiceProvider serviceProvider,
    ILogger<DocumentExpiryCheckerService> logger) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("DocumentExpiryCheckerService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckDocumentsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in DocumentExpiryCheckerService.");
            }

            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        logger.LogInformation("DocumentExpiryCheckerService stopped.");
    }

    private async Task CheckDocumentsAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var documentRepository = scope.ServiceProvider
            .GetRequiredService<IDocumentRepository>();
        var documentTypeRepository = scope.ServiceProvider
            .GetRequiredService<IDocumentTypeRepository>();
        var unitOfWork = scope.ServiceProvider
            .GetRequiredService<IUnitOfWork>();

        // ── 1. Documents expirés ───────────────────────────────────────────────
        var expiredDocuments = await documentRepository
            .GetExpiredActiveDocumentsAsync(cancellationToken);

        foreach (var document in expiredDocuments)
        {
            var result = document.MarkAsExpired();
            if (result.IsSuccess)
            {
                await documentRepository.UpdateAsync(document, cancellationToken);
                logger.LogInformation(
                    "Document {DocumentId} marked as expired.", document.Id);
            }
        }

        // ── 2. Documents proches de l'expiration ──────────────────────────────
        var activeDocumentTypes = await documentTypeRepository
            .GetAllActiveAsync(cancellationToken);

        foreach (var docType in activeDocumentTypes.Where(t => t.IsRenewable))
        {
            var threshold = DateTime.UtcNow.AddDays(docType.RenewalAlertDays);

            var expiringDocuments = await documentRepository
                .GetExpiringDocumentsAsync(threshold, cancellationToken);

            foreach (var document in expiringDocuments
                .Where(d => d.DocumentTypeId == docType.Id))
            {
                var daysRemaining = (int)(document.ExpiresAt!.Value - DateTime.UtcNow).TotalDays;
                var result = document.SendRenewalAlert(daysRemaining);

                if (result.IsSuccess)
                {
                    await documentRepository.UpdateAsync(document, cancellationToken);
                    logger.LogInformation(
                        "Renewal alert sent for Document {DocumentId} ({DaysRemaining} days remaining).",
                        document.Id, daysRemaining);
                }
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
