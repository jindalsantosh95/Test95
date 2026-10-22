using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentReference
{
    public class FareDetail
    {
        public string FareBasisCode { get; set; }
        public string FareFamilyType { get; set; }
        public IEnumerable<FareSegment> FareSegments { get; set; }
    }
}
