using CSharpFunctionalExtensions;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Configuration.Domain;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;
using VAA.EAPI.Microservices.Configuration.Mapper;

namespace VAA.EAPI.Microservices.Configuration.Application.QueriesAndHandlers
{
    public class ReshopFareFamilyDBQueryHandler : IRequestHandler<ReshopFareFamilyDBQuery, Result<Response<IEnumerable<Reshop_FareFamily>>>>
    {
        private readonly IReshopFareFamilyService _reshopFareFamilyService;
        public ReshopFareFamilyDBQueryHandler(IReshopFareFamilyService reshopFareFamilyService)
        {
            _reshopFareFamilyService = reshopFareFamilyService;
        }

        public async Task<Result<Response<IEnumerable<Reshop_FareFamily>>>> Handle(ReshopFareFamilyDBQuery request, CancellationToken cancellationToken)
        {
            var reshopFareFamilies = await _reshopFareFamilyService.GetReshopFareFamilies();
            return Result.Success(new Response<IEnumerable<Reshop_FareFamily>>
            {
                Status = Status.OK,
                Value = ReshopFareFamilyResponseMapper.MapFareFamilyDetail(reshopFareFamilies)
            });

        }
    }
}
