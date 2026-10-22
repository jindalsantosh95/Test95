using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Reference;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Reference
{
    public interface IPartnerDataEventHandler
    {
        Task<string> HandleAsync(PartnerDataMessage message);

    }
}
