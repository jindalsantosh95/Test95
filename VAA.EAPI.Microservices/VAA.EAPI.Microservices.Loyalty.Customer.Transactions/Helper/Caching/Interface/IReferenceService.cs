using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Reference;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper.Caching.Interface
{
    public interface IReferenceService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<IEnumerable<Partner>> GetAllPartnersAsync();

    }
}
