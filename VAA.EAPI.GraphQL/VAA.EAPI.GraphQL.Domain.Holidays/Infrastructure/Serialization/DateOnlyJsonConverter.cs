using System.Globalization;
using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Serialization;

public sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string Format = "yyyy-MM-dd";

    public override void WriteJson(JsonWriter writer, DateOnly value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString(Format, CultureInfo.InvariantCulture));
    }

    public override DateOnly ReadJson(
        JsonReader reader,
        Type objectType,
        DateOnly existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String
            && DateOnly.TryParseExact(
                reader.Value as string,
                Format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var value))
        {
            return value;
        }

        throw new JsonSerializationException($"Expected a date in {Format} format but received '{reader.Value}'.");
    }
}
