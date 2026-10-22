using System.Threading.Tasks;
using VAA.EAPI.Microservices.CRM.Infrastructure.Helpers;
using Xunit;

namespace VAA.EAPI.Microservices.CRM.Tests
{
    public class CRMUtilityTests
    {
        public readonly CRMUtility _utility;
        public CRMUtilityTests()
        {
            _utility = new CRMUtility();
            CRMTestsCommon.CreateTestEnvironmentSettings();
        }
        [Fact]
        public async Task GetHeaders_ShouldSucceed_WhenCalled()
        {
            var result = await CRMUtility.GetHeaders();

            Assert.NotNull(result);
        }
        [Fact]
        public void CRMWebAPIGETAPIMURL_ShouldSucceed_WithStringRequest()
        {
            var result = CRMUtility.CRMWebAPIGETAPIMURL("TestEntityName");

            Assert.NotNull(result);
        }
        [Fact]
        public void CRMWebAPIPatchAPIMURL_ShouldSucceed_WithStringRequest()
        {
            var result = CRMUtility.CRMWebAPIPatchAPIMURL("TestEntityName", "TestEntityID");

            Assert.NotNull(result);
        }
    }
}
