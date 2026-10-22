using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Products.Brand;

namespace VAA.EAPI.Infrastructure.PSS.Repository.Query
{
    public class RetrieveAmenitiesQuery : IRequest<List<FlightSegment>?>
    {
        public List<FlightSegment>? Segment { get; set; }

    }
}
