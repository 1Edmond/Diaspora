namespace ServcicesGateway.Models;

public class AuthorizationCheckRequest
{
    public Guid UserId { get; set; }
    public string Permission { get; set; } = string.Empty;
}