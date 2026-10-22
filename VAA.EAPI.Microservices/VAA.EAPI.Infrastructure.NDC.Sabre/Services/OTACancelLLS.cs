using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.OTACancel;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services
{
    public class OTACancelLLS(OTA_CancelPortTypeClient client, ILogger<OTACancelLLS> logger) : IOTACancelLLS
    {
        private readonly OTA_CancelPortTypeClient _client = client ?? throw new ArgumentNullException(nameof(client));
        private readonly ILogger<OTACancelLLS> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<OTA_CancelRQResponse> OTACancelLLSAsync(OTA_CancelRQRequest request)
        {
            OTA_CancelRQResponse response = null;
            try
            {
                response = await _client.OTA_CancelRQAsync(request.MessageHeader, request.Security, request.OTA_CancelRQ);

                return response;
            }
            catch (FaultException ex)
            {
              _logger.LogError(ex, "Failed in Sabre OTACancel. Request: {Request}, Response: {Response}, Error: {ErrorMessage}", request != null ? Xml.Serialize(request) : "<null />", response != null ? Xml.Serialize(response) : "<null />", ex.Message);

                throw SabreFaultExceptionChecker.Check(ex);
            }
        }
    }
}
