using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.IgnoreTransaction;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services
{
    public class IgnoreTransaction : IIgnoreTransaction
    {
        private readonly IgnoreTransactionPortTypeClient _ignoreTransactionPortTypeClient;
        private readonly ILogger<IgnoreTransaction> _logger;

        public IgnoreTransaction(IgnoreTransactionPortTypeClient ignoreTransactionPortTypeClient, ILogger<IgnoreTransaction> logger)
        {
            _ignoreTransactionPortTypeClient = ignoreTransactionPortTypeClient ?? throw new ArgumentNullException(nameof(ignoreTransactionPortTypeClient)); ;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IgnoreTransactionRQResponse> IgnoreTransactionAsync(IgnoreTransactionRQRequest ignoreTransactionRQ)
        {
            try
            {
                return await _ignoreTransactionPortTypeClient.IgnoreTransactionRQAsync(ignoreTransactionRQ.MessageHeader, ignoreTransactionRQ.Security, ignoreTransactionRQ.IgnoreTransactionRQ);
            }
            catch (FaultException ex)
            {
               _logger.LogError(ex, "Failed in Sabre IgnoreTransaction. Request: {Request}, Error: {ErrorMessage}", ignoreTransactionRQ != null ? Xml.Serialize(ignoreTransactionRQ) : "<null />", ex.Message);

                throw SabreFaultExceptionChecker.Check(ex);
            }
        }
    }
}

