using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Dtos;

public class KYCStatisticsDto
{
    public int TotalUsers { get; set; }
    public int UsersWithNoKYC { get; set; }
    public int UsersWithBasicKYC { get; set; }
    public int UsersWithIntermediateKYC { get; set; }
    public int UsersWithAdvancedKYC { get; set; }
    public int KYCSubmittedToday { get; set; }
    public int KYCApprovedToday { get; set; }
    public decimal VerificationRate { get; set; }
}