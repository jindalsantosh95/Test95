using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Insurance
{ 

    public class InsurancePurchase
    {
        public string PartnerBookingID { get; set; }
        public int OrderID { get; set; }
        public string Status { get; set; }
        public IEnumerable<Product> ProductDetails { get; set; }
    }

}