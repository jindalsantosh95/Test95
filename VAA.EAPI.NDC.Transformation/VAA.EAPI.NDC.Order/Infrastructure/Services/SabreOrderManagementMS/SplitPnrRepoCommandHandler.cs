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
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS
{
    public class SplitPnrRepoCommandHandler : IRequestHandler<SplitPnrRepoCommand, OrderDocument>
    {
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<SplitPnrRepoCommandHandler> _logger;

        public SplitPnrRepoCommandHandler(IHttpConnector httpConnector, ILogger<SplitPnrRepoCommandHandler> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OrderDocument> Handle(SplitPnrRepoCommand request, CancellationToken cancellationToken)
        {
            StringContent content = new(Json.Serialize(request.SplitPnrRequest));
            var patchrequest = new HttpRequestMessage(HttpMethod.Patch, ConfigReader.Get(Constants.Environment.SplitPnrMSUrl))
            {
                Content = content
            };

            patchrequest.Headers.Add("Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.OrderChangeMSSubscriptionKey));

            var httpResponseMessage = await _httpConnector.SendAsync(patchrequest, cancellationToken);
            var response = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                return Json.Deserialize<OrderDocument>(response);
            }
            else
            {
                return null;
            }

        }
    }
}
