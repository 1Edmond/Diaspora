using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Shared.Security.Models;

[Table("data_encryption_keys")]
public class DataEncryptionKey
{
    public Guid Id { get; set; }
    public string EntityId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public byte[] EncryptedKey { get; set; } = Array.Empty<byte>();
    public string KeyVersion { get; set; } = "v1";
    public DateTime CreatedAt { get; set; }
    public DateTime? RotatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}