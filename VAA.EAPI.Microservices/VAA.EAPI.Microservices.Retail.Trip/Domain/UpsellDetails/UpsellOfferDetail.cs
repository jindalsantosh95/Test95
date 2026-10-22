namespace VAA.EAPI.Microservices.Retail.Trip.Domain.UpsellDetails
{
    public class UpsellOfferDetail
    {
        public int Ordinal { get; set; }
        public OfferCodeType OfferType { get; set; }
        public string CurrencyCode { get; set; }
        public string BaseAmt { get; set; }
        public string TotalAmount { get; set; }
        public LoyaltyProgram LoyaltyProgram { get; set; }

        public static explicit operator UpsellOfferDetail(Infrastructure.Proxy.DeltaUpsellOffers.UpsellOfferDetail source)
        {
            if (source == null) return null;

            return new UpsellOfferDetail
            {
                Ordinal = source.Ordinal,
                OfferType = (OfferCodeType)source.OfferType,
                CurrencyCode = source.CurrencyCode,
                BaseAmt = source.BaseAmt,
                TotalAmount = source.TotalAmount,
                LoyaltyProgram = (LoyaltyProgram)source.LoyaltyProgram
            };
        }
    }
}

