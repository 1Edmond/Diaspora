using KYC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Application.Abstractions;

public interface IKYCDbContext
{
    DbSet<KYCVerification> KYCVerifications { get; }
    DbSet<KYCDocument> KYCDocuments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
