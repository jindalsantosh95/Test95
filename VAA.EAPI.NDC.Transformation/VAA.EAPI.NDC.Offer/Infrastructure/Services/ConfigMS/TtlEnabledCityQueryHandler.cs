using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Offer.Infrastructure.Interface;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.TTL;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.ConfigMS
{
    public class TtlEnabledCityQueryHandler : ITtlEnabledCity
    {
        private readonly IHttpConnector _httpConnector;

        public TtlEnabledCityQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<IEnumerable<TtlEnabledCityData>> Handle(string airportCode, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.ConfigurationMSOcpKey)},
                { Constants.Application.AirportCode , airportCode }
            };

            using var httpResponseMessage = await _httpConnector.GetAsync(ConfigReader.Get(Constants.Environment.ConfigurationMSTtlEnabledCityEndpoint), headers, string.Empty);

            using var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return await Json.DeserializeAsync<IEnumerable<TtlEnabledCityData>>(responseStream, cancellationToken);
            else
                return null;
        }
    }
}
