using VAA.EAPI.GraphQL.Domain.Book.Models.Request.VoucherRedemption;

namespace VAA.EAPI.GraphQL.Domain.Book.Interfaces
{
    public interface IVoucherRedemptionProvider
    {
        Task RedeemAsync(VoucherRedemptionRequest request);
        Task UpdatePnrAsync(VoucherPnrUpdateRequest request);
        Task RollbackAsync(VoucherRollbackRequest request);
    }
}
