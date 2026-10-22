using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Constants;

namespace VAA.EAPI.Automation.Heimdall.Tests.HealthChecks.DotCom
{
    public class FrontEndBasketApi
    {

        [Fact]
        [Trait("Category", GlobalTraits.HealthCheck)]
        [Trait("Tag", GlobalTraits.DotCom)]
        public void FrontEndBasketApiTest()
        {
            var response = WebRequest.Get(Config.FrontEndBasketApi);

            var responseString = response.ReadResponseAsString();

            Assert.False(responseString.Contains("\"link\""), $"Potential Cached Basket Response Returned");
            Assert.False(responseString.Contains("\"people\""), $"Potential Cached Basket Response Returned");
            Assert.False(responseString.Contains("\"addressDetails\""), $"Potential Cached Basket Response Returned");
            Assert.False(responseString.Contains("\"contactDetails\""), $"Potential Cached Basket Response Returned");
            Assert.False(responseString.Contains("\"termsAndConditions\""), $"Potential Cached Basket Response Returned");
            Assert.False(responseString.Contains("\"membershipDetails\""), $"Potential Cached Basket Response Returned");
        }
    }
}