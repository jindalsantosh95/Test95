using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ConfigMS.BspMarket;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.ConfigMS
{
    public class BspMarketPaymentQueryHandler : IRequestHandler<BspMarketPaymentQuery, BspMarketPaymentData>
    {
        private readonly IHttpConnector _httpConnector;

        public BspMarketPaymentQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<BspMarketPaymentData> Handle(BspMarketPaymentQuery request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.ConfigurationMSSubscriptionKey)},
                {"CountryCode" , request.CountryCode },{"CurrencyCode" , request.CurrencyCode}
            };

            var httpResponseMessage = await _httpConnector.GetAsync($"{ConfigReader.Get(Constants.Environment.ConfigurationMSBSPMarketPaymentEndpoint)}", headers, string.Empty);

            var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return Json.Deserialize<BspMarketPaymentData>(response);
            else
                return null;
        }
    }
}

