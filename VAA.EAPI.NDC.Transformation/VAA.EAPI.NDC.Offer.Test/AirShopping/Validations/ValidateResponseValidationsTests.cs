using Microsoft.Extensions.Hosting;
using Moq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Validations;
using VAA.EAPI.NDC.Offer.Validations.AirShopping;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.AirShopping.Validations
{
    public class ValidateResponseValidationsTests
    {
        private readonly Mock<IHostEnvironment> _hostEnvironment;

        public ValidateResponseValidationsTests()
        {
            _hostEnvironment = new Mock<IHostEnvironment>();
        }

        [Fact]
        public void Should_Execute_As_Expected_when_Valid_OfferResponse_Is_Found_In_MicroServiceResponse()
        {
            var offerDocument = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\Airshopping\\OfferResponse");

            _hostEnvironment.Setup(h => h.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationErrors = AirShoppingResponseValidations.ValidateMicroServiceResponse<ErrorType>(offerDocument, _hostEnvironment.Object);

            Assert.Null(validationErrors);
        }

        [Fact]
        public void Should_Get_Error_When_Null_OfferDoc_Is_Found_In_MicroServiceResponse()
        {
            _hostEnvironment.Setup(h => h.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationErrors = AirShoppingResponseValidations.ValidateMicroServiceResponse<ErrorType>(null, _hostEnvironment.Object);

            Assert.Equal("911", validationErrors.ResponseMessages.Errors.First().Code);
            Assert.Equal("Unknown Error", validationErrors.ResponseMessages.Errors.First().TagText);
        }

        [Fact]
        public void Should_Get_Error_When_Null_Offers_Is_Found_In_OfferResponse_In_MicroServiceResponse()
        {
            var offerDocument = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\Airshopping\\OfferResponse");
            offerDocument.Offers = null;
            _hostEnvironment.Setup(h => h.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationErrors = AirShoppingResponseValidations.ValidateMicroServiceResponse<ErrorType>(offerDocument, _hostEnvironment.Object);

            Assert.Equal("No Offers Available", validationErrors.ResponseMessages.Errors.First().TagText);
            Assert.Equal("496", validationErrors.ResponseMessages.Errors.First().Code);
        }

        [Fact]
        public void Should_Execute_As_Expected_When_Valid_CalendarSearchResponse_Is_found_In_MicroServiceResponse()
        {
            var calendarSearchResponse = TestDataProvider.GetTestData<CalendarSearchResponse>("NDCOffer\\Airshopping\\CalendarSearchResponse");

            _hostEnvironment.Setup(h => h.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationErrors = AirShoppingResponseValidations.ValidateCalendarSearchResponse(calendarSearchResponse, _hostEnvironment.Object);

            Assert.Empty(validationErrors);
        }


        [Fact]
        public void Should_Get_Error_When_Null_CalendarSearchResponse_Is_Found_In_MicroServiceResponse()
        {
            var offerDocument = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\Airshopping\\OfferResponse");

            _hostEnvironment.Setup(h => h.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationErrors = AirShoppingResponseValidations.ValidateCalendarSearchResponse(null, _hostEnvironment.Object);

            Assert.True(validationErrors.First().Code.Equals("76H"));
            Assert.True(validationErrors.First().ShortText.Equals("Unable to return calendar offers"));
            Assert.True(validationErrors.First().TagText.Equals("No Offers Available"));
            Assert.True(validationErrors.First().Status.Equals("NotProcessed"));
        }

        [Fact]
        public void Should_Not_Get_Warning_When_No_DepartureDetails_In_CalendarSearchResponse_Is_Found_In_MicroServiceResponse()
        {
            var offerDocument = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\Airshopping\\OfferResponse");

            _hostEnvironment.Setup(h => h.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var calendarSearchResponse = new CalendarSearchResponse()
            {
                FlexibleDateCalendar = new() { DepartureDetails = new DepartureDetails[] { } }
            };

            var validationErrors = AirShoppingResponseValidations.ValidateCalendarSearchResponse(calendarSearchResponse, _hostEnvironment.Object);

            Assert.True(validationErrors.First().Code.Equals("76H"));
            Assert.True(validationErrors.First().ShortText.Equals("Unable to return calendar offers"));
            Assert.True(validationErrors.First().TagText.Equals("No Offers Available"));
            Assert.True(validationErrors.First().Status.Equals("NotProcessed"));
        }

        [Fact]
        public void Should_Get_Error__When_Invalid_Code_Found_In_CalendarSearchResponse_In_MicroServiceResponse()
        {
            var offerDocument = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\Airshopping\\OfferResponse");
            var calendarSearchResponse = new CalendarSearchResponse()
            {
                FlexibleDateCalendar = new()
                {
                    DepartureDetails = new DepartureDetails[] { }
                }
            };
            calendarSearchResponse.Error = new ErrorDetails[] { new() { Code = "76H" } };
            _hostEnvironment.Setup(h => h.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var validationErrors = AirShoppingResponseValidations.ValidateCalendarSearchResponse(null, _hostEnvironment.Object);

            Assert.True(validationErrors.First().Code.Equals("76H"));
            Assert.True(validationErrors.First().ShortText.Equals("Unable to return calendar offers"));
            Assert.True(validationErrors.First().TagText.Equals("No Offers Available"));
            Assert.True(validationErrors.First().Status.Equals("NotProcessed"));
        }

        [Fact]
        public void Should_Return_ITA_NoOffers_When_OfferResponse_Has_3001_Error()
        {
            var offerResponse = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\AirShopping\\ValidateAirShoppingErrorResponse");

            var validationErrors = ResponseValidations.ValidateOfferMicroRS(offerResponse);

            Assert.NotNull(validationErrors);
            Assert.NotEmpty(validationErrors);
            Assert.Equal("ITA_NoOffers", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Should_Return_911_22_When_OfferResponse_Has_Non3001_Error()
        {
            var offerResponse = new OfferResponse
            {
                Error = new[] { new ErrorDetails { Code = "G009" } }
            };

            var validationErrors = ResponseValidations.ValidateOfferMicroRS(offerResponse);

            Assert.NotNull(validationErrors);
            Assert.NotEmpty(validationErrors);
            Assert.Equal("911_22", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Should_Return_ITA_NoOffers_When_OfferResponse_Has_G042_Error()
        {
            var offerResponse = new OfferResponse
            {
                Error = new[] { new ErrorDetails { Code = "G042" } }
            };

            var validationErrors = ResponseValidations.ValidateOfferMicroRS(offerResponse);

            Assert.NotNull(validationErrors);
            Assert.NotEmpty(validationErrors);
            Assert.Equal("ITA_NoOffers", validationErrors.First().ErrorCode);
        }
    }
}
