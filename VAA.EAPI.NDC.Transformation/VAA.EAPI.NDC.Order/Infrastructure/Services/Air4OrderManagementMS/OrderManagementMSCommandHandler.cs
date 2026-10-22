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
using VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderManagementMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.Air4OrderManagementMS
{
    public class OrderManagementMSCommandHandler : IRequestHandler<OrderManagementMSCommand, ChangeOrderRS>
    {
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<OrderManagementMSCommandHandler> _logger;

        public OrderManagementMSCommandHandler(IHttpConnector httpConnector, ILogger<OrderManagementMSCommandHandler> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ChangeOrderRS> Handle(OrderManagementMSCommand request, CancellationToken cancellationToken)
        {
            StringContent content = new(Json.Serialize(request.OrderChangeRequest));
            var changeOrderServiceuri = ConfigReader.Get(Constants.Environment.OrderMgmtMSUrl) + request.OrderChangeRequest.RLOC;

            var patchrequest = new HttpRequestMessage(HttpMethod.Patch, changeOrderServiceuri)
            {
                Content = content
            };

            patchrequest.Headers.Add("Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.OrderMgmtMSSubscriptionKey));
            patchrequest.Headers.Add("Channelid", Constants.Application.NDC);

            var httpResponseMessage = await _httpConnector.SendAsync(patchrequest, cancellationToken);
            var response = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    return new ChangeOrderRS() { orderChangeResponse = Json.Deserialize<OrderChangeResponse>(response) };
                }
                else
                {
                    return null;
                }
            }
        }
    }

