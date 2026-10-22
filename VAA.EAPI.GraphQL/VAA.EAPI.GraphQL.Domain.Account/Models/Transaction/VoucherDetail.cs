namespace VAA.EAPI.GraphQL.Domain.Account.Models.Transaction
{
    public class VoucherDetail
    {
        public string VoucherName { get; set; }
        public string VoucherNumber { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string StartDate { get; set; }
        public string ExpiryDate { get; set; }
        public string Source { get; set; }
        public string MaxValue { get; set; }
        public string Category { get; set; }
        public bool IsGifted { get; set; }
    }
}
