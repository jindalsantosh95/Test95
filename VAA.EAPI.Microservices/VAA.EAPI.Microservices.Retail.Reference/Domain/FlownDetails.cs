using System;

namespace VAA.EAPI.Microservices.Retail.Reference.Domain
{
    public class FlownDetails
    {
        public int Id { get; set; }
        public string OriginalBookedClass { get; set; }
        public string FlownUpgradedClass { get; set; }
        public string OperatingAirlineCode { get; set; }
        public DateTime DepartureDate { get; set; }
        public string FareBasis { get; set; }        
        public string TicketDesignator { get; set; }
        public string PassengerList { get; set; }
        public string RecordLocatorOperatingCarrier { get; set; }
        public string OriginCountryCode {  get; set; }
        public string DestinationCountryCode { get; set; }       
    }
}
