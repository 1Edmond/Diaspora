using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Procedure.Domain.Entities;
using Shared.Infrastructure.Persistence.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Infrastructure.Configurations;

public sealed class ServiceParameterConfiguration : BaseEntityConfiguration<ServiceParameter>
{
    public override void Configure(EntityTypeBuilder<ServiceParameter> builder)
    {
        base.Configure(builder);

        builder.ToTable("service_parameters");

        builder.HasIndex(x => x.Key).IsUnique();

        builder.Property(x => x.Key).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);

        //// Seed — dépendances bloquantes par défaut
        //builder.HasData(new
        //{
        //    Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
        //    Key = ServiceParameter.DependenciesBlocking,
        //    Value = "true",
        //    Description = "Si true, les dépendances doivent être complétées avant de compléter une procédure",
        //    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        //    CreatedBy = "System",
        //    UpdatedAt = (DateTime?)null,
        //    UpdatedBy = (string?)null
        //});
    }
}
