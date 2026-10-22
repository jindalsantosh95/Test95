namespace VAA.EAPI.NDC.Order.Infrastructure.Model.BrandMS
{
    public class FreeBag
    {
        public int? Pieces { get; set; }
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
