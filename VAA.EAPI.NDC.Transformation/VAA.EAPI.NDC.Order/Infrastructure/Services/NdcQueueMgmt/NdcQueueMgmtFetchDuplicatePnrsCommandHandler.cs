using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Order.Domain;
using VAA.EAPI.NDC.Order.Infrastructure.Model.NdcQueueMgmt;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.NdcQueueMgmt
{
    public class NdcQueueMgmtFetchDuplicatePnrsCommandHandler(
        IHttpConnector httpConnector,
        ILogger<NdcQueueMgmtFetchDuplicatePnrsCommandHandler> logger)
        : INdcQueueMgmtFetchDuplicatePnrsCommand
    {
        private readonly IHttpConnector _httpConnector = httpConnector;
        private readonly ILogger<NdcQueueMgmtFetchDuplicatePnrsCommandHandler> _logger = logger;

        public async Task<IReadOnlyCollection<string>> Handle(OrderChangeDuplicatePNRNotifRQ request, CancellationToken cancellationToken)
        {
            try
            {
                var url = ConfigReader.Get(Constants.Environment.NdcQueueMgmtFetchDuplicatePnrsUrl);
                var subscriptionKey = ConfigReader.Get(Constants.Environment.NdcQueueMgmtSubscriptionKey);

                if (url.IsNullOrWhiteSpace() || subscriptionKey.IsNullOrWhiteSpace())
                    return Array.Empty<string>();

                var headers = new Dictionary<string, string>
                {
                    { "Ocp-Apim-Subscription-Key", subscriptionKey }
                };

                var queueRequest = new
                {
                    request.PCC
                };

                var httpResponseMessage = await _httpConnector.SendAsync(url, headers, string.Empty, HttpMethod.Post, Json.Serialize(queueRequest));
                var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

                if (httpResponseMessage.StatusCode != HttpStatusCode.OK || response.IsNullOrWhiteSpace())
                    return Array.Empty<string>();

                var list = ParsePnrs(response).Select(x => x?.Trim()).Where(x => !x.IsNullOrWhiteSpace()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                return list;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "QueueMgmt fetch duplicate PNRs failed.");
                return Array.Empty<string>();
            }
        }

        private sealed class QueueMgmtRlocsResponse
        {
            public IEnumerable<string> Rlocs { get; set; } = Array.Empty<string>();
        }

        private static IEnumerable<string> ParsePnrs(string response)
        {
            var trimmed = response.TrimStart();

            if (trimmed.StartsWith("[", StringComparison.Ordinal))
                return Json.Deserialize<IEnumerable<string>>(response) ?? Array.Empty<string>();

            var rlocs = Json.Deserialize<QueueMgmtRlocsResponse>(response)?.Rlocs;
            if (rlocs?.Any() == true)
                return rlocs;

            return Json.Deserialize<FetchDuplicatePnrsResponse>(response)?.DuplicatePnrs ?? Array.Empty<string>();
        }
    }
}
