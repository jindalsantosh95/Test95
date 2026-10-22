using VAA.EAPI.Automation.Core;

namespace VAA.EAPI.Automation.CRM.Requests.Endpoint
{
    public static class GraphQl
    {
        private static readonly string _env = Config.Environment;

        public static HttpResponseMessage WebFormUpdatePayment(string bearer, string FormName, string paymentNumber, string bankCountryName, string sortCode, string bankName, string accountNumber, string iban,
            string payeeAddress, string routingCode, string ifscCode, string accountHolderName, string swiftCode, string clabeNumber, string bankCodeNumber = "", string street = "", string town = "", string state = "", string country = "", string postCode = "", string accountSuffix = "")
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Authorization", bearer);

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/graphql/secure"), Requests.Factory.GraphQl.WebFormUpdatePayment(FormName, paymentNumber, bankCountryName, sortCode, bankName, accountNumber, iban,
            payeeAddress, routingCode, ifscCode, accountHolderName, swiftCode, clabeNumber, bankCodeNumber, street, town, state, country, postCode, accountSuffix), headers);
        }
    }
}