using MediatR;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.AirShoppingMS
{
    public class FlexiDateSearchMSQueryHandler : IRequestHandler<FlexiDateSearchMSQuery, CalendarSearchResponse>
    {
        private readonly IHttpConnector _httpConnector;

        public FlexiDateSearchMSQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<CalendarSearchResponse> Handle(FlexiDateSearchMSQuery airShoppingMSQuery, CancellationToken cancellationToken)
        {
            string endpoint = ConfigReader.Get(Constants.Environment.CalendarSearchEndpoint);
            var headers = new Dictionary<string, string>()
            {
                {
                     Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.OfferMSSubscriptionKey)
                }
            };


            using var httpResponseMessage = await _httpConnector.SendAsync(endpoint, headers, string.Empty, HttpMethod.Post, Json.Serialize(airShoppingMSQuery));
            using var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return await Json.DeserializeAsync<CalendarSearchResponse>(responseStream, cancellationToken);
            else
                return null;
        }
    }
}
