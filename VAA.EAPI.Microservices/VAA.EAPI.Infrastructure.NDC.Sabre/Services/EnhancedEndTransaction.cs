using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedEndTransaction;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services
{
    public class EnhancedEndTransaction(EnhancedEndTransactionPortTypeClient client, ILogger<EnhancedEndTransaction> logger) : IEnhancedEndTransaction
    {
        private readonly ILogger<EnhancedEndTransaction> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly EnhancedEndTransactionPortTypeClient _client = client ?? throw new ArgumentNullException(nameof(client));

        public async Task<EnhancedEndTransactionRS> EnhancedEndTransactionRQAsync(EnhancedEndTransactionRQRequest request)
        {
            EnhancedEndTransactionRS response = null;
            try
            {
                var serviceResponse = await _client.EnhancedEndTransactionRQAsync(request.MessageHeader, request.Security, request.EnhancedEndTransactionRQ);
                response = serviceResponse?.EnhancedEndTransactionRS;
                return response; 
            }
            catch (FaultException ex)
            {
                _logger.LogError(ex, "Failed in Sabre EnhancedEndTransaction. Request: {Request}, Response: {Response}, Error: {ErrorMessage}", request != null ? Xml.Serialize(request) : "<null />", response != null ? Xml.Serialize(response) : "<null />", ex.Message);

                throw SabreFaultExceptionChecker.Check(ex);
            }
        }
    }
}
