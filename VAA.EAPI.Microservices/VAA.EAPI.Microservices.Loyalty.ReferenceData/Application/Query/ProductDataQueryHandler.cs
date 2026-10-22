using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Domain;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Interface;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Query
{
    public class ProductDataQueryHandler : IProductDataQueryHandler
    {
        private readonly IScvRefDBRepository _scvRefDBRepository;

        public ProductDataQueryHandler(IScvRefDBRepository scvRefDBRepository)
        {
            _scvRefDBRepository = scvRefDBRepository ?? throw new ArgumentNullException(nameof(scvRefDBRepository));
        }

        public async Task<Result<Response<IEnumerable<ProductDetails>>>> Handle(ProductDataQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return Result.Success(new Response<IEnumerable<ProductDetails>>
                {
                    Status = Status.OK,
                    Value = await _scvRefDBRepository.GetProductsDetails()
                });
            }
            catch (Exception ex)
            {
                return Result.Failure<Response<IEnumerable<ProductDetails>>>(ex.Message.ToString());
            }
        }
    }
}
