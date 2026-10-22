using Microsoft.Extensions.Hosting;
using Moq;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Validations.ServiceListValidations;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.ServiceList
{
    public class PopulateNonVSSegmentWarnings_OfferTests
    {
        private static Mock<IHostEnvironment> mockHostEnvironment = new Mock<IHostEnvironment>();

        [Fact]
        public void Validate_ErrorFor_NullSegmentOperatingCarrier()
        {
            mockHostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var OfferDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\ServiceList\\OfferDocument");
            OfferDoc.DataList.FlightSegments.FirstOrDefault().OperatingCarrier = null;

            var ServiceListResponseValidations = new ServiceListResponseValidation(mockHostEnvironment.Object);
            var validationErrors = ServiceListResponseValidations.PopulateNonVSSegmentWarningsOffer(OfferDoc);

            Assert.True(validationErrors.Any());
            Assert.Equal("76H", validationErrors.First().Code);
        }

        [Fact]
        public void Validate_NoErrorFor_VSSegmentOperatingCarrier()
        {
            mockHostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var OfferDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\ServiceList\\OfferDocument");
            OfferDoc.DataList.FlightSegments.FirstOrDefault().OperatingCarrier = Constants.Application.OwnerCode;

            var ServiceListResponseValidations = new ServiceListResponseValidation(mockHostEnvironment.Object);
            var validationErrors = ServiceListResponseValidations.PopulateNonVSSegmentWarningsOffer(OfferDoc);

            Assert.True(validationErrors.Any());
            Assert.Equal("76H", validationErrors.First().Code);
        }

        [Fact]
        public void Validtae_ErrorFr_NonVSSegmentOperatingCarrier()
        {
            mockHostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var OfferDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\ServiceList\\OfferDocument");
            OfferDoc.DataList.FlightSegments.FirstOrDefault().OperatingCarrier = "DL";

            var ServiceListResponseValidations = new ServiceListResponseValidation(mockHostEnvironment.Object);
            var validationErrors = ServiceListResponseValidations.PopulateNonVSSegmentWarningsOffer(OfferDoc);

            Assert.True(validationErrors.Any());
            Assert.Equal("76H", validationErrors.First().Code);
        }
    }
}