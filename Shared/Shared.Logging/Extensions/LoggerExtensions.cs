using Shared.Logging.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Logging.Extensions;

public static class LoggerExtensions
{
    public static void LogBusinessEvent(
        this ILogger logger,
        string eventType,
        string eventCategory,
        string message,
        Dictionary<string, object>? data = null)
    {
        var businessEvent = new BusinessLogEvent
        {
            EventType = eventType,
            EventCategory = eventCategory,
            Message = message,
            Data = data ?? new Dictionary<string, object>(),
            OccurredAt = DateTime.UtcNow
        };

        logger.LogInformation(
            "[BusinessEvent] {EventType} - {EventCategory} - {Message} {@BusinessEvent}",
            eventType,
            eventCategory,
            message,
            businessEvent);
    }

    public static void LogAudit(
        this ILogger logger,
        string action,
        string entityType,
        string entityId,
        string userId,
        string userName,
        string? oldValue = null,
        string? newValue = null,
        string? ipAddress = null)
    {
        var auditEvent = new AuditLogEvent
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            UserId = userId,
            UserName = userName,
            OldValue = oldValue,
            NewValue = newValue,
            PerformedAt = DateTime.UtcNow,
            IpAddress = ipAddress ?? "Unknown"
        };

        logger.LogWarning(
            "[AUDIT] {Action} on {EntityType} ({EntityId}) by {UserName} {@AuditEvent}",
            action,
            entityType,
            entityId,
            userName,
            auditEvent);
    }

    public static void LogPerformance(
        this ILogger logger,
        string operationName,
        long durationMs,
        Dictionary<string, object>? metrics = null)
    {
        var performanceEvent = new PerformanceLogEvent
        {
            OperationName = operationName,
            DurationMs = durationMs,
            Metrics = metrics ?? new Dictionary<string, object>(),
            StartedAt = DateTime.UtcNow.AddMilliseconds(-durationMs),
            CompletedAt = DateTime.UtcNow
        };

        var logLevel = performanceEvent.IsSlowOperation ? LogLevel.Warning : LogLevel.Information;

        logger.Log(
            logLevel,
            "[Performance] {OperationName} completed in {DurationMs}ms {@PerformanceEvent}",
            operationName,
            durationMs,
            performanceEvent);
    }

    public static void LogStructuredError(
        this ILogger logger,
        Exception exception,
        string errorCode,
        Dictionary<string, object>? context = null)
    {
        var errorEvent = new ErrorLogEvent
        {
            ErrorCode = errorCode,
            ErrorMessage = exception.Message,
            StackTrace = exception.StackTrace,
            InnerException = exception.InnerException?.Message,
            Context = context ?? new Dictionary<string, object>(),
            OccurredAt = DateTime.UtcNow
        };

        logger.LogError(
            exception,
            "[ERROR:{ErrorCode}] {ErrorMessage} {@ErrorEvent}",
            errorCode,
            exception.Message,
            errorEvent);
    }

    public static void LogDonationCompleted(
        this ILogger logger,
        string donationId,
        string donorId,
        string bloodType,
        int volumeInMl)
    {
        logger.LogBusinessEvent(
            "DonationCompleted",
            "Donation",
            $"Donation {donationId} completed successfully",
            new Dictionary<string, object>
            {
                { "DonationId", donationId },
                { "DonorId", donorId },
                { "BloodType", bloodType },
                { "VolumeInMl", volumeInMl }
            });
    }

    public static void LogLabTestCompleted(
        this ILogger logger,
        string testId,
        string donationId,
        bool isApproved)
    {
        logger.LogBusinessEvent(
            "LabTestCompleted",
            "LabTest",
            $"Lab test {testId} completed with result: {(isApproved ? "APPROVED" : "REJECTED")}",
            new Dictionary<string, object>
            {
                { "TestId", testId },
                { "DonationId", donationId },
                { "IsApproved", isApproved }
            });
    }

    public static void LogLowBloodInventory(
        this ILogger logger,
        string bloodType,
        int currentStock,
        int threshold)
    {
        logger.LogWarning(
            "[ALERT] Low inventory for {BloodType}: {CurrentStock} units (threshold: {Threshold})",
            bloodType,
            currentStock,
            threshold);
    }

    public static void LogSensitiveDataAccess(
        this ILogger logger,
        string userId,
        string userName,
        string dataType,
        string entityId,
        string ipAddress)
    {
        logger.LogAudit(
            "SensitiveDataAccess",
            dataType,
            entityId,
            userId,
            userName,
            null,
            null,
            ipAddress);
    }
}