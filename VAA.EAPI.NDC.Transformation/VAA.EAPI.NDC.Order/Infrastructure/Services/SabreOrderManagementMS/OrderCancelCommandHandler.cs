using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS
{
    public class OrderCancelCommandHandler : IRequestHandler<OrderCancelCommand, bool>
    {
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<OrderCancelCommandHandler> _logger;

        public OrderCancelCommandHandler(IHttpConnector httpConnector, ILogger<OrderCancelCommandHandler> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(OrderCancelCommand request, CancellationToken cancellationToken)
        {
            var patchrequest = new HttpRequestMessage(HttpMethod.Patch, ConfigReader.Get(Constants.Environment.OrderCancelMSUrl))
            {
                Content = new StringContent(Json.Serialize(request.OrderChangeRequest))
            };

            patchrequest.Headers.Add("Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.OrderCancelMSSubscriptionKey));

            var httpResponseMessage = await _httpConnector.SendAsync(patchrequest, cancellationToken);
            var response = await httpResponseMessage.Content.ReadAsStringAsync();


            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
    }
}
