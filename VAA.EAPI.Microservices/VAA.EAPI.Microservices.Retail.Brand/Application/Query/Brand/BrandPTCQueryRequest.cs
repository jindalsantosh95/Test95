using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Brand.Application.Query.Brand
{
    public class BrandPTCQueryRequest 
    {
        public IEnumerable<BrandQueryRequest> BrandQueryRequest { get; set; }
        
    }
}

