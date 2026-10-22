using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.BaggageMS;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Baggage;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Basket
{
    public class CreateBasketRequest
    {
        public Guid BasketId { get; set; }
        public PnrDetails BookingDetails { get; set; }
        public CheckedBaggage CheckedBaggageDetails { get; set; }
        public ContactRecord ContactDetails { get; set; }

        public static explicit operator CreateBasketRequest((SaveBaggageRequest request, Infrastructure.Models.CheckInMS.CheckInResponse checkInResponse, IEnumerable<PassengerAllowance> passengerAllowance) source)
        {
            var passenger = source.checkInResponse?.Passengers?.FirstOrDefault();

            var emailAddress = !string.IsNullOrWhiteSpace(source.request?.EmailAddress)
                                ? source.request.EmailAddress
                                : passenger?.Email;

            return new CreateBasketRequest()
            {
                BasketId = Guid.NewGuid(),
                CheckedBaggageDetails = new CheckedBaggage
                {
                    ProductType = Common.Constants.Application.BaggageProductType,
                    PassengerBaggage = source.request.SelectedBaggageOffer
                        .Select(selectedOffer =>
                        {
                            var passengerAllowance = source.passengerAllowance?.FirstOrDefault(
                                pba => pba.PassengerNiN == selectedOffer.PassengerNiN);
                            return (PsgBaggageInfo)(selectedOffer, passengerAllowance, source.checkInResponse);
                        })
                },
                BookingDetails = source.checkInResponse == null ? null : (PnrDetails)source.checkInResponse,
                ContactDetails = new ContactRecord
                {
                    EmailAddress = emailAddress,
                    PhoneNumber = passenger?.Phone?.Number,
                    CountryCode = passenger?.Phone?.CountryCode
                }
            };
        }
    }
}
