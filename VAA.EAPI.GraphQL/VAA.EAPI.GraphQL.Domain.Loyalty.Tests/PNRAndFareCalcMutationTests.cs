using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PNR;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PointCalculation;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class PNRAndFareCalcMutationTests
    {
        private readonly Mock<IPNRQueryProvider> _mockPnrQueryProvider;
        private readonly PNRAndFareCalcMutation _mutation;
        private readonly Mock<ILoyaltyMemberMgtProvider> _loyaltyMemberMgtProvider;

        public PNRAndFareCalcMutationTests()
        {
            _mockPnrQueryProvider = new Mock<IPNRQueryProvider>();
            _loyaltyMemberMgtProvider = new Mock<ILoyaltyMemberMgtProvider>();
            _mutation = new PNRAndFareCalcMutation(_mockPnrQueryProvider.Object, _loyaltyMemberMgtProvider.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullPnrQueryException()
        {
            Assert.Throws<ArgumentNullException>(() => new PNRAndFareCalcMutation(null, _loyaltyMemberMgtProvider.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullMemberMgtException()
        {
            Assert.Throws<ArgumentNullException>(() => new PNRAndFareCalcMutation(_mockPnrQueryProvider.Object, null));
        }

        [Fact]
        public async Task PNRAndFareCalculation_success()
        {
            var mockRequest = new PNRAndFareMutationRequest { Pnr = "xxxxxx" };
            var mockResponse = TestDataProvider.GetTestData<PNRAndFareMutationResponse>("Loyalty//PNRAndFareCalc//PNRAndFareMutationResponse");
            var mockPNRResponse = TestDataProvider.GetTestData<APIResponse<RetrievePNRResponse>>("Loyalty//PNRAndFareCalc//RetrievePNRResponse");
            var mockPointResponse = TestDataProvider.GetTestData<RewardBookingPointCalculationResponse>("Loyalty//PNRAndFareCalc//RewardBookingPointCalcResponse");

            _mockPnrQueryProvider.Setup(x => x.RetrievePNR(It.IsAny<string>())).ReturnsAsync(mockPNRResponse.Response);
            _loyaltyMemberMgtProvider.Setup(x => x.RetrieveRewardBookingPoints(It.IsAny<RewardBookingPointCalculationRequest>())).ReturnsAsync(mockPointResponse);

            var result = await _mutation.PNRAndFareCalculation(mockRequest);

            Assert.Equal(mockResponse.TotalPoints, result.TotalPoints);
            Assert.Equal(mockResponse.NoOfSegments, result.NoOfSegments);
            Assert.Equal(mockResponse.NoOfPassengers, result.NoOfPassengers);
            Assert.Equal(mockResponse.PaxFares.Count(), result.PaxFares.Count());
            Assert.Equal(mockResponse.Segments.Count(), result.Segments.Count());
        }
    }
}
