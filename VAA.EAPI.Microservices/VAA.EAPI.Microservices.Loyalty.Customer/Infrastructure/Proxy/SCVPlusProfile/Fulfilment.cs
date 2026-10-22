using System;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class Fulfilment
    {
        public Fulfilment(DateTime? fulfillmentRequestDate = default, DateTime? slaDate = default, string fulfillmentCode = default)
        {
            FulfillmentRequestDate = fulfillmentRequestDate;
            SlaDate = slaDate;
            FulfillmentCode = fulfillmentCode;
        }

        public DateTime? FulfillmentRequestDate { get; set; }
        public DateTime? SlaDate { get; set; }
        public string FulfillmentCode { get; set; }
    }
}
