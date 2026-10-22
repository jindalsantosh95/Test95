namespace VAA.EAPI.Microservices.Retail.Order.Domain.Order.Remarks
{
    public class AddSpecialRemarkRequest
    {
        public string Pnr { get; set; }
        public string Location { get; set; }
        public string RemarkType { get; set; }
        public string RemarkSubType { get; set; }
        public VoucherRemarkDetails VoucherDetails { get; set; }
    }
}
