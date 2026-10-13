using Freelance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using System;

namespace Freelance.Infrastructure.Configurations;

public class JobTemplateConfiguration : BaseEntityConfiguration<JobTemplate>
{
    public override void Configure(EntityTypeBuilder<JobTemplate> builder)
    {
        base.Configure(builder);
        builder.ToTable("job_templates");
        builder.Property(t => t.Name).HasMaxLength(150).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(2000);
        builder.Property(t => t.Amount).HasPrecision(18, 2);
        builder.Property(t => t.Currency).HasMaxLength(10);

        builder.Property(t => t.RequiredSkills)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v.ToList(), (System.Text.Json.JsonSerializerOptions?)null),
                v => (IReadOnlyCollection<string>)System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null)!);

        builder.Property(t => t.RequiredDocuments)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v.ToList(), (System.Text.Json.JsonSerializerOptions?)null),
                v => (IReadOnlyCollection<string>)System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null)!);

        builder.HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.EmployerId);
    }
}