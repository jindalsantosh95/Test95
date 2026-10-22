using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Query;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Domain;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Interface
{
    public interface IProductDataQueryHandler
    {
        Task<Result<Response<IEnumerable<ProductDetails>>>> Handle(ProductDataQuery request, CancellationToken cancellationToken);
    }
}

