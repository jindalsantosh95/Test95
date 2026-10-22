using VAA.EAPI.Microservices.Insights.Infrastructure.Azure.Models;
using VAA.EAPI.Microservices.Insights.Mapper;

namespace VAA.EAPI.Microservices.Insights.Tests
{
    public class AzureAppSettingsMapperTests
    {
        [Fact]
        public void Map_AddsResolverStatusToMatchingAppSetting()
        {
            var appSettings = new[]
            {
                new AzureAppSetting { Name = "KeyVaultSetting", Value = "@Microsoft.KeyVault(...)" },
                new AzureAppSetting { Name = "PlainSetting", Value = "plain-value" }
            };
            var keyVaultReferences = new[]
            {
                new AzureKeyVaultReference
                {
                    Name = "keyvaultsetting",
                    Properties = new AzureKeyVaultReferenceProperties { Status = "Resolved" }
                }
            };

            var result = AzureAppSettingsMapper.Map(appSettings, keyVaultReferences).ToList();

            Assert.Equal("Resolved", result[0].ResolverStatus);
            Assert.Null(result[1].ResolverStatus);
            Assert.Equal("plain-value", result[1].Value);
        }

        [Fact]
        public void Map_ReturnsAppSettings_WhenKeyVaultReferencesAreEmpty()
        {
            var appSettings = new[]
            {
                new AzureAppSetting { Name = "PlainSetting", Value = "plain-value" }
            };

            var result = AzureAppSettingsMapper.Map(appSettings, []).ToList();

            var appSetting = Assert.Single(result);
            Assert.Equal("PlainSetting", appSetting.Name);
            Assert.Equal("plain-value", appSetting.Value);
            Assert.Null(appSetting.ResolverStatus);
        }
    }
}
