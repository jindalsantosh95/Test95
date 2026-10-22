namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class BaggageChargeItem
    {
        public BaggageListItem BaggageListItem { get; set; }
        public CurrencyAmount PieceExcessBaggageFee { get; set; }
        public CurrencyAmount UnitWeightExcessBaggageFee { get; set; }

    }
}
