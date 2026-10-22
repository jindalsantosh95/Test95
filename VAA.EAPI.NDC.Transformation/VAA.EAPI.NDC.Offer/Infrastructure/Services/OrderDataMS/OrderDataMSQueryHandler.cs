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

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.OrderDataMS
{
    public class OrderDataMSQueryHandler : IRequestHandler<OrderDataMSQuery, OrderDocument>
    {
        private readonly IHttpConnector _httpConnector;

        public OrderDataMSQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<OrderDocument> Handle(OrderDataMSQuery request, CancellationToken cancellationToken)
        {
            var endpoint = $"{ConfigReader.Get(Constants.Environment.OrderDataMSEndpoint)}{request.OrderID}";
            var headers = new Dictionary<string, string>()
            {
                {
                    Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.OrderDataMSSubscriptionKey)
                }
            };

            using var httpResponseMessage = await _httpConnector.SendAsync(endpoint, headers, string.Empty, HttpMethod.Get, string.Empty);
            using var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return await Json.DeserializeAsync<OrderDocument>(responseStream, cancellationToken);
            else
                return null;
        }
    }
}
