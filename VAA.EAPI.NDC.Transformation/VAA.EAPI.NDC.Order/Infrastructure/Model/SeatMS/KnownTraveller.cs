using VAA.EAPI.Domain.Virgin.Models.Customer;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS
{
    public class KnownTraveller: Customer
    {
        public string CustID { get; set; }
        public string PTC { get; set; }
    }
}
