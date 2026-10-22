namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.VSRetro
{
    public class VSRetroFlownDetail
    {
        public int Id { get; set; }

        public string OriginalBookedClass { get; set; }

        public DateTime TicketIssueDate { get; set; }

        public string RecordLocatorOperatingCarrier { get; set; }

        public string FareBasis { get; set; }

        public string TicketDesignator { get; set; }

        public string TicketNumber { get; set; }

        public string MarketingFlightNumber { get; set; }

        public string OperatingAirlineCode { get; set; }

        public string OperatingAirlineAuthNum { get; set; }

        public string OperatingFlightNumber { get; set; }

        public string ArrivalAirport { get; set; }

        public string DepartureAirport { get; set; }

        public string YQTax { get; set; }

        public DateTime DepartureDate { get; set; }

        public string BaseFare { get; set; }

        public string MarketingAirlineCode { get; set; }

        public string NetFare { get; set; }

        public string VAOtherTax { get; set; }

        public string PassengerType { get; set; }

        public string BaseMiles { get; set; }

        public string FlownCabinClass { get; set; }

        public string FlownUpgradedClass { get; set; }

        public string ClaimStatus { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string VAMarketingBookingClass { get; set; }

        public string MemberNumber { get; set; }

        public string BookClass { get; set; }

        public string SalesLocation { get; set; }

        public string IataAgentNumber { get; set; }
        public string LoyaltyProgramme { get; set; }
        public string TicketType { get; set; }
        public string SentToPartnerFlag { get; set; }
        public string VADealCode { get; set; }
    }
}
