namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.FrontendAPI
{
    public class TransactionSubset
    {
        public Pagination pagination { get; set; }
        public List<Item> items { get; set; }
    }
    public class Pagination
    {
        public int? totalCount { get; set; }
        public int? pageSize { get; set; }
        public int? currentPage { get; set; }
        public int? totalPages { get; set; }
        public bool hasPrevious { get; set; }
        public bool hasNext { get; set; }
    }
    public class Item
    {
        public string? transactionID { get; set; }
        public string? transactionID_AWS { get; set; }
        public string? owner { get; set; }
        public string? transactionStatus { get; set; }
        public string? activityDate { get; set; }
        public string? activityType { get; set; }
        public string? transactionProcessedDate { get; set; }
        public string? transactionType { get; set; }
        public string? partnerName { get; set; }
        public string? partnerCode { get; set; }
        public string? operatingFlightNumber { get; set; }
        public string? bookingClass { get; set; }
        public string? originationAirport { get; set; }
        public string? destinationAirport { get; set; }
        public string? bookingReference { get; set; }
        public int? baseMiles { get; set; }
        public int? totalMiles { get; set; }
        public int? tierPoints { get; set; }
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
        public string? ptnrNumber { get; set; }
        public string? transactionChannel { get; set; }
        public string? operatingAuthNumber { get; set; }
        public string? canxBy { get; set; }
        public string? retroOverrideFlag { get; set; }
        public string? PDMFileType { get; set; }
        public string? retroOverrideReason { get; set; }
        public string? ticketIssueDate { get; set; }
        public string? transactionSubType { get; set; }
        public string? processingLog { get; set; }
    }


}


