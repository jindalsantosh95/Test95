using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.UpdateReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services
{
    public class UpdateReservation(UpdateReservationPortTypeClient updateReservationPortTypeClient, ILogger<UpdateReservation> logger) : IUpdateReservation
    {
        private readonly UpdateReservationPortTypeClient _updateReservationPortTypeClient = updateReservationPortTypeClient ?? throw new ArgumentNullException(nameof(updateReservationPortTypeClient));
        private readonly ILogger<UpdateReservation> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<UpdateReservationOperationResponse> UpdateReservationAsync(UpdateReservationOperationRequest request)
        {
            UpdateReservationOperationResponse response = null;
            try
            {
                response = await _updateReservationPortTypeClient.UpdateReservationOperationAsync(request.MessageHeader, request.Security, request.UpdateReservationRQ);
                
                return response;
            }
            catch (FaultException ex)
            {
                _logger.LogError(ex, "Failed in Sabre UpdateReservation. Request: {Request}, Response: {Response}, Error: {ErrorMessage}", request != null ? Xml.Serialize(request) : "<null />", response != null ? Xml.Serialize(response) : "<null />", ex.Message);

                throw SabreFaultExceptionChecker.Check(ex);
            }
        }
    }
}
