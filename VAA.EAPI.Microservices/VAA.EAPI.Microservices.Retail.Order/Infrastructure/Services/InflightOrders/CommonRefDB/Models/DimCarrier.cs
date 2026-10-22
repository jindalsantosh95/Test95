using System;

#nullable disable

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services.InflightOrders.CommonRefDB.Models
{
    public class DimCarrier
    {
        //public DimCarrier()
        //{
        //    FactOrderSegmentCarrierMkts = new HashSet<FactOrderSegment>();
        //    FactOrderSegmentCarrierOpas = new HashSet<FactOrderSegment>();
        //    FactPnrSegmentCarrierMkts = new HashSet<FactPnrSegment>();
        //    FactPnrSegmentCarrierOpas = new HashSet<FactPnrSegment>();
        //    FactPnrSegmentHistoryCarrierMkts = new HashSet<FactPnrSegmentHistory>();
        //    FactPnrSegmentHistoryCarrierOpas = new HashSet<FactPnrSegmentHistory>();
        //    FactPnrSegmentHistoryIssuingAirlines = new HashSet<FactPnrSegmentHistory>();
        //    FactPnrSegmentIssuingAirlines = new HashSet<FactPnrSegment>();
        //}

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

        //public virtual ICollection<FactOrderSegment> FactOrderSegmentCarrierMkts { get; set; }
        //public virtual ICollection<FactOrderSegment> FactOrderSegmentCarrierOpas { get; set; }
        //public virtual ICollection<FactPnrSegment> FactPnrSegmentCarrierMkts { get; set; }
        //public virtual ICollection<FactPnrSegment> FactPnrSegmentCarrierOpas { get; set; }
        //public virtual ICollection<FactPnrSegmentHistory> FactPnrSegmentHistoryCarrierMkts { get; set; }
        //public virtual ICollection<FactPnrSegmentHistory> FactPnrSegmentHistoryCarrierOpas { get; set; }
        //public virtual ICollection<FactPnrSegmentHistory> FactPnrSegmentHistoryIssuingAirlines { get; set; }
        //public virtual ICollection<FactPnrSegment> FactPnrSegmentIssuingAirlines { get; set; }
    }
}
