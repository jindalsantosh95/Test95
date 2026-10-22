using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Framework.Tests;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredTransactions;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Tests
{

    [Collection("EnvironmentDependentTests")]
    public class VRedTransactionApiServiceQueryHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<HttpConnector> _logger;
        private readonly IHttpConnector _httpConnector;
        private Func<HttpRequestMessage, HttpResponseMessage> tokenResponse;
        private Func<HttpRequestMessage, HttpResponseMessage> vredTransactionResponse;

        public VRedTransactionApiServiceQueryHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            VRedCommon.CreateTestEnvironmentSettings();
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
        }

        [Fact]
        public async Task Handle_NullRequest_ShouldThrowArgumentNullException()
        {
            VRedTransactionApiServiceQueryHandler handler = new VRedTransactionApiServiceQueryHandler(_httpConnector);

            async Task act() => await handler.Handle(null, CancellationToken.None);

            await Assert.ThrowsAsync<ArgumentNullException>(act);
        }

        [Fact]
        public async Task Handle_VRedTransactionApiRequestInfraFailure_ShouldReturnErrorResponse()
        {
            ConfigReader.Set(Loyalty.Vred.Constants.Environment.VredTransactionAPIV2, "https://Testresources.virginatlantic.com/dev/earn/error");
            VRedTransactionApiServiceQueryHandler handler = new VRedTransactionApiServiceQueryHandler(_httpConnector);

            VRedTransactionApiServiceQuery serviceQuery = new VRedTransactionApiServiceQuery()
            {
                TransactionPostRequest = new TransactionPostAPI()
            };

            // Act
            VRedTransactionResponse testResult = await handler.Handle(serviceQuery, CancellationToken.None);

            // Assert
            Assert.IsType<VRedTransactionResponse>(testResult);
            Assert.NotNull(testResult.ErrorResponse);
            Assert.Equal("Unable to connect to VRed Transaction endpoint", testResult.ErrorResponse.Description);
            Assert.Null(testResult.SuccessResponse);
        }

        [Fact]
        public async Task Handle_VRedTransactionApiFailure_ShouldReturnErrorResponse()
        {
            // Arrange
            ConfigReader.Set(Loyalty.Vred.Constants.Environment.VredTransactionAPIV2, "https://Testresources.virginatlantic.com/dev/earn/error");

            VRedTransactionApiServiceQueryHandler handler = new VRedTransactionApiServiceQueryHandler(_httpConnector);
           
            VRedTransactionApiServiceQuery serviceQuery = new VRedTransactionApiServiceQuery()
            {
                //No Request Id
                TransactionPostRequest = new TransactionPostAPI()
                {
                    partnerCode = "VAA",
                    partnerTransactionId = "AZ1717036923298",
                    partnerUserIdDomain = "VAA",
                    partnerUserId = "1078583299",
                    type = "CREDIT",
                    dateTime = "2024-05-30",
                    points = 100,
                    activityDateTime = "2024-05-14",
                    description = "Virgin Atlantic Duty Free Shopping",
                    callbackUrl = "http://test.com",
                    details = "",
                    transactionPartnerCode = "DF"
                }
            };

            // Act
            VRedTransactionResponse testResult = await handler.Handle(serviceQuery, CancellationToken.None);

            // Assert
            Assert.IsType<VRedTransactionResponse>(testResult);
            Assert.NotNull(testResult.ErrorResponse);
            Assert.Equal("400",testResult.ErrorResponse.Status);
            Assert.Equal("CLIENT_ERROR", testResult.ErrorResponse.Code);
            Assert.Equal("requestId must not be blank", testResult.ErrorResponse.Message);
            Assert.Null(testResult.SuccessResponse);
        }

        [Fact]
        public async Task Handle_VRedTransactionApiSuccess_ShouldReturnSuccessResponse()
        {
            // Arrange
            ConfigReader.Set(Loyalty.Vred.Constants.Environment.VredTransactionAPIV2, "https://Testresources.virginatlantic.com/dev/earn/success");
            
            VRedTransactionApiServiceQueryHandler handler = new VRedTransactionApiServiceQueryHandler(_httpConnector);
           
            VRedTransactionApiServiceQuery serviceQuery = new VRedTransactionApiServiceQuery()
            {
                TransactionPostRequest = new TransactionPostAPI()
                {
                    requestId = "be6aaeb9-a150-466d-9eae-a78ccd1b3337",
                    partnerCode = "VAA",
                    partnerTransactionId = "AZ1717036923298",
                    partnerUserIdDomain = "VAA",
                    partnerUserId = "1078583299",
                    type = "CREDIT",
                    dateTime = "2024-05-30",
                    points = 100,
                    activityDateTime = "2024-05-14",
                    description = "Virgin Atlantic Duty Free Shopping",
                    callbackUrl = "http://test.com",
                    details = "",
                    transactionPartnerCode = "DF"
                }
            };

            // Act
            VRedTransactionResponse testResult = await handler.Handle(serviceQuery, CancellationToken.None);

            // Assert
            Assert.IsType<VRedTransactionResponse>(testResult);
            Assert.Null(testResult.ErrorResponse);
            Assert.NotNull(testResult.SuccessResponse);
            Assert.Equal("be6aaeb9-a150-466d-9eae-a78ccd1b3337", testResult.SuccessResponse.RequestId);
        }
    }
}