using MediatR;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.UpsellOffers;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.ConfigMS
{
    public class UpsellOffersDBQueryHandler : IRequestHandler<UpsellOffersDBQuery, IEnumerable<UpsellOffersData>>
    {
        private readonly IHttpConnector _httpConnector;

        public UpsellOffersDBQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<IEnumerable<UpsellOffersData>> Handle(UpsellOffersDBQuery request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                 { Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.ConfigurationMSOcpKey)},
            };

            using var httpResponseMessage = await _httpConnector.GetAsync(ConfigReader.Get(Constants.Environment.ConfigurationMSUpsellOffersEndpoint), headers, string.Empty);

            using var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                return await Json.DeserializeAsync<IEnumerable<UpsellOffersData>>(responseStream, cancellationToken);
            }
            else
            {
                return new UpsellOffersData[] { new() { Error = await Json.DeserializeAsync<IEnumerable<ErrorDetails>>(responseStream, cancellationToken) } };
            }
        }
    }
}
