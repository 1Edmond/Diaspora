using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Dtos;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public List<string> Role { get; set; } = [];
    public string Status { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public string KYCLevel { get; set; } = string.Empty;
    public DateTime? KYCSubmittedAt { get; set; }
    public DateTime? KYCApprovedAt { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    //public string? PhoneNumber { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
