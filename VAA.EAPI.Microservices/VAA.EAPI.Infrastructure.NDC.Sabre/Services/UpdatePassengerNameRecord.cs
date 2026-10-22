using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.UpdatePassengerNameRecord;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services
{
    public class UpdatePassengerNameRecord(UpdatePassengerNameRecordPortTypeClient client, ILogger<UpdatePassengerNameRecord> logger) : IUpdatePassengerNameRecord
    {
        private readonly UpdatePassengerNameRecordPortTypeClient _client = client ?? throw new ArgumentNullException(nameof(client));
        private readonly ILogger<UpdatePassengerNameRecord> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<UpdatePassengerNameRecordRQResponse> UpdatePassengerNameRecordRQAsync(UpdatePassengerNameRecordRQRequest request)
        {
            UpdatePassengerNameRecordRQResponse response = null;
            try
            {
                response = await _client.UpdatePassengerNameRecordRQAsync(request.MessageHeader, request.Security, request.UpdatePassengerNameRecordRQ);

                return response;
            }
            catch (FaultException ex)
            {
                _logger.LogError(ex, "Failed in Sabre UpdatePassengerNameRecord. Request: {Request}, Response: {Response}, Error: {ErrorMessage}", request != null ? Xml.Serialize(request) : "<null />", response != null ? Xml.Serialize(response) : "<null />", ex.Message);

                throw SabreFaultExceptionChecker.Check(ex);
            }
        }
    }
}
