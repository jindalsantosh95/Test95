using MediatR;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ConfigMS.Currency;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.ConfigMS
{
    public class CurrencyDBQueryHandler : IRequestHandler<CurrencyDBQuery, CurrencyData>
    {
        private readonly IHttpConnector _httpConnector;

        public CurrencyDBQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<CurrencyData> Handle(CurrencyDBQuery request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                 { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.ConfigurationMSSubscriptionKey)},
            };

            var httpResponseMessage = await _httpConnector.GetAsync($"{ConfigReader.Get(Constants.Environment.ConfigurationMSPOSCurrencyEndpoint)}{request.CountryCode}", headers, string.Empty);

            var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return Json.Deserialize<CurrencyData>(response);
            else
                return null;
        }
    }
}