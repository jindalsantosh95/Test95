using System.Globalization;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.ConfirmBaggage
{
    public class Passenger
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int NumberOfExtraBags { get; set; }
        public string CurrencyCode { get; set; }
        public decimal BagAmount { get; set; }

        public static explicit operator Passenger(DeltaSsci.PurchaseSummaryPassengerData source)
        {
            var baggageFeeProduct = source?.ProductList?.FirstOrDefault(x => x.Description.Is("Baggage Fee"));

            if (baggageFeeProduct is null)
            {
                return null;
            }

            if (!TryGetBagAmount(
                baggageFeeProduct.ProductTotalAmt,
                baggageFeeProduct.ProductTotalAmtDecimal,
                out var bagAmount))
            {
                return null;
            }

            return new Passenger
            {
                FirstName = source.FirstName,
                LastName = source.LastName,
                NumberOfExtraBags = baggageFeeProduct.NumberOfBags,
                CurrencyCode = baggageFeeProduct.Currency ?? string.Empty,
                BagAmount = bagAmount
            };
        }

        public static bool TryGetBagAmount(string amount, string decimalAmount, out decimal bagAmount)
        {
            var value = decimalAmount.IsNullOrEmpty() ? amount : $"{amount}.{decimalAmount}";

            return decimal.TryParse(
                value,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out bagAmount);
        }
    }
}
