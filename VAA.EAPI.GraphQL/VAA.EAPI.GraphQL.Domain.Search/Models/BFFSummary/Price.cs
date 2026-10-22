namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary
{
    public class Price
    {
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public decimal Tax { get; set; }
        public string? PriceDifference { get; set; }
        public string? PriceDifferenceSign { get; set; }
        public decimal AmountIncludingTax { get; set; }
        public string? AwardPoints { get; set; }
        public string? VoucherPoints { get; set; }
        public string? PointsToPay { get; set; }

        public Price() { }

        public Price(IEnumerable<FlightOffer.Price> source, decimal selectedOfferPrice)
        {
            var amountIncTax = source.First(sp => sp.PriceComponentType == FlightOffer.Price.PriceComponentTypes.BasePriceEnum).Amount;
            Amount = decimal.Parse(source.First(sp => sp.PriceComponentType == FlightOffer.Price.PriceComponentTypes.BaseFareEnum).Amount.MonetaryAmount);
            Currency = amountIncTax.BaseCurrency;
            Tax = decimal.Parse(source.First(sp => sp.PriceComponentType == FlightOffer.Price.PriceComponentTypes.TotalTaxEnum).Amount.MonetaryAmount);
            AmountIncludingTax = decimal.Parse(amountIncTax.MonetaryAmount);
            PriceDifference = Math.Abs(decimal.Parse(amountIncTax.MonetaryAmount) - selectedOfferPrice).ToString();
            PriceDifferenceSign = decimal.Parse(amountIncTax.MonetaryAmount) > selectedOfferPrice ? "+" : "-";
        }

        public static explicit operator Price(List<FlightOffer.Price> source)
        {
            var amountIncTax = source.First(sp => sp.PriceComponentType == FlightOffer.Price.PriceComponentTypes.BasePriceEnum).Amount;
            return new()
            {
                Amount = decimal.Parse(source.First(sp => sp.PriceComponentType == FlightOffer.Price.PriceComponentTypes.BaseFareEnum).Amount.MonetaryAmount),
                Currency = amountIncTax.BaseCurrency,
                Tax = decimal.Parse(source.First(sp => sp.PriceComponentType == FlightOffer.Price.PriceComponentTypes.TotalTaxEnum).Amount.MonetaryAmount),
                AmountIncludingTax = decimal.Parse(amountIncTax.MonetaryAmount),
                AwardPoints = source.FirstOrDefault(x => x.PriceComponentType == Models.FlightOffer.Price.PriceComponentTypes.PointsEnum)?.PointsAmount?.PointsCount.ToString(),
            };
        }
    }
}
