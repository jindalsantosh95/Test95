using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.SpecialServiceRequestV4Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services
{
    public class SpecialServiceRequestV4 : ISpecialServiceRequestV4
    {
        readonly SpecialServiceRequestClient _specialServiceRequestClient;
        public SpecialServiceRequestV4(SpecialServiceRequestClient specialServiceRequestClient)
        {
            _specialServiceRequestClient = specialServiceRequestClient;
        }
        public AddResponseType AddSSRsRequest(AddRequestType addRequest)
        {
            if (_specialServiceRequestClient.ClientCredentials.ClientCertificate.Certificate is null)
            {
                _specialServiceRequestClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
            }
            return _specialServiceRequestClient.Add(addRequest);
        }
    }
}
