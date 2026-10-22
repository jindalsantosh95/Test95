using MediatR;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.FareFamily;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.ConfigMS
{
    public class FareFamilyQueryHandler : IRequestHandler<FareFamilyQuery, IEnumerable<FareFamilies>>
    {
        private readonly IHttpConnector _httpConnector;

        public FareFamilyQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<IEnumerable<FareFamilies>> Handle(FareFamilyQuery request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.ConfigurationMSOcpKey)}
            };

            using var httpResponseMessage = await _httpConnector.GetAsync(ConfigReader.Get(Constants.Environment.ConfigurationMSFareFamilyEndpoint), headers, string.Empty);

            using var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return await Json.DeserializeAsync<IEnumerable<FareFamilies>>(responseStream, cancellationToken);
            else
                return null;
        }
    }
}
