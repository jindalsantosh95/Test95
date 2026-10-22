using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.UpgradeVACMember;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class UpgradeVACMemberMutationTests
    {
        private readonly UpgradeVACMemberMutation _mutation;
        private readonly Mock<ILogger<UpgradeVACMemberMutation>> _mockLogger;
        private readonly Mock<ICustomerMutationProvider> _mockCustomerMutationProvider;
        private readonly Mock<ICustomerProvider> _mockCustomerProvider;
        private readonly Mock<IReferenceDataMutationProvider> _mockRefDataMutationProvider;
        private readonly Mock<IEligiblePNRsQueryProvider> _mockEligiblePNRsQueryProvider;
        private readonly Mock<IStampFCNumToPNRMutationsProvider> _mockStampFCNumToPNRMutationProvider;

        public UpgradeVACMemberMutationTests()
        {
            _mockLogger = new Mock<ILogger<UpgradeVACMemberMutation>>();
            _mockCustomerMutationProvider = new Mock<ICustomerMutationProvider>();
            _mockCustomerProvider = new Mock<ICustomerProvider>();
            _mockRefDataMutationProvider = new Mock<IReferenceDataMutationProvider>();
            _mockEligiblePNRsQueryProvider = new Mock<IEligiblePNRsQueryProvider>();
            _mockStampFCNumToPNRMutationProvider = new Mock<IStampFCNumToPNRMutationsProvider>();
            _mutation = new UpgradeVACMemberMutation(_mockCustomerProvider.Object, _mockCustomerMutationProvider.Object, _mockLogger.Object, _mockRefDataMutationProvider.Object, _mockEligiblePNRsQueryProvider.Object, _mockStampFCNumToPNRMutationProvider.Object);

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new UpgradeVACMemberMutation(null, _mockCustomerMutationProvider.Object, _mockLogger.Object, _mockRefDataMutationProvider.Object, _mockEligiblePNRsQueryProvider.Object, _mockStampFCNumToPNRMutationProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new UpgradeVACMemberMutation(_mockCustomerProvider.Object, null, _mockLogger.Object, _mockRefDataMutationProvider.Object, _mockEligiblePNRsQueryProvider.Object, _mockStampFCNumToPNRMutationProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new UpgradeVACMemberMutation(_mockCustomerProvider.Object, _mockCustomerMutationProvider.Object, null, _mockRefDataMutationProvider.Object, _mockEligiblePNRsQueryProvider.Object, _mockStampFCNumToPNRMutationProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new UpgradeVACMemberMutation(_mockCustomerProvider.Object, _mockCustomerMutationProvider.Object, _mockLogger.Object, null, _mockEligiblePNRsQueryProvider.Object, _mockStampFCNumToPNRMutationProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new UpgradeVACMemberMutation(_mockCustomerProvider.Object, _mockCustomerMutationProvider.Object, _mockLogger.Object, _mockRefDataMutationProvider.Object, null, _mockStampFCNumToPNRMutationProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new UpgradeVACMemberMutation(_mockCustomerProvider.Object, _mockCustomerMutationProvider.Object, _mockLogger.Object, _mockRefDataMutationProvider.Object, _mockEligiblePNRsQueryProvider.Object, null));
        }

        [Fact]
        public async Task MutationUpgradeFailWithValidationFailure()
        {
            // Arrange
            var mockRequest = TestDataProvider.GetTestData<UpgradeVACMemberRequest>("Loyalty//VACUpgrade//VACUpgradeRequest");
            mockRequest.InitiatedBy = null;

            //Assert
            await Assert.ThrowsAsync<RequestValidationException>(() => _mutation.UpgradeVACMember(mockRequest));
        }
    }
}
