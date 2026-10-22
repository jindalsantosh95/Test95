using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services
{
    public class GetReservation(GetReservationPortTypeClient client, ILogger<GetReservation> logger) : IGetReservation
    {
        private readonly ILogger<GetReservation> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly GetReservationPortTypeClient _client = client ?? throw new ArgumentNullException(nameof(client));

        public async Task<GetReservationRS> GetReservationAsync(GetReservationOperationRequest request)
        {
            GetReservationRS response = null;
            try
            {
                var serviceResponse = await _client.GetReservationOperationAsync(request.MessageHeader, request.Security, request.GetReservationRQ);
                response = serviceResponse?.GetReservationRS;
                return response;
            }
            catch (FaultException ex)
            {
               _logger.LogError(ex, "Failed in Sabre GetReservation. Request: {Request}, Response: {Response}, Error: {ErrorMessage}", request != null ? Xml.Serialize(request) : "<null />", response != null ? Xml.Serialize(response) : "<null />", ex.Message);

                throw SabreFaultExceptionChecker.Check(ex);
            }
        }
    }
}
