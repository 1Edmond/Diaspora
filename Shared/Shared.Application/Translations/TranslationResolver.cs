using Shared.Domain.Interfaces;
using Shared.Domain.Translations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shared.Application.Translations;

public static class TranslationResolver
{
    /// <summary>
    /// Résout la valeur d'une propriété traduite selon la cascade :
    /// 1. Langue préférée demandée
    /// 2. "en" (anglais)
    /// 3. "fr" (français)
    /// 4. Première traduction disponible
    /// 5. Valeur par défaut de l'entité
    /// </summary>
    public static string Resolve<TOwner>(
        string defaultValue,
        string propertyName,
        IEnumerable<Translation<TOwner>>? translations,
        string? preferredLanguage)
        where TOwner : class, ITranslatable
    {
        if (translations == null)
            return defaultValue;

        var list = translations as IReadOnlyList<Translation<TOwner>> ?? translations.ToList();
        if (list.Count == 0)
            return defaultValue;

        // 1. Langue préférée demandée
        if (!string.IsNullOrWhiteSpace(preferredLanguage))
        {
            var normalizedPreferred = preferredLanguage.Trim().ToLowerInvariant();
            var match = list.FirstOrDefault(t => t.CultureCode == normalizedPreferred);
            var val = match?.GetField(propertyName);
            if (!string.IsNullOrWhiteSpace(val))
                return val;
        }

        // 2. Repli "en"
        var enMatch = list.FirstOrDefault(t => t.CultureCode == "en");
        var enVal = enMatch?.GetField(propertyName);
        if (!string.IsNullOrWhiteSpace(enVal))
            return enVal;

        // 3. Repli "fr"
        var frMatch = list.FirstOrDefault(t => t.CultureCode == "fr");
        var frVal = frMatch?.GetField(propertyName);
        if (!string.IsNullOrWhiteSpace(frVal))
            return frVal;

        // 4. Première disponible
        var firstMatch = list.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.GetField(propertyName)));
        var firstVal = firstMatch?.GetField(propertyName);
        if (!string.IsNullOrWhiteSpace(firstVal))
            return firstVal;

        return defaultValue;
    }
}
