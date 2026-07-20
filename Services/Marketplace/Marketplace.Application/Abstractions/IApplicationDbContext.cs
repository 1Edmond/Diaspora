using Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Abstractions;
public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<ServiceListing> ServiceListings { get; }
    DbSet<Review> Reviews { get; }
    DbSet<ServiceRequest> ServiceRequests { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
