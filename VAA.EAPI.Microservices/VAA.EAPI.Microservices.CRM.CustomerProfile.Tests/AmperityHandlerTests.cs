using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Application.Query;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Infrastructure.Interface;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Infrastructure.Services;

namespace VAA.EAPI.Microservices.CRM.CustomerProfile.Tests
{
    [Collection("EnvironmentDependentTests")]
    public class AmperityHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<AmperityHandler> _logger;
        private readonly ILogger<HttpConnector> _loggerHttp;
        private readonly ILogger<AmperityService> _amperityLogger;
        private readonly IHttpConnector _httpConnector;
        private readonly IAmperityService _amperityService;
        private readonly AmperityHandler _handler;

        public AmperityHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<AmperityHandler>.Instance;
            _loggerHttp = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _amperityLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<AmperityService>.Instance;
            _httpConnector = new HttpConnector(_loggerHttp, httpClientFactoryFixture.Current);
            _amperityService = new AmperityService(_httpConnector, _amperityLogger);
            _handler = new AmperityHandler(_amperityService);

            ConfigReader.Set("AmerityUrl", "https://virginatlantic.amperity.com/api/indexes/");
            ConfigReader.Set("AmerityKey", "ix-wJhngCA7");
            ConfigReader.Set("ApiVersion", "2025-07-31");
            ConfigReader.Set("AmperityTenant", "testTenant");
            ConfigReader.Set("AmperityToken", "TestToken");
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenServiceIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new AmperityHandler(null));

            Assert.Equal("amerityService", exception.ParamName);
        }

        [Fact]
        public async Task Handler_ShouldReturnValidTokenObject_ForSuccessfulRequest()
        {
            // Act
            var result = await _handler.GetAmperityDetails("549EA5EA0EC77DD266B21C4ED0A624A354853E0AD63B184CD59A232726A985EB");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Status.OK, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal("549EA5EA0EC77DD266B21C4ED0A624A354853E0AD63B184CD59A232726A985EB", result.Value.Id);
            Assert.Equal("ix-wJhngCA7", result.Value.IndexId);
            Assert.Equal("7d09c978-b999-37d7-8477-f85c7bec2062", result.Value.Attributes.AmperityId);            
        }
        [Fact]
        public async Task Handler_ShouldReturnValidTokenObject_ForNullResponse()
        {
            // Act
            var result = await _handler.GetAmperityDetails("549EA5EA0EC77DD266B21C4ED0A624A354853E0AD63B184CD59A232726A985EBED");

            // Assert
            Assert.NotNull(result); 
            Assert.Null(result.Value);
        }
    }
}
