using System;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Model
{
    public class DimCarrier
    {
        public int CarrierId { get; set; }
        public string IataAlphaCode { get; set; }
        public string CarrierAlliance { get; set; }
        public string CarrierGrouping { get; set; }
        public string CarrierType { get; set; }
        public string IataAccountingCode { get; set; }
        public string IcaoCode { get; set; }
        public string AirlineName { get; set; }
        public string BaseAirport { get; set; }
        public string Country { get; set; }
        public DateTime EffectiveStartDate { get; set; }
        public DateTime EffectiveEndDate { get; set; }
        public bool CurrentFlag { get; set; }
        public bool InferredFlag { get; set; }
    }
}
