using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetElectronicDocument;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services
{
    public class GetElectronicDocument(GetElectronicDocumentPortTypeClient client, ILogger<GetElectronicDocument> logger) : IGetElectronicDocument
    {
        private readonly GetElectronicDocumentPortTypeClient _client = client ?? throw new ArgumentNullException(nameof(client));
        private readonly ILogger<GetElectronicDocument> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<GetElectronicDocumentRS> GetElectronicDocumentRQAsync(GetElectronicDocumentRQRequest request)
        {
            GetElectronicDocumentRS response = null;
            try
            {
               var serviceResponse = await _client.GetElectronicDocumentRQAsync(request.MessageHeader, request.Security, request.GetElectronicDocumentRQ);
               response = serviceResponse?.GetElectronicDocumentRS;
               return response; 
            }
            catch (FaultException ex)
            {
                _logger.LogError(ex, "Failed in Sabre GetElectronicDocument. Request: {Request}, Response: {Response}, Error: {ErrorMessage}", request != null ? Xml.Serialize(request) : "<null />", response != null ? Xml.Serialize(response) : "<null />", ex.Message);

                throw SabreFaultExceptionChecker.Check(ex);
            }
        }
    }
}
