using VAA.EAPI.GraphQL.Domain.Account.Constants;

namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class VoucherCategoryInfo
    {
        public VoucherCategory Code { get; set; }
        public int SortOrder { get; set; }
    }
}
