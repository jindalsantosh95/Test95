using VAA.EAPI.Microservices.Loyalty.Wallet.Helper;
using VAA.EAPI.Microservices.Loyalty.Wallet.Constants;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Tests
{
    public class AirshipWalletHelperTests
    {
        [Theory]
        [InlineData("VS", "Upper", TemplateCode.VS_UPPER)]
        [InlineData("VS", "Upper Class", TemplateCode.VS_UPPER)]
        [InlineData("vs", "premium", TemplateCode.VS_PREMIUM)]
        [InlineData("VS", "Premium Economy", TemplateCode.VS_PREMIUM)]
        [InlineData("VS", "Economy", TemplateCode.VS_ECONOMY)]
        [InlineData("DL", "Upper", TemplateCode.DL)]
        [InlineData("BA", "Premium", TemplateCode.OA)]
        public void GetTemplateCode_ReturnsTemplateForOperatingCarrierAndCabin(
            string operatingCarrierCode,
            string cabinName,
            TemplateCode expectedTemplateCode)
        {
            var result = AirshipWalletHelper.GetTemplateCode(operatingCarrierCode, cabinName);

            Assert.Equal(expectedTemplateCode, result);
        }

        [Fact]
        public void BuildAdaptivePassUrl_ReplacesExistingQueryAndEncodesValues()
        {
            var result = AirshipWalletHelper.BuildAdaptivePassUrl(
                "  https://wallet-api.asnapieu.com/v1/pass/adaptive/?ids=legacy#fragment  ",
                ["pass 1", "pass/2"],
                "ios device");

            Assert.Equal(
                "https://wallet-api.asnapieu.com/v1/pass/adaptive?ids=pass%201,pass%2F2&deviceType=ios%20device",
                result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("not-a-url")]
        public void BuildAdaptivePassUrl_ReturnsNull_WhenConfiguredUrlIsInvalid(string? configuredUrl)
        {
            var result = AirshipWalletHelper.BuildAdaptivePassUrl(configuredUrl, ["id1"]);

            Assert.Null(result);
        }

        [Fact]
        public void BuildAdaptivePassUrl_ReturnsNull_WhenIdsAreBlank()
        {
            var result = AirshipWalletHelper.BuildAdaptivePassUrl(
                "https://wallet-api.asnapieu.com/v1/pass/adaptive",
                [null, "", " "]);

            Assert.Null(result);
        }
    }
}
