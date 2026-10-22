using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Extensions
{
    public static class JObjectExtensions
    {
        public static int? ToInt(this JObject source, params string[] names)
        {
            foreach (var name in names)
            {
                if (!TryGetProperty(source, name, out var property))
                    continue;

                if (property.Type is JTokenType.Integer or JTokenType.Float)
                    return property.Value<int>();

                if (property.Type == JTokenType.String && int.TryParse(property.Value<string>(), out var value))
                    return value;
            }

            return null;
        }

        internal static bool? ToBool(this JObject source, params string[] names)
        {
            foreach (var name in names)
            {
                if (!TryGetProperty(source, name, out var property))
                    continue;

                if (property.Type == JTokenType.Boolean)
                    return property.Value<bool>();

                if (property.Type == JTokenType.String && bool.TryParse(property.Value<string>(), out var value))
                    return value;
            }

            return null;
        }

        private static bool TryGetProperty(JObject source, string name, out JToken property)
        {
            foreach (var item in source.Properties())
            {
                if (string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    property = item.Value;
                    return true;
                }
            }

            property = JValue.CreateNull();
            return false;
        }
    }
}

