using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.NDC.Sabre.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.SabreCommandLLS;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services
{
    public class SabreCommandLLS(SabreCommandPortTypeClient client, ILogger<SabreCommandLLS> logger) : ISabreCommandLLS
    {
        private readonly SabreCommandPortTypeClient _client = client ?? throw new ArgumentNullException(nameof(client));
        private readonly ILogger<SabreCommandLLS> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<SabreCommandLLSRS> SabreCommandLLSAsync(SabreCommandLLSRQRequest1 request)
        {
            SabreCommandLLSRS response = null;
            try
            {
                var serviceResponse = await _client.SabreCommandLLSRQAsync(request.MessageHeader, request.Security, request.SabreCommandLLSRQ);
                response = serviceResponse?.SabreCommandLLSRS;
                return response;
            }
            catch (FaultException ex)
            {
                _logger.LogError(ex, "Failed in Sabre SabreCommandLLS. Request: {Request}, Response: {Response}, Error: {ErrorMessage}", request != null ? Xml.Serialize(request) : "<null />", response != null ? Xml.Serialize(response) : "<null />", ex.Message);

                throw SabreFaultExceptionChecker.Check(ex);
            }
        }

    }
}
