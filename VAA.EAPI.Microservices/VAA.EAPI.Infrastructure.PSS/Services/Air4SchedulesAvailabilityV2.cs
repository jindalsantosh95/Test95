using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Proxy.SchedulesAvailability;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Services
{
    public class Air4SchedulesAvailabilityV2 : IAir4SchedulesAvailabilityV2
    {
        readonly SchedulesAvailabilityClient _schedulesAvailabilityClient;
        private readonly ILogger<Air4SchedulesAvailabilityV2> _logger;
        public Air4SchedulesAvailabilityV2(SchedulesAvailabilityClient schedulesAvailabilityClient, ILogger<Air4SchedulesAvailabilityV2> logger)
        {
            _schedulesAvailabilityClient = schedulesAvailabilityClient ?? throw new ArgumentNullException(nameof(schedulesAvailabilityClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<RetrieveSpecificAvailabilityResponseType> RetrieveSpecificAvailability(RetrieveSpecificAvailabilityRequestType retrieveSpecificAvailabilityRequestType)
        {
            try
            {
                if (_schedulesAvailabilityClient.ClientCredentials.ClientCertificate.Certificate == null)
                {
                    _schedulesAvailabilityClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
                }

                return (await _schedulesAvailabilityClient.RetrieveSpecificAvailabilityAsync(retrieveSpecificAvailabilityRequestType)).RetrieveSpecificAvailabilityResponse1;
            }
            catch (FaultException exception)
            {
                _logger.LogError(exception.Message, "Fault Exception from SchedulesAvailabilityV2.RetrieveSpecificAvailability service");
                throw Air4FaultExceptionChecker.SchedulesAvailabilityException(exception);
            }
        }
    }
}
