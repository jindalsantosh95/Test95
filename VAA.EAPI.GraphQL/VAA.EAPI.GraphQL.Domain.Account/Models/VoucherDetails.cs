namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class VoucherDetails
    {
        public IEnumerable<VoucherCategoryInfo> Categories { get; set; } = [];
        public IEnumerable<AccountMemberVoucher> Vouchers { get; set; } = [];
        public int MemberTotalVoucherCount { get; set; }
        public int PointsThreshold { get; set; }
    }
}
