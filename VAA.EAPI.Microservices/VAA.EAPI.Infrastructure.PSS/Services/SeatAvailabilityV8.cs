using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Proxy.SeatAvailability;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Services
{
    public class SeatAvailabilityV8 : ISeatAvailabilityV8
    {
        readonly SeatClient _seatClient;
        private readonly ILogger<SeatAvailabilityV8> _logger;

        public SeatAvailabilityV8(SeatClient seatClient, ILogger<SeatAvailabilityV8> logger)
        {
            _seatClient = seatClient ?? throw new ArgumentNullException(nameof(seatClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<RetrievePassengerMapResponse> RetrievePassengerMap(RetrievePassengerMapRequestType retrievePassengerMapRequest)
        {
            try
            {
                _logger.LogInformation($"SeatAvailabilityV8.RetrievePassengerMap Service Call - Start");

                if (_seatClient.ClientCredentials.ClientCertificate.Certificate == null)
                {
                    _seatClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
                }

                return (await _seatClient.RetrievePassengerMapAsync(retrievePassengerMapRequest));
            }
            catch (FaultException exception)
            {
                _logger.LogError(exception.Message, "Fault Exception from SeatAvailabilityV8.RetrievePassengerMap service");
                throw Air4FaultExceptionChecker.SeatAvailabilityException(exception);
            }
            finally
            {
                _logger.LogInformation($"SeatAvailabilityV8.RetrievePassengerMap Service Call - End");
            }
        }
    }
}
