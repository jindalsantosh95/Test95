using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Configuration.Domain;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;
using VAA.EAPI.Microservices.Configuration.Mapper;

namespace VAA.EAPI.Microservices.Configuration.Application
{
    public class UpsellOffersDBQueryHandler : IRequestHandler<UpsellOffersDBQuery, IEnumerable<UpsellOffersData>>
    {
        private readonly IUpsellOffersService _upsellOffersDetails;

        public UpsellOffersDBQueryHandler(IUpsellOffersService upsellOffersDetails)
        {
            _upsellOffersDetails = upsellOffersDetails ?? throw new ArgumentNullException(nameof(upsellOffersDetails));
        }

        public async Task<IEnumerable<UpsellOffersData>> Handle(UpsellOffersDBQuery request, CancellationToken cancellationToken)
        {
            var upsellOffersDetails = await _upsellOffersDetails.GetUpsellOffers();

            return ResponseMapper.MapUpsellOffers(upsellOffersDetails);
        }
    }
}
