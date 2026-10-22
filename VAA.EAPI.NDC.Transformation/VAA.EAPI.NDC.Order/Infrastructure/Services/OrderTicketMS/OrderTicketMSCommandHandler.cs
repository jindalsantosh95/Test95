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

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.OrderTicketMS
{
    public class OrderTicketMSCommandHandler : IRequestHandler<OrderTicketMSCommand, OrderTicketRS>
    {
        private readonly IHttpConnector _httpConnector;

        public OrderTicketMSCommandHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<OrderTicketRS> Handle(OrderTicketMSCommand request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.OrderTicketMSSubscriptionKey) }

            };

            var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.OrderTicketMSUrl), headers, string.Empty, HttpMethod.Post, Json.Serialize(request));
            var response = await httpResponseMessage.Content.ReadAsStringAsync();
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                return new OrderTicketRS() { OrderTicketDocument = Json.Deserialize<OrderTicketDocument>(response) };
            }

            else
            {
                return new OrderTicketRS() { Errors = Json.Deserialize<List<ErrorDetails>>(response) };
            }

        }

    }

}
