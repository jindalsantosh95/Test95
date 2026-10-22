using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.SegmentV4Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.Interfaces;
namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services
{
    public class Air4SegmentV4 : IAir4SegmentV4
    {
        readonly SegmentClient _segmentClient;
        public Air4SegmentV4(SegmentClient segmentClient)
        {
            _segmentClient = segmentClient;
        }
        public CancelAndRebookResponseType CancelAndRebookSegments(CancelAndRebookRequestType cancelAndRebookRequestType)
        {

            if (_segmentClient.ClientCredentials.ClientCertificate.Certificate == null)
            {
                _segmentClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
            }
            return _segmentClient.CancelAndRebook(cancelAndRebookRequestType);

        }
        public DeleteResponseType DeleteSegments(DeleteRequestType deleteRequestType)
        {
            if (_segmentClient.ClientCredentials.ClientCertificate.Certificate == null)
            {
                _segmentClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
            }

            return _segmentClient.Delete(deleteRequestType);
        }
        public InsertResponseType InsertSegments(InsertRequestType insertRequestType)
        {
            if (_segmentClient.ClientCredentials.ClientCertificate.Certificate == null)
            {
                _segmentClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
            }

            return _segmentClient.Insert(insertRequestType);
        }
    }
}
