using Chat.Application.Commands;
using Chat.Domain.Enums;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Validators.Commands;

public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    private static readonly string[] AllowedMimeTypes =
        ["image/jpeg", "image/png", "image/webp", "image/gif",
         "application/pdf", "audio/mpeg", "audio/ogg", "audio/webm",
         "application/msword",
         "application/vnd.openxmlformats-officedocument.wordprocessingml.document"];

    private const long MaxFileSizeBytes = 25 * 1024 * 1024; // 25 MB

    public SendMessageCommandValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.SenderChatProfileId).NotEmpty();

        RuleFor(x => x.Content)
            .NotEmpty().MaximumLength(4000)
            .When(x => x.Type == MessageType.Text);

        RuleFor(x => x.Attachment)
            .NotNull().When(x => x.Type is MessageType.Image or MessageType.File or MessageType.Voice)
            .WithMessage("Attachment is required for Image/File/Voice messages.")
            .Must(f => f!.Length <= MaxFileSizeBytes)
            .When(x => x.Attachment != null)
            .WithMessage("File size must not exceed 25 MB.")
            .Must(f => AllowedMimeTypes.Contains(f!.ContentType))
            .When(x => x.Attachment != null)
            .WithMessage("File type not allowed.");
    }
}