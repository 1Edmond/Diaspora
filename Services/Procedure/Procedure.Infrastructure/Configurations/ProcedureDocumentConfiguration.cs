using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;
using Procedure.Domain.Entities;

namespace Procedure.Infrastructure.Configurations;

public sealed class ProcedureDocumentConfiguration : BaseEntityConfiguration<ProcedureDocument>
{
    public override void Configure(EntityTypeBuilder<ProcedureDocument> builder)
    {
        base.Configure(builder);

        builder.ToTable("procedure_documents");

        builder.Property(x => x.ProcedureId).IsRequired();
        builder.Property(x => x.DocumentTypeId).IsRequired();

        builder.Property(x => x.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(x => new { x.ProcedureId, x.DocumentTypeId, x.Role }).IsUnique();
    }
}