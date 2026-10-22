using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Reference.Domain;

namespace VAA.EAPI.Microservices.Retail.Reference.Application.Query
{
    public class ProductDataQuery : IRequest<IEnumerable<ProductDetails>>
    {
    }
}
