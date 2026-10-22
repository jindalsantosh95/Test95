using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Domain;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Models;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Mapper
{
    public class ProductMapper
    {
        public static IEnumerable<ProductDetails> MapProductData(IEnumerable<Product> product)
        {
            return product.Select(item => (ProductDetails)item);
        }
    }
}