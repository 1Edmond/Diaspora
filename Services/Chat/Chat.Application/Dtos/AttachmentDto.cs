using Chat.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Dtos;

public sealed record AttachmentDto(
    Guid Id,
    AttachmentType Type,
    string FileName,
    string MimeType,
    long FileSize,
    int? DurationMs,
    int? Width,
    int? Height
);