using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRQ;
using VAA.EAPI.NDC.Offer.Validations;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.SeatAvailability
{
    public class NullSeatAvailabilityErrorRsTest
    {
        [Fact]
        public void Should_not_throw_error_when_seatAvailability_request_is_not_null()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            var responseMessage = RequestValidations.NullSeatAvailabilityErrorResponseMessage(request);

            Assert.NotNull(responseMessage);
            Assert.Null(responseMessage.Errors);
        }

        [Fact]
        public void Should_throw_error_when_seatAvailability_request_is_null()
        {
            var responseMessage = RequestValidations.NullSeatAvailabilityErrorResponseMessage(null);

            Assert.True(responseMessage.Errors.Any());
            Assert.Equal("912", responseMessage.Errors.First().Code);
        }


    }
}
