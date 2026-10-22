using VAA.EAPI.Domain.Virgin.Models.Products.Seat;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS
{
    public class Catalogue
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
        public SeatPrice SeatPrice { get; set; }
    }
}
