using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using VAA.EAPI.Web.Common.Extensions;

namespace VAA.EAPI.Web.Common.Serialisation
{
    public static class Json
    {
        public static object Deserialize(string value, Type propertyType)
        {
            if (value.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(value));

            return JsonConvert.DeserializeObject(value, propertyType);
        }

        public static T Deserialize<T>(string value, bool parseDates = true)
        {
            if (value.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(value));

            return JsonConvert.DeserializeObject<T>(value, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                // Keep unspecified timezones as unspecified - See comment in DeserializeObject
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
                DateParseHandling = parseDates ? DateParseHandling.DateTime : DateParseHandling.None
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
                // Flight arrival and departure times are expressed as local time with an unspecified
                // timezone. It is essential we don't attach a specific timezone to them, as they may
                // then be incorrectly displayed, either by adjusting for daylight savings time or
                // converting to the user's timezone.
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
            };

            return value.ToObject<T>(jsonSerializer);
        }

        public static T? DeserializeArray<T>(JArray value)
        {
            JsonSerializer jsonSerializer = new()
            {
                // Keep unspecified timezones as unspecified - See comment in DeserializeObject
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
            };

            return value.ToObject<T>(jsonSerializer);
        }
    }
}
