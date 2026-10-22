using CSharpFunctionalExtensions;
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

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.OrderMS
{
    public class OrderMSCommandHandler : IRequestHandler<OrderMSCommand, OrderRS>
    {
        private readonly IHttpConnector _httpConnector;

        public OrderMSCommandHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<OrderRS> Handle(OrderMSCommand request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.OrderMSSubscriptionKey) }

            };

            var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.OrderMSCreateUrl), headers, string.Empty, HttpMethod.Patch, Json.Serialize(request));
            var response = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                return new OrderRS() { OrderDocument = Json.Deserialize<OrderDocument>(response) };
            }
            else
            {
                var reponse = Json.Deserialize<Result<Response<OrderDocument>>>(response);
                return new OrderRS() { Errors = Json.Deserialize<List<ErrorDetails>>(reponse.Error) };
            }

        }

    }

}
