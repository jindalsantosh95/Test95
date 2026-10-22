using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using VAA.EAPI.GraphQL.Stitch.Core.Extensions;

namespace VAA.EAPI.GraphQL.Stitch.Core.Serialisation
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

            return JsonConvert.DeserializeObject<T>(value, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DateParseHandling = DateParseHandling.None
            });
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
    }
}
