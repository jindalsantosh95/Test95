using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Core.Xunit.Http;
using VAA.EAPI.GraphQL.Domain.Servicing.Interfaces;
using VAA.EAPI.GraphQL.Domain.Servicing.Models.PnrOrderValidation;
using VAA.EAPI.GraphQL.Domain.Servicing.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using GraphQLEnvironment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Tests
{
    public class PnrOrderValidationQueryTests
    {
        private readonly IServiceProvider _serviceProvider;

        public PnrOrderValidationQueryTests()
        {
            ConfigReader.Set(GraphQLEnvironment.OrderDataMSUrl, "https://testresources.virginatlantic.com/dev/orderdata");
            ConfigReader.Set(GraphQLEnvironment.OrderDataOcpKey, "test-ocp-key");
            ConfigReader.Set(GraphQLEnvironment.OrderDataAppId, "test-app-id");
            ConfigReader.Set(ConnectorBackedOAuthTokenProvider.TokenProviderUrlSetting, "https://testresources.virginatlantic.com/dev/oauth");

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory, HttpClientFactory>()
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IOAuthTokenProvider, ConnectorBackedOAuthTokenProvider>()
                .AddSingleton<IPnrOrderValidationProvider, PnrOrderValidationProvider>()
                .AddSingleton<PnrOrderValidationQuery>()
                .BuildServiceProvider();
        }

        [Fact]
        public void ServiceProvider_ShouldResolveQuery_WhenProviderIsRegistered()
        {
            // Act
            var query = _serviceProvider.GetRequiredService<PnrOrderValidationQuery>();

            // Assert
            Assert.NotNull(query);
        }

        [Fact]
        public async Task Validate_ShouldReturnProviderResponse()
        {
            // Arrange
            var request = new PnrOrderValidationRequest
            {
                Pnr = "ABC123",
                GivenName = "Jane",
                LastName = "Doe"
            };
            var expected = new PnrOrderValidationResponse
            {
                Pnr = true,
                IsGroup = false,
                FirstName = true,
                Surname = true
            };

            var query = _serviceProvider.GetRequiredService<PnrOrderValidationQuery>();

            // Act
            var result = await query.Validate(request);

            // Assert
            Assert.Equal(expected.Pnr, result.Pnr);
            Assert.Equal(expected.IsGroup, result.IsGroup);
            Assert.Equal(expected.FirstName, result.FirstName);
            Assert.Equal(expected.Surname, result.Surname);
        }

        [Fact]
        public async Task Validate_ShouldIncludeFalseFields_WhenSerializedForRestGateway()
        {
            // Arrange
            var request = new PnrOrderValidationRequest
            {
                Pnr = "MISSING",
                GivenName = "Jane",
                LastName = "Doe"
            };

            var query = _serviceProvider.GetRequiredService<PnrOrderValidationQuery>();

            // Act
            var result = await query.Validate(request);
            var payload = JObject.Parse(Json.Serialize(result));

            // Assert
            Assert.True(payload.ContainsKey("pnr"));
            Assert.True(payload.ContainsKey("isGroup"));
            Assert.True(payload.ContainsKey("firstName"));
            Assert.True(payload.ContainsKey("surname"));
            Assert.False(payload.Value<bool>("pnr"));
            Assert.False(payload.Value<bool>("isGroup"));
            Assert.False(payload.Value<bool>("firstName"));
            Assert.False(payload.Value<bool>("surname"));
        }
    }
}
