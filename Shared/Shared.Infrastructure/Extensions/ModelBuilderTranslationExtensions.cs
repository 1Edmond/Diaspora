using Microsoft.EntityFrameworkCore;
using Shared.Domain.Interfaces;
using Shared.Domain.Translations;
using System.Linq;

namespace Shared.Infrastructure.Persistence.Translations;

public static class ModelBuilderTranslationExtensions
{
    public static void ApplyTranslatableEntities(this ModelBuilder modelBuilder)
    {
        var ownerEntityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(ITranslatable).IsAssignableFrom(e.ClrType))
            .Select(e => e.ClrType)
            .ToList();

        foreach (var ownerType in ownerEntityTypes)
        {
            var translationType = typeof(Translation<>).MakeGenericType(ownerType);

            modelBuilder.Entity(translationType, builder =>
            {
                builder.ToTable($"{ownerType.Name}Translations");

                builder.Property<Guid>(nameof(Translation<ITranslatable>.Id));
                builder.HasKey(nameof(Translation<ITranslatable>.Id));

                builder.Property<Guid>(nameof(Translation<ITranslatable>.OwnerId))
                    .IsRequired();

                builder.Property<string>(nameof(Translation<ITranslatable>.CultureCode))
                    .HasMaxLength(10)
                    .IsRequired();

                builder.Property<Dictionary<string, string>>(nameof(Translation<ITranslatable>.Fields))
                    .HasConversion(new DictionaryJsonValueConverter())
                    .HasColumnType("jsonb")
                    .IsRequired();

                builder.Property<DateTime>(nameof(Translation<ITranslatable>.CreatedAt));
                builder.Property<DateTime?>(nameof(Translation<ITranslatable>.UpdatedAt));

                builder.HasIndex(
                        nameof(Translation<ITranslatable>.OwnerId),
                        nameof(Translation<ITranslatable>.CultureCode))
                    .IsUnique();

                builder
                    .HasOne(ownerType)
                    .WithMany()
                    .HasForeignKey(nameof(Translation<ITranslatable>.OwnerId))
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
