
namespace VAA.EAPI.GraphQL.Domain.Account.Models.Transaction
{
    public class RedemptionRequest
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public string PartnerCode { get; set; }
        public string ProductName { get; set; }
        public string ActivityDate { get; set; }
        public int Points { get; set; }
        public string Comments { get; set; }
        public string TransactionSource { get; set; }
        public string CreatedByEmailId { get; set; }
        public string CreatedByUserName { get; set; }
        public string PNR { get; set; }
        public string TicketNo { get; set; }
        public string OriginationAirport { get; set; }
        public string DestinationAirport { get; set; }
        public string BookingClass { get; set; }
        public int NoPassengers { get; set; }
        public int NoSegments { get; set; }
        public string VoucherNumber { get; set; }
        public string VoucherUsage { get; set; }
    }
}
