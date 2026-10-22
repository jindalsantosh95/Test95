using VAA.EAPI.GraphQL.Domain.Book.Models.Common;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Seats
{
    public class FlightSegmentSeatMapCatalogue
    {
        public string CatalogueId { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public Link ProductLink { get; set; }
        public string ProductGroup { get; set; }
        public string ProductSubGroup { get; set; }
        public string ReasonForIssuanceSubCode { get; set; }
        public string ReasonForIssuanceCode { get; set; }
        public string BrandId { get; set; }
        public string FlightIdRef { get; set; }
        public CurrencyAmount SeatPrice { get; set; }
    }
}
