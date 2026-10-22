using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Trips.Helpers;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.GraphQL.Seats
{
    public abstract class SeatTestsBase : IClassFixture<TokenFixture>
    {
        protected readonly string env = Config.Environment;

        protected TestConfig _testConfig = null;
        protected readonly TokenFixture _tokenFixture;

        protected SeatTestsBase(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;
        }

        protected static void ValidatePnrDetails(Dictionary<string, string> pnrDetails)
            => TestDataPrerequisiteHelper.ValidatePnrDetailsOrSkip(pnrDetails);

        protected static void SkipTest(string reason) => TestDataPrerequisiteHelper.Skip(reason);

        protected static string MaskCardNumber(string cardNumber) => cardNumber?.Length >= 4 ? $"**** **** **** {cardNumber[^4..]}" : "****";

        protected void ConfigureSeatPaymentCard(Currency currency)
        {
            _testConfig.Book.Payment = new Payment(CardType.Mastercard);
            _testConfig.Book.Payment.CardDetails.CardNumber = currency == Currency.USD ? "5333308664112277" : "5101081046006034";
        }

        protected static bool IsSeatAlreadyAssigned(dynamic bookingResult)
        {
            try
            {
                dynamic passengers = bookingResult.data.Booking.passengers;
                if (passengers == null || !passengers.HasValues)
                    return false;

                foreach (dynamic pax in passengers)
                {
                    dynamic slices = pax.slices;
                    if (slices == null || !slices.HasValues)
                        continue;
                    foreach (dynamic slice in slices)
                    {
                        dynamic segments = slice.segments;
                        if (segments == null || !segments.HasValues)
                            continue;
                        foreach (dynamic segment in segments)
                        {
                            dynamic legs = segment.legs;
                            if (legs == null || !legs.HasValues)
                                continue;
                            foreach (dynamic leg in legs)
                            {
                                dynamic seatAssignments = leg.seatAssignments;
                                if (seatAssignments != null && seatAssignments.HasValues)
                                    return true;
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw new System.InvalidOperationException("Failed to determine whether a seat is already assigned.", ex);
            }
            return false;
        }
    }
}
