namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.VoucherRedemption
{
    public sealed class VoucherRedemptionAcceptedException : HttpRequestException
    {
        public VoucherRedemptionAcceptedException(string? voucherId, Exception innerException)
            : base("Voucher redemption was accepted but its VRed callback failed.", innerException)
        {
            VoucherId = voucherId;
        }

        public string? VoucherId { get; }
    }
}
