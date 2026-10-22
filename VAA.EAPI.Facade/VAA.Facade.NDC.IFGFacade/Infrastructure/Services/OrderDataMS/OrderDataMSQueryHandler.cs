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
using VAA.Facade.NDC.IFGFacade.Infrastructure.Models.OrderData;

namespace VAA.Facade.NDC.IFGFacade.Infrastructure.Services.OrderDataMS
{
    public class OrderDataMSQueryHandler : IOrderDataQuery
    {
        private readonly IHttpConnector _httpConnector;

        public OrderDataMSQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<OrderRS> Handle(string orderId, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>
            {
                  { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.OrderDataMSSubscriptionKey) }
            };
            var httpResponseMessage = await _httpConnector.SendAsync($"{ConfigReader.Get(Constants.Environment.OrderDataMSEndpoint)}{orderId}", headers, string.Empty, HttpMethod.Get, string.Empty);

            var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                return new OrderRS
                {
                    OrderDocument = Json.Deserialize<OrderDocument>(response)
                };
            }

            return new OrderRS
            {
                Errors = Json.Deserialize<IEnumerable<ErrorDetails>>(response)
            };
        }
    }
}