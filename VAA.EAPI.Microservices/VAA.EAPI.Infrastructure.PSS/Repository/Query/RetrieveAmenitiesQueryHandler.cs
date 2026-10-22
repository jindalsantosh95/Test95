using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Virgin.Models.Products.Brand;
using VAA.EAPI.Infrastructure.PSS.Entities.RetrieveAmenities;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Repository.Query
{
    public class RetrieveAmenitiesQueryHandler : IRequestHandler<RetrieveAmenitiesQuery, List<FlightSegment>?>
    {
        private readonly IRetrieveAmenities _retrievAmenities;

        public RetrieveAmenitiesQueryHandler(IRetrieveAmenities retrieveAmenities)
        {
            _retrievAmenities = retrieveAmenities ?? throw new ArgumentNullException(nameof(retrieveAmenities));
        }

        public async Task<List<FlightSegment>?> Handle(RetrieveAmenitiesQuery request, CancellationToken cancellationToken)
        {
            var retrieveRequest = new RetrieveAmenitiesRequest()
            {
                FlightSegment = request.Segment
            };
            var response = await _retrievAmenities.Retrieve(retrieveRequest);

            return response?.FlightSegment;
        }
    }
}
