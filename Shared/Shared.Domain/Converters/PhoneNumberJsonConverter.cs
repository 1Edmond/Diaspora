using Shared.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Domain.Converters;

public class PhoneNumberJsonConverter : JsonConverter<PhoneNumber>
{
    public override PhoneNumber? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var value = doc.RootElement.GetProperty("Value").GetString()!;
        var countryCode = doc.RootElement.GetProperty("CountryCode").GetString()!;
        return PhoneNumber.Create(value, countryCode).Value;
    }

    public override void Write(Utf8JsonWriter writer, PhoneNumber value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("Value", value.Value);
        writer.WriteString("CountryCode", value.CountryCode);
        writer.WriteEndObject();
    }
}