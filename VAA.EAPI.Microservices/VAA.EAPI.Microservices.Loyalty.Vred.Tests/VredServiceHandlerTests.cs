using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
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
using VAA.EAPI.Microservices.Loyalty.Vred.Entities;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Tests
{
    [Collection("EnvironmentDependentTests")]
    public class VredServiceHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<VredServiceHandler> _serviceLogger;
        private readonly ILogger<HttpConnector> _logger;
        private readonly IHttpConnector _httpConnector;

        public VredServiceHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            // Load "mock" settings 
            VRedCommon.CreateTestEnvironmentSettings();

            _serviceLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<VredServiceHandler>.Instance;
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
        }

        [Fact]
        public async Task Handle_NullRequest_ShouldThrowArgumentNullException()
        {
            // Arrange
            VredServiceHandler handler = new VredServiceHandler(_httpConnector, _serviceLogger);

            // Act
            async Task act() => await handler.Handle(null, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(act);
        }


        /// <summary>
        /// Test to check infrastructure failure (network) when making token request call
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Handle_TokenRequestInfraFailure_ShouldReturnErrorResponse()
        {
            // Arrange
            VredServiceHandler handler = new VredServiceHandler(_httpConnector, _serviceLogger);
            System.Environment.SetEnvironmentVariable(Loyalty.Vred.Constants.Environment.VRedEnrolmentAPISettingName, "https://Testresources.virginatlantic.com/dev/enrolment/bad");

            // Act
            var testResult = await handler.Handle(new VredServiceQuery(new Microservices.Loyalty.Vred.Entities.MemberEnrolmentVRedRequest(), "test"), CancellationToken.None);
            
            // Assert
            Assert.IsType<VRedEnrolmentResponse>(testResult);
            Assert.NotNull(testResult.ErrorResponse);
            Assert.Empty(testResult.Enrollments);
        }


        /// <summary>
        /// Successfully bbtain a token but request to Enrolment API has infrastructure failure
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Handle_EnrolmentRequestIfraFailure_ShouldReturnErrorResponse()
        {
            // Arrange
            VredServiceHandler handler = new VredServiceHandler(_httpConnector, _serviceLogger);
            System.Environment.SetEnvironmentVariable(Loyalty.Vred.Constants.Environment.VRedEnrolmentAPISettingName, "https://Testresources.virginatlantic.com/dev/enrolment/bad");

            // Act
            var testResult = await handler.Handle(new VredServiceQuery(new Microservices.Loyalty.Vred.Entities.MemberEnrolmentVRedRequest(), "test"), CancellationToken.None);


            // Assert
            Assert.IsType<VRedEnrolmentResponse>(testResult);
            Assert.NotNull(testResult.ErrorResponse);            
        }

        /// <summary>
        /// Successfully obtain a token and make a call to Enrolment API but API returns failure response
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Handle_EnrolmentFailure_ShouldReturnErrorResponse()
        {
            // Arrange

            System.Environment.SetEnvironmentVariable(Loyalty.Vred.Constants.Environment.VRedEnrolmentAPISettingName, "https://Testresources.virginatlantic.com/dev/enrolment/bad");

            VredServiceHandler handler = new VredServiceHandler(_httpConnector, _serviceLogger);

            // Act
            var testResult = await handler.Handle(new VredServiceQuery(new Microservices.Loyalty.Vred.Entities.MemberEnrolmentVRedRequest(), "test"), CancellationToken.None);


            // Assert
            Assert.IsType<VRedEnrolmentResponse>(testResult);
            Assert.NotNull(testResult.ErrorResponse);
            Assert.NotNull(testResult.Enrollments);
            Assert.Empty(testResult.Enrollments);
        }

        /// <summary>
        /// Successfully obtain a token and make a call to Enrolment API and API returns good response
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Handle_EnrolmentSucceeds_ShouldReturnSuccessResponse()
        {
            // Arrange
            System.Environment.SetEnvironmentVariable(Loyalty.Vred.Constants.Environment.VRedEnrolmentAPISettingName, "https://Testresources.virginatlantic.com/dev/enrolment/good");

            VredServiceHandler handler = new VredServiceHandler(_httpConnector, _serviceLogger);

         // Act
         var testResult = await handler.Handle(new VredServiceQuery(new Microservices.Loyalty.Vred.Entities.MemberEnrolmentVRedRequest(), "test"), CancellationToken.None);

            // Assert
            Assert.IsType<VRedEnrolmentResponse>(testResult);
            Assert.Null(testResult.ErrorResponse);
            Assert.NotNull(testResult.Enrollments);
            Assert.Single(testResult.Enrollments);
        }
    }
}
