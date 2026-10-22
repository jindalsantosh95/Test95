using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.BaggageMS;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Basket
{
    public class PsgBaggageInfo
    {
        public string PassengerNiN { get; set; }
        public string BaggageOfferId { get; set; }
        public int? BaggageCount { get; set; }
        public decimal? TotalPrice { get; set; }
        public string CurrencyCode { get; set; }
        public int DecimalPrecisionCount { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? NumOfFreeBags { get; set; }
        public int? NumOfPaidBags { get; set; }

        public static explicit operator PsgBaggageInfo((Baggage.SelectedBaggageOffer selectedOffer, PassengerAllowance passengerAllowance, Infrastructure.Models.CheckInMS.CheckInResponse checkInResponse) source)
        {
            var allowedBaggageOffers = source.passengerAllowance?.BaggageOffers;

            var passengerDetails = source.checkInResponse?.Passengers?.FirstOrDefault(
                pax => pax.PassengerId.Split("|")[0] == source.selectedOffer.PassengerNiN);

            return new PsgBaggageInfo
            {
                PassengerNiN = source.selectedOffer.PassengerNiN,
                BaggageOfferId = source.selectedOffer.BaggageOfferId,
                BaggageCount = (int?)source.selectedOffer.BaggageCount,
                TotalPrice = source.selectedOffer.TotalPrice,
                CurrencyCode = source.selectedOffer.CurrencyCode,
                DecimalPrecisionCount = source.selectedOffer.DecimalPrecisionCount,
                FirstName = passengerDetails?.GivenNames,
                LastName = passengerDetails?.Surname,
                NumOfFreeBags = allowedBaggageOffers?.Count(o => o.Price?.Amount == 0),
                NumOfPaidBags = allowedBaggageOffers?.First(item => item.baggageOfferId.Is(source.selectedOffer.BaggageOfferId)).BaggageCount - allowedBaggageOffers?.Count(o => o.Price?.Amount == 0)
            };
        }
    }
}
