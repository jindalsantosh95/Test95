using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Domain.IATAAcknowledgement;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Infrastructure.Services
{
    public interface IPortalOCNEndpointService
    {
        Task<IATAAcknowledgement> Send(IATAOrderChangeNotifRQ request);
    }
}
