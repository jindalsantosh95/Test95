using HotChocolate;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    [GraphQLName("TripsSeatAmount")]
    public class CurrencyAmount
    {
        public string MonetaryAmount { get; set; }
        public string BaseCurrency { get; set; }
        public int DecimalPrecisionCount { get; set; }
        public decimal PriceDifference { get; set; }
        public string PriceDifferenceSign { get; set; }

        public static explicit operator CurrencyAmount((Basket basket, string seatNumber) source)
        {
            (var basket, var seatNumber) = source;
            if (source.basket == null) return null;

            var amount = source.basket.FlightPassengerSeatMap.Where(fl => fl.SeatNumber == seatNumber && !fl.SelectedSeatOfferId.IsNullOrEmpty()).Sum(price => price.Price.Amount).ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            var monetaryAmount = amount.Replace(".", string.Empty);

            return new CurrencyAmount
            {
                MonetaryAmount = monetaryAmount,
                BaseCurrency = source.basket.FlightPassengerSeatMap.First(fl => fl.Price.Amount > 0m).Price.Currency,
                DecimalPrecisionCount = 2
            };
        }

        public static explicit operator CurrencyAmount(Basket basket)
        {
            if (basket == null) return null;

            var amount = basket.FlightPassengerSeatMap.Where(fl => !fl.SelectedSeatOfferId.IsNullOrEmpty()).Sum(price => price.Price.Amount).ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            var monetaryAmount = amount.Replace(".", string.Empty);
           
            return new CurrencyAmount
            {
                MonetaryAmount = monetaryAmount,
                BaseCurrency = basket.FlightPassengerSeatMap.First(fl => fl.Price.Amount > 0m).Price.Currency,
                DecimalPrecisionCount = 2
            };
        }
    }
}