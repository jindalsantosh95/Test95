using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("BookAvailableVoucher")]
    public sealed class AvailableVoucher
    {
        public string? VoucherId { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public long BalancePoints { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
