namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class PriceComponent
    {
        public string PriceComponentType { get; set; }
        public string PriceComponentName { get; set; }
        public CurrencyAmount Amount { get; set; }
        public CurrencyAmount BaseCurrencyAmount { get; set; }
        public string PriceComponentDescription { get; set; }


    }
}
