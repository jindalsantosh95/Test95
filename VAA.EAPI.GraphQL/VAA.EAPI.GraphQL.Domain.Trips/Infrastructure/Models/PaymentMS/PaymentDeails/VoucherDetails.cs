using HotChocolate;
using NodaTime;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.Common;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS.PaymentDeails
{
    [GraphQLName("GraphqlTripsVoucherMethod")]
    public class VoucherMethod
    {
        public string VoucherType { get; set; }
        public string Issuer { get; set; }
        public string VoucherNumber { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public LocalDate? IssueLocalDate { get; set; }
        public LocalDate? ExpiryLocalDate { get; set; }
        public CurrencyAmount Amount { get; set; }
        public int? Percentage { get; set; }
    }
}
