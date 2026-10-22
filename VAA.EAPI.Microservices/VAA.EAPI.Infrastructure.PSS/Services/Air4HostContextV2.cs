using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Proxy.HostContext;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Services
{
    public class Air4HostContextV2 : IAir4HostContextV2
    {
        readonly HostContextClient _hostContextClient;
        private readonly ILogger<Air4HostContextV2> _logger;
        public Air4HostContextV2(HostContextClient hostContextClient, ILogger<Air4HostContextV2> logger)
        {
            _hostContextClient = hostContextClient ?? throw new ArgumentNullException(nameof(hostContextClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<CreateAndSwingResponse> CreateAndSwing(CreateAndSwingRequestType createAndSwingRequestType)
        {
            try
            {
                if (_hostContextClient.ClientCredentials.ClientCertificate.Certificate == null)
                {
                    _hostContextClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
                }

                return await _hostContextClient.CreateAndSwingAsync(createAndSwingRequestType);
            }
            catch (FaultException exception)
            {
                _logger.LogError(exception.Message, "Fault Exception from HostContextV2.CreateAndSwingAsync service");
                throw Air4FaultExceptionChecker.HostContextException(exception);
            }
        }

        public async Task<CreateResponse> Create(CreateRequestType createRequestType)
        {
            try
            {
                if (_hostContextClient.ClientCredentials.ClientCertificate.Certificate == null)
                {
                    _hostContextClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
                }

                return await _hostContextClient.CreateAsync(createRequestType);
            }
            catch (FaultException exception)
            {
                _logger.LogError(exception.Message, "Fault Exception from HostContextV2.CreateAsync service");
                throw Air4FaultExceptionChecker.HostContextException(exception);
            }
        }

        public async Task<SwingCityDutyCodeResponse> Swing(SwingCityDutyCodeRequestType swingCityDutyCodeRequestType)
        {
            try
            {
                if (_hostContextClient.ClientCredentials.ClientCertificate.Certificate == null)
                {
                    _hostContextClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
                }

                return await _hostContextClient.SwingCityDutyCodeAsync(swingCityDutyCodeRequestType);
            }
            catch (FaultException exception)
            {
                _logger.LogError(exception.Message, "Fault Exception from HostContextV2.SwingCityDutyCodeAsync service");
                throw Air4FaultExceptionChecker.HostContextException(exception);
            }
        }
    }
}
