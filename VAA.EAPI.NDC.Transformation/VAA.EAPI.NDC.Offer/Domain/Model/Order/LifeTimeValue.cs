using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class LifeTimeValue
    {
        public LocalAndUTCDateTime DatePartyIdentified { get; set; }
        public int? ValueScore { get; set; }
        public string ValueGrade { get; set; }
        public CurrencyAmount LifeTimeRevenuce { get; set; }
        public IEnumerable<AnnualRevenueRecord> AnnualRevenue { get; set; }
        public IEnumerable<VIPRecord> VIPDetail { get; set; }


    }
}
