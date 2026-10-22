using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderNotificationMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.OrderNotificationMS
{
    public class OrderNotificationMSCommandHandler : IOrderNotificationMSCommand
    {
        private readonly IHttpConnector _httpConnector;

        public OrderNotificationMSCommandHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<OrderNotificationResponse> Handle(OrderNotificationRequest request)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.OrderNotificationMSSubscriptionKey) }
            };

            var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.OrderNotificationMSUrl), headers, string.Empty, HttpMethod.Post, Json.Serialize(request));
            var response = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return new OrderNotificationResponse() { OrderNotificationAgentResponse = Json.Deserialize<OrderNotificationAgentResponse>(response)};
            else
                return new OrderNotificationResponse() { Errors = Json.Deserialize<IEnumerable<ErrorDetails>>(response)};
        }
    }
}
