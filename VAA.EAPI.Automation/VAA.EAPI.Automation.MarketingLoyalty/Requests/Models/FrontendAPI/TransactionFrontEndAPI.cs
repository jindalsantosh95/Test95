namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.FrontendAPI
{
    public class TransactionFrontEndAPI
    {
        public string? transactionID { get; set; }
        public string? transactionID_AWS { get; set; }
        public string? memberNumber { get; set; }
        public string? customerId { get; set; }
        public string? memberName { get; set; }
        public string? seatNumber { get; set; }
        public string? vsRetroCom { get; set; }
        public string? fcNumber { get; set; }
        public string? boardingNumber { get; set; }
        public string? pnrName { get; set; }
        public string? owner { get; set; }
        public string? transactionStatus { get; set; }
        public DateTime activityDate { get; set; }
        public DateTime transactionProcessedDate { get; set; }
        public string? transactionType { get; set; }
        public string? partnerName { get; set; }
        public string? partnerCode { get; set; }
        public string? ptnrNumber { get; set; }
        public string? operatingFlightNumber { get; set; }
        public string? bookingClass { get; set; }
        public string? originationAirport { get; set; }
        public string? destinationAirport { get; set; }
        public string? bookingReference { get; set; }
        public string? baseMiles { get; set; }
        public int? totalMiles { get; set; }
        public int? points { get; set; }
        public string? tierPoints { get; set; }
        public string? statementDescription { get; set; }
        public string? cabinClass { get; set; }
        public string? productName { get; set; }
        public string? marketingCarrierCode { get; set; }
        public string? marketingFlightNumber { get; set; }
        public string? marketingBookingClass { get; set; }
        public string? cancellationReason { get; set; }
        public string? comments { get; set; }
        public string? ticketNumber { get; set; }
        public string? standardMiles { get; set; }
        public string? programmeBonusMiles { get; set; }
        public string? retailerBonusMiles { get; set; }
        public string? serviceRecoveryMiles { get; set; }
        public string? voucherNumber { get; set; }
        public string? voucherType { get; set; }
        public string? transactionChannel { get; set; }
        public string? operatingAuthNumber { get; set; }
        public string? canxBy { get; set; }
        public string? retroOverrideFlag { get; set; }
        public string? PDMFileType { get; set; }
        public string? retroOverrideReason { get; set; }
        public DateTime? ticketIssueDate { get; set; }
        public string? transactionSubType { get; set; }
        public string? processingLog { get; set; }
    }
}
