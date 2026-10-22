using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.CoBrand;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.CoBrand;
using Xunit;
namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{

    public class GenerateSynchronyTokenServiceCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IHttpConnector _httpConnectorMock;
        private readonly ISynchronyAccessTokenGenerator _tokenGeneratorMock;
        private readonly ISynchronyPayloadCryptoService _payloadCryptoServiceMock;
        private readonly ILogger<GenerateSynchronyTokenServiceCommandHandler> _logger;
        private readonly GenerateSynchronyTokenServiceCommandHandler _handler;

        public GenerateSynchronyTokenServiceCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("synchronydatashareapiurl", "https://api-stg.syf.com/mock/");
            ConfigReader.Set("SynchronyAuthenticationUrl", "https://api-stg.syf.com/v1/oauth2/token");
            ConfigReader.Set("SynchronyApiClientId", "dummy");
            ConfigReader.Set("SynchronyApiClientSecret", "dummy");
            ConfigReader.Set("SynchronyEncryptionPublicKey", "{\"kty\":\"RSA\",\"n\":\"sXchQMWxRldc2a5WYwIBz7AqQzZqMvmOq6OXU6R-4UmEgfYmZsJm-m_zT18UWTgPEqXL-M9KmLRAEH_3hn5zpuHzdL_R5DhCDk3cEU1g9ZyEoTPgStOTyN-j_S0cbzPR-s28lfET7Dg8K9fB6R_NX4GpEO_l8e3qXYjTL1u-jZFM\",\"e\":\"AQAB\"}");


            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;

            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<GenerateSynchronyTokenServiceCommandHandler>.Instance;
            _httpConnectorMock = new HttpConnector(logger, httpClientFactoryFixture.Current);
            _tokenGeneratorMock = new SynchronyAccessTokenGenerator(_httpConnectorMock);
            _payloadCryptoServiceMock = new SynchronyPayloadCryptoService();

            _handler = new GenerateSynchronyTokenServiceCommandHandler(
                _httpConnectorMock,
                _tokenGeneratorMock,
                _payloadCryptoServiceMock,
                _logger
            );
            // Set a mock correlation ID in AsyncContext
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenTokenIsGenerated()
        {
            // Arrange
            var request = new GenerateSynchronyTokenServiceCommand
            {
                ApplicantDetails = new()
                {
                    FirstName = "Alice",
                    LastName = "Smith",
                    DateOfBirth = "1992-05-01",
                    Contacts = new()
                    {
                        Email = "alice.smith@example.com"
                    },
                    LoyaltyMemberDetails = new()
                    {
                        MemberNumber = "ABC123456"
                    }
                },
                InitialPurchaseAmount = "250.00",
                AccountInfo = "mock"
            };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.NotNull(result.Value.Value);
            Assert.Equal("Rresz8MN", result.Value.Value.SynchronyToken);
        }
    }
}
