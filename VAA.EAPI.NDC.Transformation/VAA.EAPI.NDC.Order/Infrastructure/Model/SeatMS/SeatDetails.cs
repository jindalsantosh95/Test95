using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS
{
    public class SeatDetails
    {
        public string SeatNumber { get; set; }
        public Segment AssociatedSegment { get; set; }
        public Customer Beneficiary { get; set; }
        public string SeatOfferID { get; set; }
        public string SeatPrice { get; set; }
        public string DecimalPrecisionCount {  get; set; }
        public string Currency {  get; set; }
        public bool IsInvalid {  get; set; }
    }
}
