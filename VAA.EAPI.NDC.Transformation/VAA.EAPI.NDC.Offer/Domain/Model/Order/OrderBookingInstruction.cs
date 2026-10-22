namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class OrderBookingInstruction
    {
        public string ItemType { get; set; }
        public string ItemCode { get; set; }
        public string ItemText { get; set; }
        public bool? PNRLevel { get; set; }
        public bool? SegmentLevel { get; set; }
        public bool? NameRelated { get; set; }

    }
}
