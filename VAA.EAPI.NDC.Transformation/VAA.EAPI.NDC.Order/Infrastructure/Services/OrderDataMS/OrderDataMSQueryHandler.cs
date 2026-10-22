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
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using Environment = VAA.EAPI.NDC.Order.Constants.Environment;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS
{
    public class OrderDataMSQueryHandler : IRequestHandler<OrderDataMSQuery, OrderRS>
    {
        private readonly IHttpConnector _httpConnector;

        public OrderDataMSQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<OrderRS> Handle(OrderDataMSQuery request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Environment.OrderDataMSSubscriptionKey)},
            };

            var httpResponseMessage = await _httpConnector.SendAsync($"{ConfigReader.Get(Environment.OrderDataMSEndpoint)}{request.OrderID}", headers, string.Empty, HttpMethod.Get, string.Empty);
            var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return new OrderRS() { OrderDocument = Json.Deserialize<OrderDocument>(response) };
            else
                return new OrderRS() { Errors = Json.Deserialize<IEnumerable<ErrorDetails>>(response) };
        }
    }
}
