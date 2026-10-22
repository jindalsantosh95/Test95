using System;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Proxy.PNRCommon;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.Services
{
    public class PnrCommonV6 : IPnrCommonV6
    {
        readonly PnrCommonClient _pnrCommonClient;
        public PnrCommonV6(PnrCommonClient pnrCommonClient)
        {
            _pnrCommonClient = pnrCommonClient ?? throw new ArgumentNullException(nameof(pnrCommonClient));
        }

      
        public virtual SetupContextResponseType Air4SetupContext(SetupContextRequestType setupContextRequest)
        {
            if (_pnrCommonClient.ClientCredentials.ClientCertificate.Certificate == null)
                _pnrCommonClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();

            return _pnrCommonClient.SetupContext(setupContextRequest);
        }

      
        public TeardownContextResponseType Air4TeardownContext(TeardownContextRequestType teardownContextRequest)
        {
            if (_pnrCommonClient.ClientCredentials.ClientCertificate.Certificate == null)
                _pnrCommonClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();

            return _pnrCommonClient.TeardownContext(teardownContextRequest);
        }

       
    }
}
