using MediatR;
using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Microservices.Retail.Brand.Application.Query.Brand
{
    public class BrandQueryRequest
    {
        public string Currency { get; set; }
        public IEnumerable<BrandQueryRequestTrip> Itinerary { get; set; }
        public string PseudoCityCode { get; set; }
        public string AccountCode { get; set; }
        public BrandQueryRequestSegment BrandQueryRequestSegment { get; set; }

        public static explicit operator BrandQueryRequest((string Currency, IEnumerable<BrandQueryRequestTrip> Itinerary, string Ptc, string PseudoCityCode, string AccountCode) source)
        {
            return new BrandQueryRequest
            {
                Currency = source.Currency,
                PseudoCityCode = source.PseudoCityCode,
                AccountCode = source.AccountCode,
                Itinerary =
                [
                    new()
                         {
                            Segments = source.Itinerary.First().Segments.Select(x => (BrandQueryRequestSegment)x),
                            PassengerTypeCodes = [source.Ptc],
                            Source = source.Itinerary?.FirstOrDefault()?.Source
                         }
                ]
            };
        }

    }
}
