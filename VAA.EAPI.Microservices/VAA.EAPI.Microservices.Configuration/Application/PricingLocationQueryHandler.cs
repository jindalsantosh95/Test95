using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Configuration.Domain;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Services;
using VAA.EAPI.Microservices.Configuration.Mapper;

namespace VAA.EAPI.Microservices.Configuration.Application
{
    public class PricingLocationDbQueryHandler : IRequestHandler<PricingLocationDbQuery, Result<Response<PricingLocation>>>
    {
        private readonly IAirportListService _AirportListService;
        private readonly ILocationMappingService _LocationMappingService;

        public PricingLocationDbQueryHandler(IAirportListService AirportListService, ILocationMappingService LocationMappingService)
        {
            _AirportListService= AirportListService?? throw new ArgumentNullException(nameof(AirportListService));
            _LocationMappingService = LocationMappingService ?? throw new ArgumentNullException(nameof(LocationMappingService));
        }

        public async Task<Result<Response<PricingLocation>>> Handle(PricingLocationDbQuery request, CancellationToken cancellationToken)
        {
            var airportDetails = await _AirportListService.GetairportDetails();
            var locationDetails = await _LocationMappingService.GetLocationMapping();

            return Result.Success(new Response<PricingLocation>
            {
                Status = Status.OK,
                Value = ResponseMapper.MapPricingLocation(airportDetails,locationDetails)
            });
        }
    }
}
