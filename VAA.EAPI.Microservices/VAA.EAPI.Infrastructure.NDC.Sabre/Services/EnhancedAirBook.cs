using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreatePNR;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedAirBook;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services
{
    public class EnhancedAirBook(EnhancedAirBookPortTypeClient client, ILogger<EnhancedAirBook> logger) : IEnhancedAirBook
    {
        private readonly EnhancedAirBookPortTypeClient _client = client ?? throw new ArgumentNullException(nameof(client));
        private readonly ILogger<EnhancedAirBook> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<EnhancedAirBookRQResponse> EnhancedAirBookAsync(EnhancedAirBookRQRequest request)
        {
            EnhancedAirBookRQResponse response = null;
            try
            {
               response = await _client.EnhancedAirBookRQAsync(request.MessageHeader, request.Security, request.EnhancedAirBookRQ);

                return response;
            }
            catch (FaultException ex)
            {
                _logger.LogError(ex, "Failed in Sabre EnhancedAirBook. Request: {Request}, Response: {Response}, Error: {ErrorMessage}", request != null ? Xml.Serialize(request) : "<null />", response != null ? Xml.Serialize(response) : "<null />", ex.Message);

                throw SabreFaultExceptionChecker.Check(ex);
            }
        }
    }
}
