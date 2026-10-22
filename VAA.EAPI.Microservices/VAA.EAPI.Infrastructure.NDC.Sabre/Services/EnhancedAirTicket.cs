using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedAirTicket;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services
{
    public class EnhancedAirTicket(AirTicketPortTypeClient client, ILogger<EnhancedAirTicket> logger) : IEnhancedAirTicket
    {
        private readonly AirTicketPortTypeClient _client = client ?? throw new ArgumentNullException(nameof(client));
        private readonly ILogger<EnhancedAirTicket> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<AirTicketRS> EnhancedAirTicketRQAsync(AirTicketRQRequest request)
        {
            AirTicketRS response = null;
            try
            {
                var serviceResponse = await _client.AirTicketRQAsync(request.MessageHeader, request.Security, request.AirTicketRQ);
                response = serviceResponse?.AirTicketRS;
                return response;
            }
            catch (FaultException ex)
            {
                _logger.LogError(ex, "Failed in Sabre EnhancedAirTicket. Request: {Request}, Response: {Response}, Error: {ErrorMessage}", request != null ? Xml.Serialize(request) : "<null />", response != null ? Xml.Serialize(response) : "<null />",  ex.Message);
                
                throw SabreFaultExceptionChecker.Check(ex);
            }
        }
    }
}
