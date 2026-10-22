using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Automation.Core.Delta.Factory
{
    static class Soa
    {
        public static string SOABrand()
        {
            var payload = File.ReadAllText($"requests\\Body\\SOA\\SoaBrand.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            return JsonConvert.SerializeObject(payloadObject);
        }
    }
}