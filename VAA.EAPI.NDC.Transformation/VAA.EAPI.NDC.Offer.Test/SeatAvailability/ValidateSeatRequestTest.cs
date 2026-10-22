using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRQ;
using Xunit;
using VAA.EAPI.NDC.Offer.Validations.SeatAvailability;

namespace VAA.EAPI.NDC.Offer.Tests.SeatAvailability
{
    public  class ValidateSeatRequestTest
    {
        [Fact]
        public void Should_not_throw_error_when_seatAvailability_request_is_not_null()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            var responseMessage = ValidateSeatAvailability.ValidateSeatAvailabilityRequest(request);

            Assert.NotNull(responseMessage);
        }

        [Fact]
        public void Should_throw_error_when_seatAvailability_request_is_null()
        {
            IATA_SeatAvailabilityRQ request = null;
            var responseMessage = ValidateSeatAvailability.ValidateSeatAvailabilityRequest(request);

            Assert.True(responseMessage.First().ErrorCode.Any());
            Assert.Equal("911_6", responseMessage?.First().ErrorCode);
        }
    }
}
