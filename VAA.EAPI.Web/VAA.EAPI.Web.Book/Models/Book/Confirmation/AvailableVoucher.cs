using System;

namespace VAA.EAPI.Web.Book.Models.Book.Confirmation
{
    public class AvailableVoucher
    {
        public string VoucherId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public long BalancePoints { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
