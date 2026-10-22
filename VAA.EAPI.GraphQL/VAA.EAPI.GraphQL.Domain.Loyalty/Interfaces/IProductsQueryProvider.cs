using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Reference;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface IProductsQueryProvider
    {
        Task<IEnumerable<ProductDetails>> GetProductDetails();
    }
}
