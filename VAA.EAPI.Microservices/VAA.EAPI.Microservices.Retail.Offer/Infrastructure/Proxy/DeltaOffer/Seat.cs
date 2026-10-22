using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class Seat
    {
        public string SeatOrderId { get; set; }
        public string SeatOrderItemId { get; set; }
        public string SeatColumnName { get; set; }
        public int SeatRowNum { get; set; }
        public string SeatPositionTypeCode { get; set; }
        public IEnumerable<string> SeatCharacteristic { get; set; }
        public string SeatProductCode { get; set; }
        public bool EconomyComfort { get; set; }
        public bool ExitRow { get; set; }
        public bool ExitOverride { get; set; }
    }
}
