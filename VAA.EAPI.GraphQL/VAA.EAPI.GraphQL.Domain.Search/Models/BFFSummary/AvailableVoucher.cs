namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary
{
    public sealed class AvailableVoucher
    {
        public string? VoucherId { get; set; }
        public string? Type { get; set; }
        public long BalancePoints { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}

