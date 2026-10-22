using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net.Http;

namespace VAA.Facade.EnrolmentFacade.Helper.Serialisation
{
    public class Json
    {
        public static object DeserializeObject(string json, Type propertyType)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new ArgumentNullException(nameof(json));
            return JsonConvert.DeserializeObject(json, propertyType);
        }
        public static T DeserializeObject<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new ArgumentNullException(nameof(json));
            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver()});
        }
        public static string Serialize(object instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            return JsonConvert.SerializeObject(instance, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
            });
        }

        internal static T Serialize<T>(HttpResponseMessage responseGet)
        {
            throw new NotImplementedException();
        }
    }
}
