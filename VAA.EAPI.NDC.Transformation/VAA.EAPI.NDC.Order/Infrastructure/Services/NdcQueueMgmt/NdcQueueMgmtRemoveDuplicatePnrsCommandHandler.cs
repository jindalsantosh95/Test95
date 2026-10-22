using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Order.Domain;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.NdcQueueMgmt
{
    public class NdcQueueMgmtRemoveDuplicatePnrsCommandHandler(
        IHttpConnector httpConnector,
        ILogger<NdcQueueMgmtRemoveDuplicatePnrsCommandHandler> logger)
        : INdcQueueMgmtRemoveDuplicatePnrsCommand
    {
        private readonly IHttpConnector _httpConnector = httpConnector;
        private readonly ILogger<NdcQueueMgmtRemoveDuplicatePnrsCommandHandler> _logger = logger;

        public async Task<bool> Handle(OrderChangeDuplicatePNRNotifRQ request, string pnr, CancellationToken cancellationToken)
        {
            try
            {
                var url = ConfigReader.Get(Constants.Environment.NdcQueueMgmtRemoveDuplicatePnrsUrl);
                var subscriptionKey = ConfigReader.Get(Constants.Environment.NdcQueueMgmtSubscriptionKey);

                if (url.IsNullOrWhiteSpace() || subscriptionKey.IsNullOrWhiteSpace() || pnr.IsNullOrWhiteSpace())
                    return false;

                var headers = new Dictionary<string, string>
                {
                    { "Ocp-Apim-Subscription-Key", subscriptionKey }
                };

                var removeRequest = new { request.PCC, Pnr = pnr };

                var httpResponseMessage = await _httpConnector.SendAsync(url, headers, string.Empty, HttpMethod.Post, Json.Serialize(removeRequest));
                _ = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

                if (httpResponseMessage.StatusCode == HttpStatusCode.OK || httpResponseMessage.StatusCode == HttpStatusCode.Accepted || httpResponseMessage.StatusCode == HttpStatusCode.NoContent)
                    return true;

                _logger.LogWarning(
                    "QueueMgmt remove duplicate PNRs call failed. StatusCode={StatusCode}.",
                    (int)httpResponseMessage.StatusCode);

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "QueueMgmt remove duplicate PNRs failed.");
                return false;
            }
        }
    }
}
