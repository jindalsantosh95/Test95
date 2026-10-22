using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Automation.CRM.Requests.Factory
{
    public static class Crm
    {
        public static string CrmIncidents(string title)
        {
            var payload = File.ReadAllText($"requests\\Body\\CRM\\Incidents.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["title"] = title;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string CrmPayments(string caseNumber, string incidentId)
        {
            var payload = File.ReadAllText($"requests\\Body\\CRM\\CreatePaymentRecord.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["vaa_casenumber"] = caseNumber;
            payloadObject["vaa_Case@odata.bind"] = incidentId;

            return JsonConvert.SerializeObject(payloadObject);
        }
    }
}
