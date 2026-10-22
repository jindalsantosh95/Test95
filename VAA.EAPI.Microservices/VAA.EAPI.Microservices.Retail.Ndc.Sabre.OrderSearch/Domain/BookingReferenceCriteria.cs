using VAA.EAPI.Domain.Virgin.Models.Order;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Domain
{
    public class BookingReferenceCriteria
    {
        public BookingReference BookingReference { get; set; }
        public CustomerName CustomerName { get; set; }
    }
}
