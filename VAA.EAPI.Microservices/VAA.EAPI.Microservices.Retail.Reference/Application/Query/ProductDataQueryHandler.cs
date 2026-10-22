using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Reference.Domain;
using VAA.EAPI.Microservices.Retail.Reference.Domain.Interface;

namespace VAA.EAPI.Microservices.Retail.Reference.Application.Query
{
    public class ProductDataQueryHandler : IRequestHandler<ProductDataQuery, IEnumerable<ProductDetails>>
    {
        private readonly IScvRefDBRepository _scvRefDBRepository;

        public ProductDataQueryHandler(IScvRefDBRepository scvRefDBRepository)
        {
            _scvRefDBRepository = scvRefDBRepository ?? throw new ArgumentNullException(nameof(scvRefDBRepository));
        }

        public async Task<IEnumerable<ProductDetails>> Handle(ProductDataQuery request, CancellationToken cancellationToken)
        {
            return await _scvRefDBRepository.GetProductsDetails();
        }
    }
}
