using System.Linq;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Reshop;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{
    public class ReshopSearchMapperTests
    {
        public ReshopSearchMapperTests()
        {
            OfferTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void BundledRequestMapperTest()
        {
            //Arrange 
            var request = TestDataProvider.GetTestData<ReshopRequest>("OfferMS\\OrderReshopBundledValidRequest");
            request.OriginDestinationAssociations.ForEach(x => x.OriginDestination.DepartureDate = "2023-01-22");
            //Act
            var itaRequest = ReshopRequestMapper.ReshopSearchReqMapper(request, true);

            //Assert
            Assert.Equal("RNRPYI", itaRequest.inputs.pnrLocator);
            Assert.NotNull(itaRequest.inputs);
            Assert.NotNull(itaRequest.inputs.slice);
            Assert.NotNull(itaRequest.inputs.slice[0].destination);
            Assert.NotNull(itaRequest.inputs.slice[0].origin);
            Assert.NotNull(itaRequest.inputs.slice[0].date);
            Assert.NotNull(itaRequest.inputs.paxType);
            Assert.NotEmpty(itaRequest.inputs.fareFamilyToPrice);
        }

        [Fact]
        public void ValidateCoupons_In_RequestMapperTest()
        {
            //Arrange 
            var request = TestDataProvider.GetTestData<ReshopRequest>("OfferMS\\OrderReshopBundledValidRequest");

            //Act
            var itaRequest = ReshopRequestMapper.ReshopSearchReqMapper(request, true);

            //Assert
            Assert.NotNull(itaRequest.inputs.reissue.ticket);
            Assert.NotNull(itaRequest.inputs.reissue.ticket.First().coupon);
            Assert.Equal("K", itaRequest.inputs.reissue.ticket.First().coupon.First().bookingCode);
            Assert.Equal("2023-04-10T18:15", itaRequest.inputs.reissue.ticket.First().coupon.First().departure);
            Assert.Equal("DEL", itaRequest.inputs.reissue.ticket.First().coupon.First().destinationCode);
            Assert.Equal("LHR", itaRequest.inputs.reissue.ticket.First().coupon.First().originCode);
            Assert.Equal("VS300", itaRequest.inputs.reissue.ticket.First().coupon.First().flight);
            Assert.Equal("9322826489082", itaRequest.inputs.reissue.ticket.First().ticketNumber);
            Assert.Equal("2025-05-06", itaRequest.inputs.reissue.ticket.First().issueDate);
        }
    }
}
