using System.Collections.Generic;

namespace VAA.EAPI.Web.Book.Models.Book.Confirmation
{
    public class VoucherRedemptionDetails
    {
        public IEnumerable<RedemptionOption> RedemptionOptions { get; set; }
        public BestRedemptionOption BestOption { get; set; }
        public IEnumerable<AvailableVoucher> AvailableVouchers { get; set; }
    }
}
