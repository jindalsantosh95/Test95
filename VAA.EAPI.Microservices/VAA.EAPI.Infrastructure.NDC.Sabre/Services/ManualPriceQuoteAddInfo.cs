using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.ManualPriceQuoteAddInfoLLS;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services
{
    public class ManualPriceQuoteAddInfo(ManualPriceQuoteAddInfoPortTypeClient client, ILogger<ManualPriceQuoteAddInfo> logger) : IManualPriceQuoteAddInfo
    {
        private readonly ManualPriceQuoteAddInfoPortTypeClient _client = client ?? throw new ArgumentNullException(nameof(client));
        private readonly ILogger<ManualPriceQuoteAddInfo> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<ManualPriceQuoteAddInfoRS> ManualPriceQuoteAddInfoRQAsync(ManualPriceQuoteAddInfoRQRequest request)
        {
            ManualPriceQuoteAddInfoRS response = null;
            try
            {
                var serviceResponse = await _client.ManualPriceQuoteAddInfoRQAsync(request.MessageHeader, request.Security, request.ManualPriceQuoteAddInfoRQ);
                response = serviceResponse?.ManualPriceQuoteAddInfoRS;
                return response;
            }
            catch (FaultException ex)
            {
               _logger.LogError(ex, "Failed in Sabre ManualPriceQuote. Request: {Request}, Response: {Response}, Error: {ErrorMessage}", request != null ? Xml.Serialize(request) : "<null />", response != null ? Xml.Serialize(response) : "<null />", ex.Message);

                throw SabreFaultExceptionChecker.Check(ex);
            }
        }
    }
}
