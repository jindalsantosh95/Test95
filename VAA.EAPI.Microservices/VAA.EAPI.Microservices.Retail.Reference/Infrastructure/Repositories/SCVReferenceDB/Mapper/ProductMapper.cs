using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Reference.Domain;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Models;

namespace VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Mapper
{
    public class ProductMapper
    {
        public static IEnumerable<ProductDetails> MapProductData(IEnumerable<Product> product)
        {
            return product.Select(item => (ProductDetails)item);
        }
    }
}