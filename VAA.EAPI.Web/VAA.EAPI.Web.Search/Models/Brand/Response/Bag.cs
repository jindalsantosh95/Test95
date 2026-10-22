namespace VAA.EAPI.Web.Search.Models.Brand.Response
{
    public class Bag
    {
        public string Carrier { get; set; }
        public int SeqNo { get; set; }
        public string ServiceType { get; set; }
        public string SubCode { get; set; }
        public string DisplayText { get; set; }
        public string OptionalServiceId { get; set; }
        public BagWeight Weight { get; set; }
        public BagSize Size { get; set; }
    }
}