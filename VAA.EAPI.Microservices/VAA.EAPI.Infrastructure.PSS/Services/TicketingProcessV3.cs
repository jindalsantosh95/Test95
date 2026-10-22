using System;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Services
{
    public class TicketingProcessV3 : ITicketingProcessV3
    {
        readonly TicketingProcessClient _ticketingProcessClient;
        public TicketingProcessV3(TicketingProcessClient ticketingProcessClient)
        {
            _ticketingProcessClient = ticketingProcessClient ?? throw new ArgumentNullException(nameof(ticketingProcessClient));
        }

        public RetrieveResponseType Retrieve(RetrieveRequestType retrieveRequest)
        {
            if (_ticketingProcessClient.ClientCredentials.ClientCertificate.Certificate == null)
            {
                _ticketingProcessClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
            }
            return _ticketingProcessClient.Retrieve(retrieveRequest);
        }

        public async Task<PerformRevenueResponse> PerformRevenue(PerformRevenueRequestType performRevenueRequest)
        {
            try
            {
                if (_ticketingProcessClient.ClientCredentials.ClientCertificate.Certificate == null)
                {
                    _ticketingProcessClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
                }
                return await _ticketingProcessClient.PerformRevenueAsync(performRevenueRequest);

            }
            catch (FaultException exception)
            {
                throw Air4FaultExceptionChecker.TicketingProcessV3Exception(exception);
            }
        }

        public async Task<EmailReceiptResponse> EmailReceipt(EmailReceiptRequestType emailReceiptRequest)
        {
            try
            {
                if (_ticketingProcessClient.ClientCredentials.ClientCertificate.Certificate == null)
                {
                    _ticketingProcessClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
                }
                return await _ticketingProcessClient.EmailReceiptAsync(emailReceiptRequest);

            }
            catch (FaultException exception)
            {
                throw Air4FaultExceptionChecker.TicketingProcessV3Exception(exception);
            }
        }
    }
}
