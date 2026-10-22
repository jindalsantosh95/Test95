using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy;

namespace VAA.EAPI.Infrastructure.PSS.Services.Interfaces
{
    public interface ITicketingProcessV3
    {
        RetrieveResponseType Retrieve(RetrieveRequestType retrieveRequest);
        Task<PerformRevenueResponse> PerformRevenue(PerformRevenueRequestType performRevenueRequest);
        Task<EmailReceiptResponse> EmailReceipt(EmailReceiptRequestType emailReceiptRequest);
    }
}
