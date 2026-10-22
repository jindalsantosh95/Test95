namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.BrandMS
{
    public class PaidBag
    {
        public int? ExcessFirst { get; set; }
        public int? ExcessLast { get; set; }
        public Fee Fee { get; set; }
        public int? Free { get; set; }
        public string OsSequence { get; set; }
        public string Carrier { get; set; }
        public int? SeqNo { get; set; }
        public string ServiceType { get; set; }
        public string SubCode { get; set; }
        public string DisplayText { get; set; }
        public string OptionalServiceId { get; set; }
        public BagWeight Weight { get; set; }
        public BagSize Size { get; set; }
    }
}
