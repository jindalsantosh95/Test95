using Microsoft.Extensions.Hosting;
using Moq;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Validations.ServiceListValidations;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.ServiceList
{
    public class PopulateNonVSSegmentWarnings_OrderTests
    {
        private static Mock<IHostEnvironment> mockHostEnvironment = new Mock<IHostEnvironment>();

        [Fact]
        public void Validate_ErrorFor_NullSegmentMarketingCarrier()
        {
            mockHostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var OrderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\ServiceList\\OrderDocument");
            OrderDoc.DataList.SegmentDetails.FirstOrDefault().MarketingCarrier = null;

            var ServiceListResponseValidations = new ServiceListResponseValidation(mockHostEnvironment.Object);
            var validationErrors = ServiceListResponseValidations.PopulateNonVSSegmentWarningsOrder(OrderDoc);

            Assert.True(validationErrors.Any());
            Assert.Equal("76H", validationErrors.First().Code);
        }

        [Fact]
        public void Validate_NoErrorFor_VSSegmentMarketingCarrier()
        {
            mockHostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var OrderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\ServiceList\\OrderDocument");
            OrderDoc.DataList.SegmentDetails.FirstOrDefault().MarketingCarrier="VS";

            var ServiceListResponseValidations = new ServiceListResponseValidation(mockHostEnvironment.Object);
            var validationErrors = ServiceListResponseValidations.PopulateNonVSSegmentWarningsOrder(OrderDoc);

            Assert.False(validationErrors.Any());
        }

        [Fact]
        public void Validate_ErrorFor_NonVSSegmentMarketingCarrier()
        {
            mockHostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var OrderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\ServiceList\\OrderDocument");
            OrderDoc.DataList.SegmentDetails.FirstOrDefault().OperatingCarrier ="DL";

            var ServiceListResponseValidations = new ServiceListResponseValidation(mockHostEnvironment.Object);
            var validationErrors = ServiceListResponseValidations.PopulateNonVSSegmentWarningsOrder(OrderDoc);

            Assert.True(validationErrors.Any());
            Assert.Equal("76H", validationErrors.First().Code);
        }
    }
}
