using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.Domain.Virgin.Models.Common
{
    public class PriceClass
    {
        public string Id { get; set; }
        public string FareBasisCode { get; set; }
        public string TicketDesigCode { get; set; }
        public string FareTypeCode { get; set; }
        public string ProductName { get; set; }
        public string ClassOfService { get; set; }
        public CabinType CabinType { get; set; }
    }
}
