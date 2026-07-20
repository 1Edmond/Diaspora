using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Notification.Application.Abstractions;
using Notification.Application.Models;
using Notification.Domain.Enums;
using MailKit.Net.Smtp;
using MailKit.Security;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Notification.Infrastructure.NotificationSenders;

public class EmailNotificationSender : INotificationSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailNotificationSender> _logger;

    public EmailNotificationSender(
        IConfiguration configuration,
        ILogger<EmailNotificationSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public NotificationChannel Channel => NotificationChannel.Email;

    public async Task<Result> SendAsync(NotificationContext context)
    {
        var payload = ExtractPayload(context);
        if (payload is null)
        {
            _logger.LogError(
                "[EmailNotificationSender] Invalid or missing payload for message {MessageId} (event: {EventType})",
                context.MessageId, context.EventType);
            return Result.Failure("Invalid email payload");
        }

        if (string.IsNullOrWhiteSpace(payload.To))
        {
            _logger.LogError(
                "[EmailNotificationSender] Missing recipient for message {MessageId}", context.MessageId);
            return Result.Failure("Missing email recipient");
        }

        try
        {
            var smtpSettings = _configuration.GetSection("Smtp");
            var host = smtpSettings["Host"];
            var port = smtpSettings.GetValue<int>("Port");
            var username = smtpSettings["Username"];
            var password = smtpSettings["Password"];
            var fromEmail = smtpSettings["FromEmail"];
            var fromName = smtpSettings["FromName"] ?? "Notifications";
            var useSsl = smtpSettings.GetValue<bool>("UseSsl", true);

            if (string.IsNullOrWhiteSpace(host))
            {
                _logger.LogError("[EmailNotificationSender] SMTP host is not configured");
                return Result.Failure("SMTP configuration missing");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(MailboxAddress.Parse(payload.To));

            if (!string.IsNullOrWhiteSpace(payload.Cc))
                message.Cc.Add(MailboxAddress.Parse(payload.Cc));

            message.Subject = payload.Subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = payload.Body };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            await client.ConnectAsync(
                host,
                port,
                useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);

            if (!string.IsNullOrWhiteSpace(username))
                await client.AuthenticateAsync(username, password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation(
                    "[EmailNotificationSender] Email sent for message {MessageId} to {Recipient} — event: {EventType}",
                    context.MessageId, payload.To, context.EventType);

            return Result.Success("Email sent");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[EmailNotificationSender] Failed to send email for message {MessageId} to {Recipient}",
                context.MessageId, payload.To);
            return Result.Failure($"Failed to send email: {ex.Message}");
        }
    }

    private EmailNotificationPayload? ExtractPayload(NotificationContext context)
    {
        try
        {
            return context.Payload switch
            {
                EmailNotificationPayload typed => typed,
                JsonElement json => json.Deserialize<EmailNotificationPayload>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }),
                string json => JsonSerializer.Deserialize<EmailNotificationPayload>(
                    json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }),
                _ => JsonSerializer.Deserialize<EmailNotificationPayload>(
                    JsonSerializer.Serialize(context.Payload),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            };
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex,
                "[EmailNotificationSender] Failed to deserialize payload for message {MessageId}",
                context.MessageId);
            return null;
        }
    }
}