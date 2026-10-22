namespace VAA.EAPI.GraphQL.Domain.Account.Models.Transaction
{
    public class StatementTextrequest
    {
        public string TransactionType { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string PartnerName { get; set; }
        public string ActivityNumber { get; set; }
        public string PartnerCode { get; set; }
        public string ExternalReservationNumber { get; set; }
        public string Quantity { get; set; }
        public string OriginAirport { get; set; }
        public string DestinationAirport { get; set; }
        public string TransactionSubType { get; set; }
        public string MemberType { get; set; }
        public string MemberMediaCode { get; set; }
        public string MarketingCarrierCode { get; set; }
        public string CabinClass { get; set; }
        public string VACodeShareFlag { get; set; }
        public string MergedMemberNumber { get; set; }
        public string MemberNumber { get; set; }
        public string StatementText { get; set; }
        public string ClassCode { get; set; }
        public string VoucherName { get; set; }
    }
}
