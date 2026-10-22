namespace VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.BaggageAllowance
{
    public class BaggageOffer
    {
        public int BaggageCount { get; set; }
        public string BaggageOfferId { get; set; }
        public BaggagePrice Price { get; set; }

        public static explicit operator BaggageOffer((DeltaSsci.BaggageData baggageData, int index, double baggageAmount) source)
        {
            return new BaggageOffer
            {
                BaggageOfferId = source.baggageData.OfferItemId,
                BaggageCount = source.index + 1,
                Price = new BaggagePrice
                {
                    Amount = (decimal)source.baggageData.BaggagePrice,
                    Currency = source.baggageData.Currency,
                    AdditionalBagPrice = (decimal)source.baggageAmount,
                    DecimalPrecisionCount = source.baggageData.DecimalPrecisionCnt
                }
            };
        }
    }
}
