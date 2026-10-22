using VAA.EAPI.GraphQL.Common.Extensions;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Baggage
{
    public class PassengerBag
    {
        public string GivenNames { get; set; }
        public string Surname { get; set; }
        public int NumberOfBags { get; set; }
        public string CurrencyCode { get; set; }
        public double TotalPaidAmount { get; set; }

        public static explicit operator PassengerBag((Infrastructure.Models.BaggageMS.Passenger passenger, Basket.Basket basket) source)
        {
            var passengerBaggageInfo = source.basket?.CheckedBaggageDetails?.PassengerBaggage?.FirstOrDefault(
                pax => pax.FirstName.Is(source.passenger.FirstName)
                    && pax.LastName.Is(source.passenger.LastName)
                    && (source.passenger.BagAmount == pax?.TotalPrice));

            return new PassengerBag
            {
                GivenNames = source.passenger.FirstName,
                Surname = source.passenger.LastName,
                NumberOfBags = passengerBaggageInfo?.NumOfPaidBags ?? 0,
                CurrencyCode = source.passenger.CurrencyCode,
                TotalPaidAmount = source.passenger?.BagAmount != null ? (double)(source.passenger.BagAmount) : 0
            };
        }
    }
}
