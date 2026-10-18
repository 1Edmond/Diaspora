using Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketplace.Infrastructure.Configurations;

public sealed class MarketplaceParameterConfiguration : IEntityTypeConfiguration<MarketplaceParameter>
{
    public void Configure(EntityTypeBuilder<MarketplaceParameter> builder)
    {
        builder.ToTable("marketplace_parameters");

        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Key).IsUnique();

        builder.Property(x => x.Key).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();

        builder.HasData(
            new MarketplaceParameter("DefaultEscrowCommissionRate", "0.10", "Taux de commission séquestre plateforme par défaut (ex: 0.10 pour 10%)"),
            new MarketplaceParameter("DisputeAutoEscalationDays", "5", "Nombre de jours d'attente avant escalade administrative d'office"),
            new MarketplaceParameter("MaxParcelWeightKg", "30", "Poids maximal autorisé par défaut pour un colis voyageur P2P (en kg)"),
            new MarketplaceParameter("MinParcelDeclaredValue", "1000", "Valeur déclarée minimale d'un colis (en devise locale)")
        );
    }
}
