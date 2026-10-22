using Newtonsoft.Json;
using VAA.EAPI.HealthStatusApp.Models;

namespace VAA.EAPI.HealthStatusApp.Helpers
{

    public class StringToIntConverter : JsonConverter<int>
    {
        public override void WriteJson(JsonWriter writer, int value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }

        public override int ReadJson(JsonReader reader, Type objectType, int existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String && string.IsNullOrWhiteSpace(reader.Value?.ToString()))
            {
                return 0;
            }

            return Convert.ToInt32(reader.Value);
        }
    }

    public class ResultConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Result);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                return serializer.Deserialize<Result>(reader);
            }
            else if (reader.TokenType == JsonToken.Integer)
            {
                return new Result { Status = Convert.ToInt32(reader.Value) };
            }
            else if (reader.TokenType == JsonToken.String && string.IsNullOrEmpty((string)reader.Value))
            {
                return null;
            }

            throw new JsonSerializationException("Unexpected token when deserializing Result");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var result = (Result)value;

            if (result != null)
            {
                serializer.Serialize(writer, result);
            }
            else
            {
                writer.WriteNull();
            }
        }
    }
}
