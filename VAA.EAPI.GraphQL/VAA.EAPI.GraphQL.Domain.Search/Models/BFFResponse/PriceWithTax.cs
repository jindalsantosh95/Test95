using VAA.EAPI.GraphQL.Common.Extensions;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    public class PriceWithTax : Price
    {

        public decimal? Tax { get; set; }

        public decimal? AmountIncludingTax { get; set; }

        public string? PriceDifference { get; set; }

        public string? PriceDifferenceSign { get; set; }

        public string? AwardPointsDifference { get; set; }

        public string? AwardPointsDifferenceSign { get; set; }

        public static explicit operator PriceWithTax?((IEnumerable<FlightOffer.Price>? salePrice, bool isAwardSearch) source)
        {
            if (source.salePrice == null) return null;

            var amountIncTax = source.salePrice.First(sp => sp.PriceComponentType == FlightOffer.Price.PriceComponentTypes.BasePriceEnum).Amount;

            var priceWithTax = new PriceWithTax
            {
                Amount = source.salePrice != null ? decimal.Parse(source.salePrice.First(sp => sp.PriceComponentType == FlightOffer.Price.PriceComponentTypes.BaseFareEnum).Amount.MonetaryAmount) : null,
                Currency = amountIncTax.BaseCurrency,
                Tax = source.salePrice != null ? decimal.Parse(source.salePrice.First(sp => sp.PriceComponentType == FlightOffer.Price.PriceComponentTypes.TotalTaxEnum).Amount.MonetaryAmount) : null,
                AmountIncludingTax = amountIncTax.MonetaryAmount.Parse<decimal>(),
                PriceDifference = amountIncTax.PriceDifference == 0 ? "0.00" : amountIncTax.PriceDifference.ToString(),
                PriceDifferenceSign = amountIncTax.PriceDifferenceSign,
                AwardPoints = source.isAwardSearch
                    ? source.salePrice.First(x => x.PriceComponentType == FlightOffer.Price.PriceComponentTypes.PointsEnum).PointsAmount.PointsCount.ToString()
                    : null,
                AwardPointsDifference = source.isAwardSearch && source.salePrice.First(x => x.PriceComponentType == FlightOffer.Price.PriceComponentTypes.PointsEnum)?.DifferenceDetails != null
                    ? source.salePrice.First(x => x.PriceComponentType == FlightOffer.Price.PriceComponentTypes.PointsEnum).DifferenceDetails.PointsDifference.ToString()
                    : null,
                AwardPointsDifferenceSign = source.isAwardSearch && source.salePrice.First(x => x.PriceComponentType == FlightOffer.Price.PriceComponentTypes.PointsEnum)?.DifferenceDetails != null
                    ? source.salePrice.First(x => x.PriceComponentType == FlightOffer.Price.PriceComponentTypes.PointsEnum).DifferenceDetails.PointsDifferenceSign
                    : null
            };
            return priceWithTax;
        }
    }
}
