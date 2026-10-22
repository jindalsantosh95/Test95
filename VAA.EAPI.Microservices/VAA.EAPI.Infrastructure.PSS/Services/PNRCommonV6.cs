using System;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Services
{
    public class PNRCommonV6 : IPNRCommonV6
    {
        readonly PnrCommonClient _pnrCommonClient;
        public PNRCommonV6(PnrCommonClient pnrCommonClient)
        {
            _pnrCommonClient = pnrCommonClient ?? throw new ArgumentNullException(nameof(pnrCommonClient));
        }

        public RetrieveResponseType Air4Retrieve(RetrieveRequestType retrieveRequest)
        {
            if (_pnrCommonClient.ClientCredentials.ClientCertificate.Certificate == null)
                _pnrCommonClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();

            return _pnrCommonClient.Retrieve(retrieveRequest);
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
