using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ARCPaymentMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.ARCPaymentMS
{
    public class CheckAgentStatusQueryHandler : ICheckAgentStatusQuery
    {
        private readonly IHttpConnector _httpConnector;

        public CheckAgentStatusQueryHandler(IHttpConnector httpConnector) 
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<CheckResponse> Handle(string agencyNumber)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.ARCPaymentMSSubscriptionKey)}
            };

            NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

            queryString.Add("agencynumber", agencyNumber);

            var requestUrl = $"{ConfigReader.Get(Constants.Environment.ARCPaymentMSUrl)}{Constants.Application.ARCPaymentCheckEndpoint}?{queryString}";

            var httpResponseMessage = await _httpConnector.GetAsync(requestUrl, headers, string.Empty);

            var response = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return Json.Deserialize<CheckResponse>(response);
            else
                return null;
        }

    }
}
