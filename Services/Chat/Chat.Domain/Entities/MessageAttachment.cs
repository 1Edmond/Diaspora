using Chat.Domain.Enums;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Domain.Entities;

public sealed class MessageAttachment : Entity
{
    public Guid MessageId { get; private set; }
    public AttachmentType Type { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FilePath { get; private set; } = string.Empty;
    public string MimeType { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public int? DurationMs { get; private set; }  // pour Voice
    public int? Width { get; private set; }  // pour Image
    public int? Height { get; private set; }  // pour Image

    private MessageAttachment() { }

    public MessageAttachment(
        Guid messageId, AttachmentType type,
        string fileName, string filePath, string mimeType, long fileSize,
        int? durationMs = null, int? width = null, int? height = null)
        : base(Guid.NewGuid())
    {
        MessageId = messageId;
        Type = type;
        FileName = fileName;
        FilePath = filePath;
        MimeType = mimeType;
        FileSize = fileSize;
        DurationMs = durationMs;
        Width = width;
        Height = height;
    }
}
