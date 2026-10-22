namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers
{
    public class RedeemVoucherRequest
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public string InitiatedBy { get; set; }
        public string PartnerCode { get; set; }
        public string PartnerName { get; set; }
        public string TransactionChannel { get; set; }
        public string RedemptionType { get; set; }
        public RedeemVoucherFlightDetails FlightDetails { get; set; }
        public VoucherDetails VoucherDetails { get; set; }
    }
}
