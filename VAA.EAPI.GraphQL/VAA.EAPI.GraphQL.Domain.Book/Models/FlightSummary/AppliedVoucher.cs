using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("BookAppliedVoucher")]
    public sealed class AppliedVoucher
    {
        public string? VoucherId { get; set; }
        public long PointsUsed { get; set; }
    }
}
