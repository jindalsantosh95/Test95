namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary
{
    public class VoucherDetail
    {
        public string VoucherName { get; set; }
        public string VoucherNumber { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string MaxValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime? UsedDate { get; set; }
        public string Category { get; set; }
    }
}
