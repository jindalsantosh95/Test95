using System;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class BookingReference
    {
        public string AirlineDesignator { get; set; }
        public string RLOC { get; set; }
        public DateTime? RLOCCreateDate { get; set; }
        public string ExternalRecordLocator { get; set; }
        public string ExternalSystem { get; set; }
        public Link BookingDetail { get; set; }
        public string AirlineDesigCode { get; set; }

    }
}
