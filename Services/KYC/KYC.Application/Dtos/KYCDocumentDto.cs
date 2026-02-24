using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Application.Dtos;

public class KYCDocumentDto
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }

    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
