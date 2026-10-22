namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class PriceOption
    {
        public string OptionType { get; set; }
        public CurrencyAmount Price { get; set; }
        public int? AwardPoints { get; set; }
        public CurrencyAmount AwardConversion { get; set; }
        public CurrencyAmount MinPoints { get; set; }


    }
}
