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
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.Air4OrderSearchMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.Air4OrderSearchMS
{
    public class Air4OrderSearchMSQueryHandler : IRequestHandler<Air4OrderSearchMSQuery, Air4OrderSearchMSResponse>
    {
        private readonly IHttpConnector _httpConnector;

        public Air4OrderSearchMSQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<Air4OrderSearchMSResponse> Handle(Air4OrderSearchMSQuery request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.Air4OrderSearchMSSubscriptionKey)},
                { "Channelid", Constants.Application.NDC }
            };

            using var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.Air4OrderSearchMSUrl), headers, string.Empty, HttpMethod.Post, Json.Serialize(request.OrderSearchRequest));
            using var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return new Air4OrderSearchMSResponse() { OrderSearchResponse = await Json.DeserializeAsync<OrderSearchResponse>(responseStream, cancellationToken) };
            else
                return new Air4OrderSearchMSResponse() { ErrorDetails = await Json.DeserializeAsync<List<ErrorDetails>>(responseStream, cancellationToken) };
        }
    }
}
