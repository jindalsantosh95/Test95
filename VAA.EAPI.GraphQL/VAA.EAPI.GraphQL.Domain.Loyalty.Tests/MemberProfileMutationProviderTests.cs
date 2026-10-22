using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MembersProfile;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class MemberProfileMutationProviderTests
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;

        public MemberProfileMutationProviderTests()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task VACProfileUpdate_ShouldSuccess()
        {
            //Arrange
            var mockRequest = TestDataProvider.GetTestData<MemberProfileUpdateRequest>("Loyalty//VACProfileUpdate//MemberProfileUpdateRequest");

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("XYZ");  // Dummy token           
            _mockConnector.Setup(m => m.SendAsync<string>(It.IsAny<string>(),
                                                       It.IsAny<Dictionary<string, string>>(),
                                                       It.IsAny<string>(),
                                                       It.IsAny<HttpMethod>(),
                                                       It.IsAny<string>(),
                                                       It.IsAny<bool>())).Returns(Task.FromResult(new APIResponse<string> { Response = Application.ProfileUpdateMSSuccess }));

            //Act
            var provider = new MemberProfileMutationProvider<string>(_mocktokenProvider.Object, _mockConnector.Object);
            var result = await provider.UpdateMemberProfile(mockRequest);

            //Assert            
            Assert.NotNull(result);
            Assert.Equal(Application.ProfileUpdateMSSuccess, result);
        }
    }
}
