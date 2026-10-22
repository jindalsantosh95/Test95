using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Products.Brand;

namespace VAA.EAPI.Infrastructure.PSS.Entities.RetrieveAmenities
{
    public class RetrieveAmenitiesRequest
    {
        public List<FlightSegment> FlightSegment { get; set; }

    }
}
