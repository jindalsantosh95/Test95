using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Brand.Application.Query.Brand;
using VAA.EAPI.Microservices.Retail.Brand.Infrastructure.ATPCO;

namespace VAA.EAPI.Microservices.Retail.Brand.Infrastructure.ATPCO.Request
{
    public class Itinerary
    {
        public string Pos { get; set; }
        public string Src { get; set; }
        public IEnumerable<string> Psgrs { get; set; }
        public IEnumerable<Segment> Segments { get; set; }

        public static explicit operator Itinerary(BrandQueryRequestTrip source)
        {
            if (source == null)
                return null;

            return new()
            {
                Pos = source.PointOfSale,
                Src = source.Source,
                Psgrs = source.PassengerTypeCodes,
                Segments = source.Segments.Select(s => (Segment)s)
            };
        }
    }
}
