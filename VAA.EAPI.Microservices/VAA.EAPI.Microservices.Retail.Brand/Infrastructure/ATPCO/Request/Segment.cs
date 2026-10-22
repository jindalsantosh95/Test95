using VAA.EAPI.Microservices.Retail.Brand.Application.Query.Brand;

namespace VAA.EAPI.Microservices.Retail.Brand.Infrastructure.ATPCO.Request
{
    public class Segment
    {
        public string Dep { get; set; }
        public string Arr { get; set; }
        public string Cxr { get; set; }
        public int Fltno { get; set; }
        public string Date { get; set; }
        public int Cabin { get; set; }
        public string Fbc { get; set; }

        public static explicit operator Segment(BrandQueryRequestSegment source)
        {
            if (source is null)
                return null;

            return new()
            {
                Date = source.DepartureDate.ToString("yyyy-MM-dd"),
                Cxr = source.Carrier,
                Fltno = source.FlightNumber,
                Dep = source.Origin,
                Arr = source.Destination,
                Cabin = source.Cabin,
                Fbc = string.IsNullOrWhiteSpace(source.TicketDesignator)
                    ? source.FareBasis
                    : $"{source.FareBasis}/{source.TicketDesignator}"
            };
        }
    }

}
