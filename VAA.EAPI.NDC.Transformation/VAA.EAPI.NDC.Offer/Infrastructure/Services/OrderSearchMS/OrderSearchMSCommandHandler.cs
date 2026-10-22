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
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.OrderSearchMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.OrderSearchMS
{
    public class OrderSearchMSCommandHandler : IRequestHandler<OrderSearchMSCommand, OrderDocument>
    {
        private readonly IHttpConnector _httpConnector;

        public OrderSearchMSCommandHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<OrderDocument> Handle(OrderSearchMSCommand request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Offer.Constants.Environment.OrderSearchMSSubscriptionKey)}
            };

            using var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Offer.Constants.Environment.OrderSearchMSUrl) + Offer.Constants.Environment.OrderSearchEndpoint, headers, string.Empty, HttpMethod.Post, Json.Serialize(request.OrderSearchRequest));
            using var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return await Json.DeserializeAsync<OrderDocument>(responseStream, cancellationToken);

            return null;
        }

    }
}
