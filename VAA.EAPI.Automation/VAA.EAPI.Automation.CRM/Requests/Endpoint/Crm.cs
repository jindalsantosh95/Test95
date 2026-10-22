using VAA.EAPI.Automation.Core;

namespace VAA.EAPI.Automation.CRM.Requests.Endpoint
{
    public static class Crm
    {
        public static HttpResponseMessage CrmCountryFilterOnName(string bearer, string countryBankName)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Authorization", bearer);

            return WebRequest.Get(Config.Crm.GetUrl($"/api/data/v9.2/vaa_countries?$select=vaa_countryid,vaa_name,vaa_territoryshortname,vaa_territorycode,vaa_type&$filter=vaa_name eq '{countryBankName.Replace("'", "''")}'"), headers);
        }

        public static HttpResponseMessage CrmReturnPaymentUsingPaymentNumber(string bearer, string paymentNumber)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Authorization", bearer);

            return WebRequest.Get(Config.Crm.GetUrl($"api/data/v9.2/vaa_payments?$filter=vaa_paymentnumber eq '{paymentNumber}'"), headers);
        }

        public static HttpResponseMessage CrmCreateCase(string bearer, string title)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Authorization", bearer);

            return WebRequest.Json.Post(Config.Crm.GetUrl("api/data/v9.0/incidents"), Factory.Crm.CrmIncidents(title), headers);
        }

        public static HttpResponseMessage CrmGetCaseDetailsById(string bearer, string locationUrl)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Authorization", bearer);

            return WebRequest.Get(locationUrl, headers);
        }

        public static HttpResponseMessage CrmCreatePaymentRecord(string bearer, string caseNumber, string incidentId)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Authorization", bearer);

            return WebRequest.Json.Post(Config.Crm.GetUrl("api/data/v9.2/vaa_payments"), Factory.Crm.CrmPayments(caseNumber, incidentId), headers);
        }

        public static HttpResponseMessage CrmGetPaymentDetailsById(string bearer, string locationUrl)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Authorization", bearer);

            return WebRequest.Get(locationUrl, headers);
        }
    }
}