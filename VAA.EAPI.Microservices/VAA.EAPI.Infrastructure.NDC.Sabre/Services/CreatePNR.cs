using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreatePNR;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services
{
    public class CreatePNR(CreatePassengerNameRecordPortTypeClient client, ILogger<CreatePNR> logger) : ICreatePNR
    {
        private readonly CreatePassengerNameRecordPortTypeClient _client = client ?? throw new ArgumentNullException(nameof(client));
        private readonly ILogger<CreatePNR> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CreatePassengerNameRecordRS> CreatePNRAsync(CreatePassengerNameRecordRQRequest request)
        {
            CreatePassengerNameRecordRS response = null;
            try
            {
                var serviceResponse = await _client.CreatePassengerNameRecordRQAsync(request.MessageHeader, request.Security, request.CreatePassengerNameRecordRQ);
                response = serviceResponse?.CreatePassengerNameRecordRS; 
                return response;
            }
            catch (FaultException ex)
            {
                _logger.LogError(ex, "Failed in Sabre CreatePNR. Request: {Request}, Response: {Response}, Error: {ErrorMessage}", request != null ? Xml.Serialize(request) : "<null />", response != null ? Xml.Serialize(response) : "<null />", ex.Message);

                throw SabreFaultExceptionChecker.Check(ex);
            }
        }
    }
}
