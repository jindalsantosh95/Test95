using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Account.Constants;

namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class Voucher
    {
        [GraphQLDescription("Member's voucher type")]
        public string Type { get; set; }

        [GraphQLDescription("Voucher source")]
        public string Source { get; set; }

        [GraphQLDescription("Voucher Description")]
        public string Description { get; set; }

        [GraphQLDescription("Voucher issue date")]
        public string IssueDate { get; set; }

        [GraphQLDescription("Voucher Expiry date")]
        public string ExpiryDate { get; set; }

        [GraphQLDescription("Voucher Status")]
        public string Status { get; set; }

        [GraphQLDescription("Voucher Number")]
        public string Number { get; set; }

        [GraphQLDescription("Voucher gifted")]
        public bool IsGifted { get; set; }
    }
}
