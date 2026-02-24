using System.Text.Json;

namespace ServcicesGateway.Models;

public class AuthorizationCheckResponse
{
    public bool HasPermission { get; set; }

    public string[] Permissions { get; set; } = [];
    public string[] Roles { get; set; } = [];

    public string? Reason { get; set; }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }
}