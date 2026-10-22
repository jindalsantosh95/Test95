using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class ItemStatus
    {
        public LocalAndUTCDateTime TimeStamp { get; set; }
        public string Status { get; set; }
        public string QoS { get; set; }
        public string QoSInformation { get; set; }
        public LocalAndUTCDateTime ActualStartDeliveryDateTime { get; set; }
        public LocalAndUTCDateTime PlannedStartDeliveryDateTime { get; set; }
        public LocalAndUTCDateTime ActualEndDeliveryDateTime { get; set; }
        public LocalAndUTCDateTime PlannedEndDeliveryDateTime { get; set; }
        public int? QuantityDelivered { get; set; }
        public IEnumerable<ItemStatusTracking> Tracking { get; set; }

    }
}
