using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.AirShoppingRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.FareFamily;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Validations;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.AirShopping.Validations
{
    public class ValidateCalendarSearchRQTests
    {
        [Fact]
        public void Validate_ErrorFor_CalendarSearch_DirectFlightRequest()
        {
            var airShoppingRQ = TestDataProvider.GetXmlTestData<IATA_AirShoppingRQ>("NDCOffer\\Airshopping\\DirectCalendarSearchAirshoppingRQ");

            var validationErrors = RequestValidations.ValidateCalendarSearchRQ(airShoppingRQ);

            Assert.True(validationErrors.Count > 0);
            Assert.Equal("915_1", validationErrors[0].ErrorCode);
        }

        [Fact]
        public void Validate_ErrorFor_CalendarSearch_WithToleranceWindowTimeFilterRequest()
        {
            var airShoppingRQ = TestDataProvider.GetXmlTestData<IATA_AirShoppingRQ>("NDCOffer\\Airshopping\\CalendarSearchWithToleranceTimeFilterASReq");

            var validationErrors = RequestValidations.ValidateCalendarSearchRQ(airShoppingRQ);

            Assert.True(validationErrors.Count > 0);
            Assert.Equal("915_1", validationErrors[0].ErrorCode);
        }

        [Fact]
        public void Validate_Success_CalendarSearch_GoodRequest()
        {
            var airShoppingRQ = TestDataProvider.GetXmlTestData<IATA_AirShoppingRQ>("NDCOffer\\Airshopping\\CalendarSearchAirShoppingRQ");

            var validationErrors = RequestValidations.ValidateCalendarSearchRQ(airShoppingRQ);

            Assert.Empty(validationErrors);
        }

        [Fact]
        public void ValidResponse()
        {
            var response = new CalendarSearchResponse() { FlexibleDateCalendar = new() { DepartureDetails = [new() { }] } };
            var responseMessage = ResponseValidations.ValidateCalendarSearch(response);

            Assert.Empty(responseMessage);
        }

        [Fact]
        public void NullResponse_InvalidResponse()
        {
            var responseMessage = ResponseValidations.ValidateCalendarSearch(null);

            Assert.Equal("76_H", responseMessage[0].Code);
        }

        [Fact]
        public void NoDepartureDetails_InvalidResponse()
        {
            var response = new CalendarSearchResponse() { FlexibleDateCalendar = new() { DepartureDetails = [] } };
            var responseMessage = ResponseValidations.ValidateCalendarSearch(response);

            Assert.Equal("76_H", responseMessage[0].Code);
        }

        [Fact]
        public void Error_InvalidResponse()
        {
            var response = new CalendarSearchResponse() { Error = [new()] };
            var responseMessage = ResponseValidations.ValidateCalendarSearch(response);

            Assert.Equal("76_H", responseMessage[0].Code);
        }

        [Fact]
        public void Should_Return_143_When_Invalid_PaxTypes_Present()
        {
            // Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_AirShoppingRQ>("NDCOffer\\Airshopping\\AirShoppingRequest_PrivateFare");

            var paxTypes = request.Request.PaxList;

            var matchedPtcTypes = new List<ItaFareTypeCodeData>
                                {
                                    new ItaFareTypeCodeData
                                    {
                                        FareSubTypeCode = "STUDENT",
                                        ItaPaxType = "STU",
                                        NdcPaxType = "ADT"
                                    }
                                };

            // Act
            var validationErrors = RequestValidations.ValidatePaxType(paxTypes, matchedPtcTypes);

            // Assert
            Assert.Single(validationErrors);
            Assert.Equal("143_2", validationErrors.First().ErrorCode);
        }
    }
}
