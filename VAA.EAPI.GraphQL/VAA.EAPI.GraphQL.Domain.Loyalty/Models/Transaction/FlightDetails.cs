using HotChocolate;
using System;
using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction
{
    [GraphQLName("loyaltyTransactionFlightDetails")]
    public class FlightDetails
    {
        public DateTime? BookingDate { get; set; }
        public string BookingClass { get; set; }
        public string DestinationAirport { get; set; }
        public string ExternalReservationNumber { get; set; }
        public string FareBasis { get; set; }
        public string FareBasisCode { get; set; }
        public string GDSRecordLocator { get; set; }
        public DateTime? InternalFlightDate { get; set; }
        public string InternalOrigin { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string MarketingPartnerCode { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string OriginAirportcode { get; set; }
        public string PNRName { get; set; }
        public DateTime? DepartureDate { get; set; }
        public string Ticketnumber { get; set; }
        public string TravelClass { get; set; }
        public string PassengerTypeCheckDigit { get; set; }
        public string RetroOverrideReason { get; set; }
        public string DocumentType { get; set; }
        public string RetroOverrideFlag { get; set; }
        public string BoardingNumber { get; set; }
        public string NumberOfSegments { get; set; }
        public string FirstNameAsPerDocument { get; set; }
        public string LastNameAsPerDocument { get; set; }
        public string SecondaryDocumentType { get; set; }
        public string Seatnumber { get; set; }
        public string CodeShareFlag { get; set; }
        public string MarketingBookingClass { get; set; }
        public string SecondaryDocumentStatus { get; set; }
        public string PrimaryDocumentStatus { get; set; }
        public string RetroFlag { get; set; }
        public string CabinClass { get; set; }
        public string OriginalOrigin { get; set; }
        public string OriginalDestination { get; set; }
        public string OriginalBookingClass { get; set; }
        public DateTime? OriginalActivityDate { get; set; }
        public string OriginalFlightNumber { get; set; }
        public string PassengerType { get; set; }
        public string ForceCredit { get; set; }
        public string VABaseFare { get; set; }
        public string VANetFare { get; set; }
        public string VARetroComments { get; set; }
        public string VAOperatingAirlineComments { get; set; }
        public string VAAgentSetAddress { get; set; }
        public string VAAgentDutyCode { get; set; }
        public string VAAgencyIATANumber { get; set; }
        public string VAAgentInitial { get; set; }
        public string VAJFTransactionId { get; set; }
        public string VAJFUpdateTransactionId { get; set; }
        public string VAPArtialFlownIndicator { get; set; }
        public string VAPseudoCityCode { get; set; }
        public string VAPassengerTypeGroups { get; set; }
        public string VARewardType { get; set; }
        public string VASourceTransactionCode { get; set; }
        public string VAMarketedCabin { get; set; }
        public string VAOtherTax { get; set; }
        public string VABonusPromoId { get; set; }
        public string VAPostingTypeCode { get; set; }
        public string UserCreated { get; set; }
        public string CommunicationSent { get; set; }
        public string MIFlag { get; set; }
        public string RetroResponseErrorCode { get; set; }
        public string RetroResponseErrorMessage { get; set; }
        public string BookingType { get; set; }
        public string VAETicketIndicator { get; set; }
        public string TransactionId { get; set; }
        public string PartnerProcessedDate { get; set; }
        public string Distance { get; set; }
        public string Quantity { get; set; }
        public string TaxAmount { get; set; }
        public string OperatingAirlineAuthNumber { get; set; }
        public IEnumerable<PassengerDetails> PassengerDetails { get; set; }
    }
}
