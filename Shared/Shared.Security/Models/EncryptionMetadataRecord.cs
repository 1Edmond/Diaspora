using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Shared.Security.Models;

[Table("encryption_metadata")]
public class EncryptionMetadataRecord
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public string KeyVersion { get; set; } = "v1";
    public Guid KeyId { get; set; }
    public string Algorithm { get; set; } = "AES-256-GCM";
    public string DataType { get; set; } = "General";
    public DateTime EncryptedAt { get; set; }
    public DateTime? LastRotatedAt { get; set; }
    public int DataSizeBytes { get; set; }
}
