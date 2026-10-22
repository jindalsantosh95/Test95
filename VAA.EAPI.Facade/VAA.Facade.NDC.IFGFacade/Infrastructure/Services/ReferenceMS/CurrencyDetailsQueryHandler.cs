using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Models.ReferenceMS;

namespace VAA.Facade.NDC.IFGFacade.Infrastructure.Services.ReferenceMS
{
    public class CurrencyDetailsQueryHandler : ICurrencyDetailsQuery
    {
        private readonly IHttpConnector _httpConnector;

        public CurrencyDetailsQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<IEnumerable<Currency>> Handle()
        {
            var headers = new Dictionary<string, string>()
            {
                {"Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.ReferenceMSSubscriptionKey)},
            };
            string uri = ConfigReader.Get(Constants.Environment.ReferenceMSUrl) + Constants.Application.CurrencyEndpoint;

            var httpResponseMessage = await _httpConnector.SendAsync(uri, headers, string.Empty, HttpMethod.Get, Constants.Application.Air4CurrencyNumber, false);
            var response = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return Json.Deserialize<IEnumerable<Currency>>(response);
            else
                return null;
        }
    }
}
