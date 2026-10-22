using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.ReferenceMS
{
    public class CurrencyConversionQueryHandler : ICurrencyConversionQuery
    {
        private readonly IHttpConnector _httpConnector;

        public CurrencyConversionQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<string> Handle(CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.ReferenceMSSubscriptionKey) },
            };

            var uri = ConfigReader.Get(Constants.Environment.ReferenceMSUrl) + Constants.Application.CurrencyExchangeRateEndpoint;

            var httpResponseMessage = await _httpConnector.SendAsync(uri, headers, string.Empty, HttpMethod.Get, string.Empty, false);
            var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return response;
            else
                return null;
        }
    }
}
