using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.BspMarket;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.ConfigMS
{
    public class BspMarketPCCCurrencyQueryHandler : IRequestHandler<BspMarketPCCCurrencyQuery, PccCurrencyData>
    {
        private readonly IHttpConnector _httpConnector;

        public BspMarketPCCCurrencyQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<PccCurrencyData> Handle(BspMarketPCCCurrencyQuery request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.ConfigurationMSOcpKey)},
                {"CurrencyCode" , request.CurrencyCode }
            };

            using var httpResponseMessage = await _httpConnector.GetAsync($"{ConfigReader.Get(Constants.Environment.ConfigurationMSPccCurrencyEndpoint)}", headers, string.Empty);

            using var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return await Json.DeserializeAsync<PccCurrencyData>(responseStream, cancellationToken);
            else
                return null;
        }
    }
}

