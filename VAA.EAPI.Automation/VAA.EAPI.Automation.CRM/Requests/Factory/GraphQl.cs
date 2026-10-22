using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Automation.CRM.Requests.Factory
{
    public static class GraphQl
    {
        public static string WebFormUpdatePayment(string FormName, string paymentNumber, string bankCountryName, string sortCode, string bankName, string accountNumber, string iban,
            string payeeAddress, string routingCode, string ifscCode, string accountHolderName, string swiftCode, string clabeNumber, string bankCodeNumber, string street, string town, string state, string country, string postCode, string accountSuffix)
        {
            var payload = File.ReadAllText("requests\\Body\\CRM\\WebFormUpdatePayment.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            var variables = payloadObject["variables"] as JObject;
            var payment = variables["payment"] as JObject;

            variables["formName"] = FormName;
            payment["paymentNumber"] = paymentNumber;
            payment["bankCountryName"] = bankCountryName;
            payment["sortCode"] = sortCode;
            payment["bankName"] = bankName;
            payment["accountNumber"] = accountNumber;
            payment["iban"] = iban;
            payment["payeeAddress"] = payeeAddress;
            payment["routingCode"] = routingCode;
            payment["ifscCode"] = ifscCode;
            payment["accountHolderName"] = accountHolderName;
            payment["swiftCode"] = swiftCode;
            payment["clabeNumber"] = clabeNumber;
            payment["bankCodeNumber"] = bankCodeNumber;
            payment["street"] = street;
            payment["town"] = town;
            payment["state"] = state;
            payment["country"] = country;
            payment["postCode"] = postCode;
            payment["accountSuffix"] = accountSuffix;

            var requestJson = JsonConvert.SerializeObject(payloadObject);

            return JsonConvert.SerializeObject(payloadObject);
        }

    }
}
