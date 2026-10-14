using Shared.Domain.Constants;
using Shared.Domain.Interfaces;
using Shared.Domain.Translations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Shared.Infrastructure.Extensions;

public static class TranslatableExtensions
{
    public static string GetTranslated<TOwner>(
        this TOwner owner,
        Expression<Func<TOwner, string>> property,
        IEnumerable<Translation<TOwner>> translations,
        string cultureCode,
        string defaultCulture = SharedConstants.DefaultCulture,
        string defaultValue = "")
        where TOwner : class, ITranslatable
    {
        var propertyName = ExtractPropertyName(property);
        var normalizedCulture = cultureCode.Trim().ToLowerInvariant();
        var materialized = translations as ICollection<Translation<TOwner>> ?? translations.ToList();

        var requested = materialized.FirstOrDefault(t => t.CultureCode == normalizedCulture)
            ?.GetField(propertyName);

        if (!string.IsNullOrEmpty(requested))
            return requested;

        var fallback = materialized.FirstOrDefault(t => t.CultureCode == defaultCulture)
            ?.GetField(propertyName);

        return fallback ?? defaultValue;
    }

    public static bool HasDefaultCulture<TOwner>(
        this IEnumerable<Translation<TOwner>> translations,
        string defaultCulture = SharedConstants.DefaultCulture)
        where TOwner : class, ITranslatable
    {
        return translations.Any(t => t.CultureCode == defaultCulture);
    }

    private static string ExtractPropertyName<TOwner>(Expression<Func<TOwner, string>> property)
    {
        if (property.Body is MemberExpression member)
            return member.Member.Name;

        throw new ArgumentException(
            "L'expression doit référencer directement une propriété (ex: p => p.Title).",
            nameof(property));
    }
}
