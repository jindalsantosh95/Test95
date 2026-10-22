using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Account.Constants;

namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class AccountMemberVoucher
    {
        [GraphQLDescription("Member's voucher type")]
        public VoucherTypes Type { get; set; }

        [GraphQLDescription("Voucher Name")]
        public string Name { get; set; }

        [GraphQLDescription("Voucher source")]
        public VoucherSource Source { get; set; }

        [GraphQLDescription("Voucher Description")]
        public string Description { get; set; }

        [GraphQLDescription("Voucher issue date")]
        public string IssueDate { get; set; }

        [GraphQLDescription("Voucher Expiry date")]
        public string ExpiryDate { get; set; }

        [GraphQLDescription("Voucher Status")]
        public VoucherStatus? Status { get; set; }

        [GraphQLDescription("Voucher Number")]
        public string Number { get; set; }

        [GraphQLDescription("Voucher Category")]
        public VoucherCategory Category { get; set; }

        [GraphQLDescription("Voucher gifted")]
        public bool IsGifted { get; set; }

        [GraphQLDescription("Voucher Points")]
        public string Points { get; set; }
    }
}
