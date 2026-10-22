namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.BackendAPI
{

    public class RetrieveAwardDetails
    {
        public string VoucherID { get; set; }
        public VoucherDetails VoucherDetails { get; set; }
    }

    public class VoucherDetails
    {
        public string VoucherName { get; set; }
        public string VoucherNumber { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string StartDate { get; set; }
        public string ExpiryDate { get; set; }
        public string TransactionSource { get; set; }
    }


}
