using System;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class VIPRecord
    {
        public Object IPType { get; set; }
        public string Entity { get; set; }
        public DateTime? EffectiveStart { get; set; }
        public DateTime? EffectiveEnd { get; set; }

    }
}
