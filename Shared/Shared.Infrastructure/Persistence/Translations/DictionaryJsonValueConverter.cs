using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;
using System.Text.Json;

namespace Shared.Infrastructure.Persistence.Translations;

public sealed class DictionaryJsonValueConverter : ValueConverter<Dictionary<string, string>, string>
{
    public DictionaryJsonValueConverter() : base(
        fields => JsonSerializer.Serialize(fields, JsonOptions),
        json => Deserialize(json))
    {
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    private static Dictionary<string, string> Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, string>();

        return JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions)
            ?? new Dictionary<string, string>();
    }
}
