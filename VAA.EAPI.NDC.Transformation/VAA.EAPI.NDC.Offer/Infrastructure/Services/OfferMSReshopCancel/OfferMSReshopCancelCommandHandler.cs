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
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMSReshopCancel;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.OfferMSReshopCancel
{
    public class OfferMSReshopCancelCommandHandler(IHttpConnector httpConnector) : IRequestHandler<OfferMSReshopCancelCommand, CancelReshopResponse>
    {
        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));

        public async Task<CancelReshopResponse> Handle(OfferMSReshopCancelCommand request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.OfferMSSubscriptionKey)},
            };

            using var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.OfferMSReshopCancelEndpoint), headers, string.Empty, HttpMethod.Post, Json.Serialize(request.OrderReshopRequest));

            using var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return await Json.DeserializeAsync<CancelReshopResponse>(responseStream, cancellationToken);
            else
            {
                try
                {
                    return new CancelReshopResponse() { Errors = await Json.DeserializeAsync<IEnumerable<ErrorDetails>>(responseStream, cancellationToken) };
                }
                catch (Exception)
                {
                    return new CancelReshopResponse() { Errors = [new ErrorDetails() { Code = httpResponseMessage.StatusCode.ToString(), Message = httpResponseMessage.ReasonPhrase ?? httpResponseMessage.StatusCode.ToString() }] };
                }
            }
        }
    }
}
