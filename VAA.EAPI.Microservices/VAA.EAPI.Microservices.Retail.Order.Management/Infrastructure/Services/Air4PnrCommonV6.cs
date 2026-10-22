using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services
{
    public class Air4PnrCommonV6 : IAir4PnrCommonV6
    {
        readonly PnrCommonClient _pnrCommonClient;
        public Air4PnrCommonV6(PnrCommonClient pnrCommonClient)
        {
            _pnrCommonClient = pnrCommonClient;
        }
        public virtual SetupContextResponseType Air4SetupContext(SetupContextRequestType setupContextRequest)
        {

            if (_pnrCommonClient.ClientCredentials.ClientCertificate.Certificate == null)
            {
                _pnrCommonClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
            }

            return _pnrCommonClient.SetupContext(setupContextRequest);
        }

        public TeardownContextResponseType Air4TearDownContext(TeardownContextRequestType teardownContextRequest)
        {
            if (_pnrCommonClient.ClientCredentials.ClientCertificate.Certificate == null)
            {
                _pnrCommonClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
            }

            return _pnrCommonClient.TeardownContext(teardownContextRequest);
        }
    }
}
