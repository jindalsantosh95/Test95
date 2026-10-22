using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class FlightDetails
    {
        [JsonProperty("Distance")]
        public string Distance { get; set; }

        [JsonProperty("BookingDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? BookingDate { get; set; }

        [JsonProperty("BookingClass")]
        public string BookingClass { get; set; }

        [JsonProperty("DestinationAirport")]
        public string DestinationAirport { get; set; }

        [JsonProperty("ExternalReservationNumber")]
        public string ExternalReservationNumber { get; set; }

        [JsonProperty("FareBasis")]
        public string FareBasis { get; set; }

        [JsonProperty("FareBasisCode")]
        public string FareBasisCode { get; set; }

        [JsonProperty("GDSRecordLocator")]
        public string GDSRecordLocator { get; set; }

        [JsonProperty("InternalFlightDate")]
        public DateTimeOffset? InternalFlightDate { get; set; }

        [JsonProperty("InternalOrigin")]
        public string InternalOrigin { get; set; }

        [JsonProperty("MarketingFlightNumber")]
        public string MarketingFlightNumber { get; set; }

        [JsonProperty("OperatingFlightNumber")]
        public string OperatingFlightNumber { get; set; }

        [JsonProperty("OriginAirportcode")]
        public string OriginAirportcode { get; set; }

        [JsonProperty("PNRName")]
        public string PNRName { get; set; }

        [JsonProperty("DepartureDate")]
        public DateTimeOffset? DepartureDate { get; set; }

        [JsonProperty("Ticketnumber")]
        public string Ticketnumber { get; set; }

        [JsonProperty("TravelClass")]
        public string TravelClass { get; set; }

        [JsonProperty("PassengerTypeCheckDigit")]
        public string PassengerTypeCheckDigit { get; set; }

        [JsonProperty("RetroOverrideReason")]
        public string RetroOverrideReason { get; set; }

        [JsonProperty("DocumentType")]
        public string DocumentType { get; set; }

        [JsonProperty("RetroOverrideFlag")]
        public string RetroOverrideFlag { get; set; }

        [JsonProperty("NumberOfSegments")]
        public string NumberOfSegments { get; set; }

        [JsonProperty("FirstNameAsPerDocument")]
        public string FirstNameAsPerDocument { get; set; }

        [JsonProperty("LastNameAsPerDocument")]
        public string LastNameAsPerDocument { get; set; }

        [JsonProperty("SecondaryDocumentType")]
        public string SecondaryDocumentType { get; set; }

        [JsonProperty("Seatnumber")]
        public string Seatnumber { get; set; }

        [JsonProperty("CodeShareFlag")]
        public string CodeShareFlag { get; set; }

        [JsonProperty("MarketingBookingClass")]
        public string MarketingBookingClass { get; set; }

        [JsonProperty("SecondaryDocumentStatus")]
        public string SecondaryDocumentStatus { get; set; }

        [JsonProperty("PrimaryDocumentStatus")]
        public string PrimaryDocumentStatus { get; set; }

        [JsonProperty("RetroFlag")]
        public string RetroFlag { get; set; }

        [JsonProperty("CabinClass")]
        public string CabinClass { get; set; }

        [JsonProperty("OriginalOrigin")]
        public string OriginalOrigin { get; set; }

        [JsonProperty("OriginalDestination")]
        public string OriginalDestination { get; set; }

        [JsonProperty("OriginalBookingClass")]
        public string OriginalBookingClass { get; set; }

        [JsonProperty("OriginalActivityDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? OriginalActivityDate { get; set; }

        [JsonProperty("OriginalFlightNumber")]
        public string OriginalFlightNumber { get; set; }

        [JsonProperty("PassengerType")]
        public string PassengerType { get; set; }

        [JsonProperty("ForceCredit")]
        public string ForceCredit { get; set; }

        [JsonProperty("VABaseFare")]
        public string VABaseFare { get; set; }

        [JsonProperty("VARewardType")]
        public string VARewardType { get; set; }

        [JsonProperty("UserCreated")]
        public string UserCreated { get; set; }

        [JsonProperty("CommunicationSent")]
        public string CommunicationSent { get; set; }

        [JsonProperty("MIFlag")]
        public string MIFlag { get; set; }

        [JsonProperty("RetroResponseErrorCode")]
        public string RetroResponseErrorCode { get; set; }

        [JsonProperty("RetroResponseErrorMessage")]
        public string RetroResponseErrorMessage { get; set; }

        [JsonProperty("BookingType")]
        public string BookingType { get; set; }

        [JsonProperty("VAETicketIndicator")]
        public string VAETicketIndicator { get; set; }

        [JsonProperty("MarketingPartnerCode")]
        public string MarketingPartnerCode { get; set; }

        [JsonProperty("PartnerProcessedDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? PartnerProcessedDate { get; set; }

        [JsonProperty("Quantity")]
        public string Quantity { get; set; }

        [JsonProperty("TaxAmount")]
        public string TaxAmount { get; set; }

        [JsonProperty("PassengerDetails")]
        public List<PassengerDetails> PassengerDetails { get; set; }

        [JsonProperty("OperatingAirlineAuthNumber")]
        public string OperatingAirlineAuthNumber { get; set; }
    }
}
