using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EndTransaction;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services
{
    public class EndTransaction(EndTransactionPortTypeClient client, ILogger<EndTransaction> logger) : IEndTransaction
    {
      
        private readonly ILogger<EndTransaction> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly EndTransactionPortTypeClient _client = client ?? throw new ArgumentNullException(nameof(client));
       

        public async Task<EndTransactionRS> EndTransactionRQAsync(EndTransactionRQRequest request)
        {
            EndTransactionRS response = null;
            try
            {
                var serviceResponse = await _client.EndTransactionRQAsync(request.MessageHeader, request.Security, request.EndTransactionRQ);
                response = serviceResponse?.EndTransactionRS;
                return response; 
            }
            catch (FaultException ex)
            {
                _logger.LogError(ex, "Failed in Sabre EndTransaction. Request: {Request}, Response: {Response}, Error: {ErrorMessage}", request != null ? Xml.Serialize(request) : "<null />", response != null ? Xml.Serialize(response) : "<null />", ex.Message);

                throw SabreFaultExceptionChecker.Check(ex);
            }
        }
    }
}
