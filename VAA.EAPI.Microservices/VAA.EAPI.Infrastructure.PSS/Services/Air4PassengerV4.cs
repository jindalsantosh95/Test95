using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Proxy.PassengerV4Proxy;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Services
{
    public class Air4PassengerV4 : IAir4PassengerV4
    {
        readonly PassengerClient _passengerClient;
        private readonly ILogger<Air4PassengerV4> _logger;
        public Air4PassengerV4(PassengerClient pnrCommonClient, ILogger<Air4PassengerV4> logger)
        {
            _passengerClient = pnrCommonClient ?? throw new ArgumentNullException(nameof(pnrCommonClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ManageMultipleTravelDocsAndSecureFlightResponse> Air4ManageMultipleTravelDocsAndSecureFlight(ManageMultipleTravelDocsAndSecureFlightRequestType manageMultipleTravelDocsAndSecureFlightRequestType)
        {
            try
            {
                _logger.LogInformation($"Air4ManageMultipleTravelDocsAndSecureFlight Service Call - Start");
                if (_passengerClient.ClientCredentials.ClientCertificate.Certificate == null)
                {
                    _passengerClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
                }

                return await _passengerClient.ManageMultipleTravelDocsAndSecureFlightAsync(manageMultipleTravelDocsAndSecureFlightRequestType);
            }
            catch (FaultException exception)
            {
                _logger.LogError(exception.Message, "Fault Exception from passengerV4 service");
                throw Air4FaultExceptionChecker.PassengerV4Exception(exception);
            }
            finally
            {
                _logger.LogInformation($"Air4ManageMultipleTravelDocsAndSecureFlight Service Call - End");
            }
        }

        public Task<UpdateSFPDResponseType> PSDSSync(UpdateSFPDRequestType updateSFPDRequestType)
        {
            try
            {
                if (_passengerClient.ClientCredentials.ClientCertificate.Certificate == null)
                {
                    _passengerClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
                }

                _logger.LogInformation($"PSDS Sync Service Call - Start");
                return Task.FromResult(_passengerClient.UpdateSecureFlightData(updateSFPDRequestType));

            }
            catch (FaultException exception)
            {
                _logger.LogError(exception.Message, "Fault Exception from passengerV4 service");
                throw Air4FaultExceptionChecker.PassengerV4Exception(exception);
            }
        }
    }
}
