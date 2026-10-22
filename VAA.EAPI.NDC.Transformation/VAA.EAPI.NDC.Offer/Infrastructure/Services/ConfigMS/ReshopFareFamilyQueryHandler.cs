using MediatR;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.ReshopFareFamily;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.ConfigMS
{
    public class ReshopFareFamilyQueryHandler : IRequestHandler<ReshopFareFamilyQuery, IEnumerable<ReshopFareFamily>>
    {
        private readonly IHttpConnector _httpConnector;

        public ReshopFareFamilyQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<IEnumerable<ReshopFareFamily>> Handle(ReshopFareFamilyQuery request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                 { Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.ConfigurationMSOcpKey)},
            };

            using var httpResponseMessage = await _httpConnector.GetAsync(ConfigReader.Get(Constants.Environment.ConfigurationMSReshopFareFamilyEndpoint), headers, string.Empty);

            using var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return await Json.DeserializeAsync<IEnumerable<ReshopFareFamily>>(responseStream, cancellationToken);
            else
                return null;
        }
    }
}