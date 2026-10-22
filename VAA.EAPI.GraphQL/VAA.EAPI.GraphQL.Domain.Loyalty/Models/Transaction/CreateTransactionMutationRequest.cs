using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction
{
    public class CreateTransactionMutationRequest
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public string MemberStatus { get; set; }
        public string PartnerCode { get; set; }
        public string ProductName { get; set; }
        public DateTime? ActivityDate { get; set; }
        public string Comments { get; set; }
        public int Points { get; set; }
        public int TierPoints { get; set; }
        public string TransactionSource { get; set; }
        public string CreatedBy { get; set; }
        public int NoOfPassengers { get; set; }
        public int NoOfSegments { get; set; }
        public string PointOverrideFlag { get; set; }
        public TransactionMutationFlightDetails FlightDetails { get; set; }
        public TransactionMutationVocuherDetails VoucherDetails { get; set; }
    }
}
