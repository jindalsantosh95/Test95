using VAA.EAPI.GraphQL.Domain.Account.Models;

namespace VAA.EAPI.GraphQL.Domain.Account.Interface
{
    public interface IVoucherQueryProvider
    {
        Task<IEnumerable<Voucher>> GetVoucherQuery(string customerId, int page, int pageSize);
        Task<VoucherDetails> GetAccountMemberVouchers(string customerId, int page, int pageSize);
    }
}
