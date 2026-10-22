namespace VAA.EAPI.Microservices.Retail.Order.Management.Domain.Remarks
{
    public class AddSpecialRemarkRequest
    {
        public string Pnr { get; set; }
        public string Location { get; set; }
        public string RemarkType { get; set; }
        public string RemarkSubType { get; set; }
        public VoucherDetails VoucherDetails { get; set; }
    }
}
