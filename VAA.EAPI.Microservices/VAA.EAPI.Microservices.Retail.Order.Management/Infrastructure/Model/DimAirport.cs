using System;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Model
{
    public class DimAirport
    {
        public int AirportId { get; set; }
        public string IataAirportCode { get; set; }
        public string IcaoAirportCode { get; set; }
        public string Name { get; set; }
        public string CityIataCode { get; set; }
        public string RouteSwareAptNme { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public decimal? DecimalLatitude { get; set; }
        public decimal? DecimalLongitude { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string State { get; set; }
        public string StateName { get; set; }
        public string GlobalRegion { get; set; }
        public int? Wac { get; set; }
        public string WacName { get; set; }
        public int? AirportAltitudeFt { get; set; }
        public string FromUtcOffset { get; set; }
        public string ToUtcOffset { get; set; }
        public string DstOffset { get; set; }
        public string ToUtcDtsOffset { get; set; }
        public string WikiLink { get; set; }
        public bool? IsDlHub { get; set; }
        public bool? IsAfklHub { get; set; }
        public bool? IsVsHub { get; set; }
        public DateTime EffectiveStartDate { get; set; }
        public DateTime EffectiveEndDate { get; set; }
        public bool CurrentFlag { get; set; }
        public bool InferredFlag { get; set; }

        //public virtual ICollection<FactOrderSegment> FactOrderSegmentAirportBoardpoints { get; set; }
        //public virtual ICollection<FactOrderSegment> FactOrderSegmentAirportOffpoints { get; set; }
        //public virtual ICollection<FactPnrSegment> FactPnrSegmentAirportBoardpoints { get; set; }
        //public virtual ICollection<FactPnrSegment> FactPnrSegmentAirportOffpoints { get; set; }
        //public virtual ICollection<FactPnrSegmentHistory> FactPnrSegmentHistoryAirportBoardpoints { get; set; }
        //public virtual ICollection<FactPnrSegmentHistory> FactPnrSegmentHistoryAirportOffpoints { get; set; }
    }
}
