using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Products.Brand;

namespace VAA.EAPI.Infrastructure.PSS.Entities.RetrieveAmenities
{
    public class RetrieveAmenitiesResponse
    {
        public List<Message> Warning { get; set; }
        public List<FlightSegment> FlightSegment { get; set; }
        public List<Message> Error { get; set; }
    }
}
