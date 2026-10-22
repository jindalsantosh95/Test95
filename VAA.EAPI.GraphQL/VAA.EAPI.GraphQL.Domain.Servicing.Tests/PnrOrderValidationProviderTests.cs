using Microsoft.Extensions.DependencyInjection;
using System.Net;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Http;
using VAA.EAPI.GraphQL.Domain.Servicing.Interfaces;
using VAA.EAPI.GraphQL.Domain.Servicing.Models.PnrOrderValidation;
using VAA.EAPI.GraphQL.Domain.Servicing.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using GraphQLEnvironment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Tests
{
    public class PnrOrderValidationProviderTests
    {
        private readonly IServiceProvider _serviceProvider;

        public PnrOrderValidationProviderTests()
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
                .BuildServiceProvider();
        }

        [Fact]
        public async Task ValidateAsync_ShouldReturnAllTrue_WhenPnrAndNamesMatch()
        {
            // Arrange
            var provider = _serviceProvider.GetRequiredService<IPnrOrderValidationProvider>();
            var request = new PnrOrderValidationRequest
            {
                Pnr = "ABC123",
                GivenName = "jane",
                LastName = "doe"
            };

            // Act
            var result = await provider.ValidateAsync(request);

            // Assert
            Assert.True(result.Pnr);
            Assert.False(result.IsGroup);
            Assert.True(result.FirstName);
            Assert.True(result.Surname);
        }

        [Fact]
        public async Task ValidateAsync_ShouldReturnGroupAndSkippedNameValidation_WhenBookingIsGroup()
        {
            var provider = _serviceProvider.GetRequiredService<IPnrOrderValidationProvider>();
            var request = new PnrOrderValidationRequest
            {
                Pnr = "GROUP1",
                GivenName = "Jane",
                LastName = "Doe"
            };

            var result = await provider.ValidateAsync(request);

            Assert.True(result.Pnr);
            Assert.True(result.IsGroup);
            Assert.False(result.FirstName);
            Assert.False(result.Surname);
        }

        [Fact]
        public async Task ValidateAsync_ShouldReturnAllTrue_WhenPnrAndNamesContainLeadingOrTrailingWhitespace()
        {
            // Arrange
            var provider = _serviceProvider.GetRequiredService<IPnrOrderValidationProvider>();
            var request = new PnrOrderValidationRequest
            {
                Pnr = " ABC123 ",
                GivenName = " jane ",
                LastName = " doe "
            };

            // Act
            var result = await provider.ValidateAsync(request);

            // Assert
            Assert.True(result.Pnr);
            Assert.True(result.FirstName);
            Assert.True(result.Surname);
        }

        [Fact]
        public async Task ValidateAsync_ShouldReturnPnrAndFirstNameTrue_WhenSurnameDoesNotMatch()
        {
            // Arrange
            var provider = _serviceProvider.GetRequiredService<IPnrOrderValidationProvider>();
            var request = new PnrOrderValidationRequest
            {
                Pnr = "ABC123",
                GivenName = "Jane",
                LastName = "Smith"
            };

            // Act
            var result = await provider.ValidateAsync(request);

            // Assert
            Assert.True(result.Pnr);
            Assert.True(result.FirstName);
            Assert.False(result.Surname);
        }

        [Fact]
        public async Task ValidateAsync_ShouldReturnAllFalse_WhenOrderDocumentIsNotFound()
        {
            // Arrange
            var provider = _serviceProvider.GetRequiredService<IPnrOrderValidationProvider>();
            var request = new PnrOrderValidationRequest
            {
                Pnr = "MISSING",
                GivenName = "Jane",
                LastName = "Doe"
            };

            // Act
            var result = await provider.ValidateAsync(request);

            // Assert
            Assert.False(result.Pnr);
            Assert.False(result.FirstName);
            Assert.False(result.Surname);
        }

        [Fact]
        public async Task ValidateAsync_ShouldReturnAllFalse_WhenOrderDocumentBookingReferenceDoesNotMatchRequestedPnr()
        {
            // Arrange
            var provider = _serviceProvider.GetRequiredService<IPnrOrderValidationProvider>();
            var request = new PnrOrderValidationRequest
            {
                Pnr = "PNRMISMATCH",
                GivenName = "Jane",
                LastName = "Doe"
            };

            // Act
            var result = await provider.ValidateAsync(request);

            // Assert
            Assert.False(result.Pnr);
            Assert.False(result.FirstName);
            Assert.False(result.Surname);
        }

        [Fact]
        public async Task ValidateAsync_ShouldPropagateUnprocessableEntity_WhenOrderDataRejectsRequest()
        {
            // Arrange
            var provider = _serviceProvider.GetRequiredService<IPnrOrderValidationProvider>();
            var request = new PnrOrderValidationRequest
            {
                Pnr = "UNPROCESSABLE",
                GivenName = "Jane",
                LastName = "Doe"
            };

            // Act
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => provider.ValidateAsync(request));

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, exception.StatusCode);
        }

        [Fact]
        public async Task ValidateAsync_ShouldThrowArgumentException_WhenPnrIsMissing()
        {
            // Arrange
            var provider = _serviceProvider.GetRequiredService<IPnrOrderValidationProvider>();
            var request = new PnrOrderValidationRequest
            {
                Pnr = " ",
                GivenName = "Jane",
                LastName = "Doe"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => provider.ValidateAsync(request));
        }

        [Fact]
        public async Task ValidateAsync_ShouldThrowGone_WhenNoMockMatches()
        {
            // Arrange
            var provider = _serviceProvider.GetRequiredService<IPnrOrderValidationProvider>();
            var request = new PnrOrderValidationRequest
            {
                Pnr = "UNMATCHED",
                GivenName = "Jane",
                LastName = "Doe"
            };

            // Act
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => provider.ValidateAsync(request));

            // Assert
            Assert.Equal(HttpStatusCode.Gone, exception.StatusCode);
        }
    }
}
