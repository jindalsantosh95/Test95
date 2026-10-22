using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.TicketingProcessV3Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.Interfaces;
namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services
{
    public class Air4TicketingProcessV3 : IAir4TicketingProcessV3
    {

        readonly TicketingProcessClient _ticketingProcessClient;
        public Air4TicketingProcessV3(TicketingProcessClient ticketingProcessClient)
        {
            _ticketingProcessClient = ticketingProcessClient;
        }
        public async Task<PerformRevenueResponseType> PerformRevenue(PerformRevenueRequestType performRevenueRequestType)
        {
            if (_ticketingProcessClient.ClientCredentials.ClientCertificate.Certificate is null)
            {
                _ticketingProcessClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
            }
            return (await _ticketingProcessClient.PerformRevenueAsync(performRevenueRequestType)).PerformRevenueResponse1;

        }
        public async Task<RepriceResponseType> Reprice(RepriceRequestType repriceRequestType)
        {

            if (_ticketingProcessClient.ClientCredentials.ClientCertificate.Certificate is null)
            {
                _ticketingProcessClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
            }
            return (await _ticketingProcessClient.RepriceAsync(repriceRequestType)).RepriceResponse1;

        }
    }
}
