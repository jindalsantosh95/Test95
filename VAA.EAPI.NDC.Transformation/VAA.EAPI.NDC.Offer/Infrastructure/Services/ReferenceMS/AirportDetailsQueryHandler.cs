using MediatR;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ReferenceMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.ReferenceMS
{
    public class AirportDetailsQueryHandler : IRequestHandler<AirportDetailsQuery, IEnumerable<AirportDetail>>
    {
        private readonly IHttpConnector _httpConnector;

        public AirportDetailsQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<IEnumerable<AirportDetail>> Handle(AirportDetailsQuery request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                 { Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.ReferenceMSOcpKey)},
            };

            using var httpResponseMessage = await _httpConnector.GetAsync($"{ConfigReader.Get(Constants.Environment.ReferenceMSAirportDetailsEndpoint)}?airportCodes={request.AirportCodes}", headers, string.Empty);

            using var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return await Json.DeserializeAsync<IEnumerable<AirportDetail>>(responseStream, cancellationToken);
            else
                return null;
        }
    }
}