using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Reference;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Reference
{
    public interface IProductDataEventHandler
    {
        Task<string> HandleAsync(ProductDataMessage message);
    }
}
