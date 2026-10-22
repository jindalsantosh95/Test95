using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Common.Serialisation
{
    public static class Json
    {
        public static object Deserialize(string value, Type propertyType)
        {
            if (value.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(value));

            return JsonConvert.DeserializeObject(value, propertyType);
        }

        public static T Deserialize<T>(string value)
        {
            if (value.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(value));

            return JsonConvert.DeserializeObject<T>(value, CreateDeserializationSettings());
        }

        public static async Task<T> DeserializeAsync<T>(Stream value, CancellationToken cancellationToken = default)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            using var bufferedStream = new MemoryStream();
            await value.CopyToAsync(bufferedStream, 81920, cancellationToken);
            bufferedStream.Position = 0;

            using var streamReader = new StreamReader(bufferedStream);
            using var jsonReader = new JsonTextReader(streamReader);
            var result = JsonSerializer.Create(CreateDeserializationSettings()).Deserialize<T>(jsonReader);

            if (jsonReader.TokenType == JsonToken.None) throw new ArgumentNullException(nameof(value));

            return result;
        }

        public static string Serialize(object instance, JsonSerializerSettings settings)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            return JsonConvert.SerializeObject(instance, settings);
        }

        public static string Serialize(object instance, bool ignoreNullValues = true)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            return Serialize(instance, new JsonSerializerSettings
            {
                NullValueHandling = ignoreNullValues ? NullValueHandling.Ignore : NullValueHandling.Include,
                DefaultValueHandling = ignoreNullValues ? DefaultValueHandling.Ignore : DefaultValueHandling.Include,
                ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore // added to resolve error- Self referencing loop detected for property 'parent' with type 'Microsoft.Azure.Cosmos.Tracing.Trace'
            });
        }

        public static T? DeserializeObject<T>(JToken value)
        {
            JsonSerializer jsonSerializer = new()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };

            return value.ToObject<T>(jsonSerializer);
        }

        public static T? DeserializeArray<T>(JArray value)
        {
            JsonSerializer jsonSerializer = new()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };

            return value.ToObject<T>(jsonSerializer);
        }

        public static string SerializeWithDoubleHandling(object instance, bool ignoreNullValues = true)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = ignoreNullValues ? NullValueHandling.Ignore : NullValueHandling.Include,
                DefaultValueHandling = ignoreNullValues ? DefaultValueHandling.Ignore : DefaultValueHandling.Include,
                ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Converters = [new DoubleConverter()]
            };

            return JsonConvert.SerializeObject(instance, settings);
        }

        private static JsonSerializerSettings CreateDeserializationSettings() => new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            DateParseHandling = DateParseHandling.None
        };

        private class DoubleConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => objectType == typeof(double);

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
                writer.WriteValue(value is double d && d == Math.Floor(d) ? (long)d : value);

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) =>
                Convert.ToDouble(reader.Value);
        }



    }
}
