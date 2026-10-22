using System;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop
{

    public class BookingReferences
    {
        public string AirlineDesignator { get; set; }
        public string RLOC { get; set; }
        public DateTime? RLOCCreateDate { get; set; }
        public string RLOCCreateDateTime { get; set; }
        public string ExternalRecordLocator { get; set; }
    }

}