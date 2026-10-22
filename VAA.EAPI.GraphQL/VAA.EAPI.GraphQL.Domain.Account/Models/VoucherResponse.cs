namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class VoucherResponse
    {
        public IEnumerable<Voucher> VoucherDetails { get; set; }
        public ErrorResponse ErrorResponse { get; set; }
    }
}
