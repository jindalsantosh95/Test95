using MediatR;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.OfferPriceMS
{
    public class OfferPriceMicroserviceCommandHandler : IRequestHandler<OfferPriceMicroserviceCommand, OfferResponse>
    {
        private readonly IHttpConnector _httpConnector;

        public OfferPriceMicroserviceCommandHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<OfferResponse> Handle(OfferPriceMicroserviceCommand offerQuery, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.OfferMSSubscriptionKey)},
                {"Channelid",Constants.Application.NDC }
            };

            using var httpResponseMessage = await _httpConnector.SendAsync($"{ConfigReader.Get(Constants.Environment.OfferPriceMSEndpoint)}", headers, string.Empty, HttpMethod.Post, Json.Serialize(offerQuery));

            using var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                return await Json.DeserializeAsync<OfferResponse>(responseStream, cancellationToken);
            }
            else
            {
                try
                {
                    return new OfferResponse() { Error = await Json.DeserializeAsync<IEnumerable<ErrorDetails>>(responseStream, cancellationToken) };
                }
                catch (Exception)
                {
                    return new OfferResponse() { Error = new ErrorDetails[] { new ErrorDetails() { Code = httpResponseMessage.StatusCode.ToString(), Message = httpResponseMessage.ReasonPhrase ?? httpResponseMessage.StatusCode.ToString() } } };
                }
            }
        }
    }
}
