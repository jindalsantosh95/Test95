using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using System.Threading;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.IdentityEntities;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using System.Linq;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    public class OTPValidateCommandHandlerTests
    {
        private readonly Mock<ITableStorageService<GenerateOtpDbEntities>> _tableStorageService;
        private readonly Mock<ILogger<OTPValidateCommandHandler>> _logger;
        private readonly OTPValidateCommandHandler _handler;
        private static readonly JwtSecurityTokenHandler jwtSecurityTokenHandler = new();

        public OTPValidateCommandHandlerTests()
        {
            _tableStorageService = new Mock<ITableStorageService<GenerateOtpDbEntities>>();
            _logger = new Mock<ILogger<OTPValidateCommandHandler>>();
            _handler = new OTPValidateCommandHandler(_tableStorageService.Object, _logger.Object);
            IdentityTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenTableServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OTPValidateCommandHandler(null, _logger.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OTPValidateCommandHandler(_tableStorageService.Object, null));
        }

        [Fact]
        public async Task Handle_ShouldReturnJwtToken_WhenOtpIsValid()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<OTPValidateCommand>("IdentityMS\\OTPValidateCommandRequest");

            var tableResponse = TestDataProvider.GetTestData<GenerateOtpDbEntities>("IdentityMS\\OTPTableResponse"); 
            tableResponse.OTPExpiryTime = DateTime.UtcNow.AddMinutes(10).ToString();

            _tableStorageService.Setup(x => x.RetrieveAsync("OTPGeneration", request.EmailId)).ReturnsAsync(tableResponse);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result.Value);
            Assert.Null(result.Errors);
            Assert.IsType<string>(result.Value);
            Assert.Equal(Status.OK, result.Status);
            Assert.True(jwtSecurityTokenHandler.CanReadToken(result.Value));
            Assert.True(jwtSecurityTokenHandler.ReadJwtToken(result.Value).ValidTo > DateTime.UtcNow);
        }

        [Fact]
        public async Task Handle_ShouldReturnEroor_WhenOtpIsMissing()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<OTPValidateCommand>("IdentityMS\\OTPValidateCommandRequest");
            request.Otp = string.Empty;

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Errors);
            Assert.IsType<List<ErrorDetails>>(result.Errors);
            Assert.Equal("10152", result.Errors.First().Code);
            Assert.Contains("One of the mandatory field is missing", result.Errors.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldReturnEroor_WhenEmailisMissing()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<OTPValidateCommand>("IdentityMS\\OTPValidateCommandRequest");
            request.EmailId = string.Empty;

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Errors);
            Assert.IsType<List<ErrorDetails>>(result.Errors);
            Assert.Equal("G050", result.Errors.First().Code);
            Assert.Contains("One of the mandatory field is missing", result.Errors.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldReturnEroor_WhenOTPCharacterLengthisMorethanLimit()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<OTPValidateCommand>("IdentityMS\\OTPValidateCommandRequest");
            request.Otp = "123";

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Errors);
            Assert.IsType<List<ErrorDetails>>(result.Errors);
            Assert.Equal("10152", result.Errors.First().Code);
            Assert.Contains("One of the mandatory field is missing", result.Errors.First().Message);
        }


        [Fact]
        public async Task Handle_ShouldReturnEroor_WhenOTPhasExpired()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<OTPValidateCommand>("IdentityMS\\OTPValidateCommandRequest");

            var tableResponse = TestDataProvider.GetTestData<GenerateOtpDbEntities>("IdentityMS\\OTPTableResponse");
            tableResponse.OTPExpiryTime = DateTime.UtcNow.AddMinutes(-10).ToString();

            _tableStorageService.Setup(x => x.RetrieveAsync("OTPGeneration", request.EmailId)).ReturnsAsync(tableResponse);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Errors);
            Assert.IsType<List<ErrorDetails>>(result.Errors);
            Assert.Equal("10154", result.Errors.First().Code);
            Assert.Contains("Provided OTP is expired.", result.Errors.First().Message);
        }


        [Fact]
        public async Task Handle_ShouldReturnEroor_WhenOtpMaxValidationCounthasReached()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<OTPValidateCommand>("IdentityMS\\OTPValidateCommandRequest");

            var tableResponse = TestDataProvider.GetTestData<GenerateOtpDbEntities>("IdentityMS\\OTPTableResponse");
            tableResponse.OTPExpiryTime = DateTime.UtcNow.AddMinutes(10).ToString();
            tableResponse.OTPValidationCount = 5;

            _tableStorageService.Setup(x => x.RetrieveAsync("OTPGeneration", request.EmailId)).ReturnsAsync(tableResponse);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Errors);
            Assert.IsType<List<ErrorDetails>>(result.Errors);
            Assert.Equal("10158", result.Errors.First().Code);
            Assert.Contains("Maximum attempt reached for unsuccessful verification", result.Errors.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldReturnEroor_WhenOtpinRequestisWrong()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<OTPValidateCommand>("IdentityMS\\OTPValidateCommandRequest");
            request.Otp = "876543";

            var tableResponse = TestDataProvider.GetTestData<GenerateOtpDbEntities>("IdentityMS\\OTPTableResponse");
            tableResponse.OTPExpiryTime = DateTime.UtcNow.AddMinutes(10).ToString();

            _tableStorageService.Setup(x => x.RetrieveAsync("OTPGeneration", request.EmailId)).ReturnsAsync(tableResponse);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Errors);
            Assert.IsType<List<ErrorDetails>>(result.Errors);
            Assert.Equal("10153", result.Errors.First().Code);
            Assert.Contains("Invalid OTP", result.Errors.First().Message);
        }
    }
}
