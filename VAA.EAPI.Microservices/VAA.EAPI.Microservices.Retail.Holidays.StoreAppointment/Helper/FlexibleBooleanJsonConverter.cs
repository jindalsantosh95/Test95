using System.Text.Json;
using System.Text.Json.Serialization;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Helper;

public sealed class FlexibleBooleanJsonConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Number when reader.TryGetInt64(out var numeric) => numeric != 0,
            JsonTokenType.String => ParseFromString(reader.GetString()),
            JsonTokenType.Null => false,
            _ => false
        };
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteBooleanValue(value);
    }

    private static bool ParseFromString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (bool.TryParse(value, out var asBool))
        {
            return asBool;
        }

        if (long.TryParse(value, out var asNumber))
        {
            return asNumber != 0;
        }

        return false;
    }
}