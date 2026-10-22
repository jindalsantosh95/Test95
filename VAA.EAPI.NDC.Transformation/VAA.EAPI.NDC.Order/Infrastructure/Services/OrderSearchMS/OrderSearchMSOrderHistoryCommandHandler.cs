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
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.OrderSearchMS
{
    public class OrderSearchMSOrderHistoryCommandHandler : IRequestHandler<OrderSearchMSOrderHistoryCommand, OrderDocument>
    {
        private readonly IHttpConnector _httpConnector;

        public OrderSearchMSOrderHistoryCommandHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<OrderDocument> Handle(OrderSearchMSOrderHistoryCommand request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.OrderSearchMSSubscriptionKey)}
            };

            var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.OrderSearchMSOrderHistoryUrl), headers, string.Empty, HttpMethod.Post, Json.Serialize(request.OrderSearchRequest));
            var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return Json.Deserialize<OrderDocument>(response);
            else
                return null;
        }

    }
}
