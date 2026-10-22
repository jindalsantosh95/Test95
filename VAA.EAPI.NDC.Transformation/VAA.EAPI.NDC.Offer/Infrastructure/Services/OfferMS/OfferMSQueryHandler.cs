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
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.OfferMS
{
    public class OfferMSQueryHandler : IRequestHandler<OfferMSQuery, OfferResponse>
    {
        private readonly IHttpConnector _httpConnector;

        public OfferMSQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<OfferResponse> Handle(OfferMSQuery request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.OfferMSSubscriptionKey)},
                { "Channelid", Constants.Application.NDC }
            };

            using var httpResponseMessage = await _httpConnector.SendAsync($"{ConfigReader.Get(Constants.Environment.OfferMSSearchEndpoint)}{request.OfferID}", headers, string.Empty, HttpMethod.Get, string.Empty);
            using var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return await Json.DeserializeAsync<OfferResponse>(responseStream, cancellationToken);
            else
                return null;

        }
    }
}
