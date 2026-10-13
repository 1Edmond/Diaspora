using Freelance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;

namespace Freelance.Infrastructure.Configurations;

public class JobPostingConfiguration : BaseEntityConfiguration<JobPosting>
{
    public override void Configure(EntityTypeBuilder<JobPosting> builder)
    {
        base.Configure(builder);
        builder.ToTable("job_postings");
        builder.Property(p => p.Title).HasMaxLength(150).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(2000).IsRequired();
        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.Currency).HasMaxLength(10);
        builder.Property(p => p.City).HasMaxLength(150);
        builder.Property(p => p.Country).HasMaxLength(150);

        builder.Property(p => p.RequiredSkills)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v.ToList(), (System.Text.Json.JsonSerializerOptions?)null),
                v => (IReadOnlyCollection<string>)System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null)!);

        builder.Property(p => p.RequiredDocuments)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v.ToList(), (System.Text.Json.JsonSerializerOptions?)null),
                v => (IReadOnlyCollection<string>)System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null)!);

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.EmployerId);
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => new { p.City, p.Country });
        builder.HasIndex(p => p.EventStartAt);
    }
}