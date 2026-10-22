using System.Collections.Generic;

namespace VAA.EAPI.Web.Book.Models.Book.Confirmation
{
    public class VoucherRedemptionSelection
    {
        public int? MaxVouchersToUse { get; set; }
        public IEnumerable<string> SelectedVoucherIds { get; set; }
        public int? SelectedOptionId { get; set; }
    }
}
