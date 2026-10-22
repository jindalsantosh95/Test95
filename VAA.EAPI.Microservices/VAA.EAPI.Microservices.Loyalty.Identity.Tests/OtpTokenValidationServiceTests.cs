using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    public class OtpTokenValidationServiceTests
    {
        private readonly IOtpTokenValidationService _tokenValidationService;

        public OtpTokenValidationServiceTests()
        {
            IdentityTestsCommon.CreateTestEnvironmentSettings();
            _tokenValidationService = new OtpTokenValidationService();  
        }

        [Fact]
        public async Task Service_ShouldThrowNullExceptionWhenRequestisNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _tokenValidationService.ValidaateOtpToken(new OtpTokenValidationRequest { }));
        }

        [Fact]
        public async Task Service_ShouldValidateandReturnEmail_WhenTokenIsValid()
        {
            // Arrange
            var request = new OtpTokenValidationRequest
            {
                Token = await ValidationTokenHelper.CreateToken("bp123@test.com")
            };

            // Act
            var result = await _tokenValidationService.ValidaateOtpToken(request);

            //Assert
            Assert.NotNull(result.Value);
            Assert.Null(result.Errors);
            Assert.IsType<string>(result.Value);
            Assert.Equal(Status.OK, result.Status);
            Assert.Equal("bp123@test.com", result.Value);
        }

        [Fact]
        public async Task Service_ShouldThrowError_ForInvalidToken()
        {
            // Arrange
            var request = new OtpTokenValidationRequest
            {
                Token = "TestToken"
            };

            // Act
            var result = await _tokenValidationService.ValidaateOtpToken(request);

            //Assert
            Assert.Null(result.Value);
            Assert.NotNull(result.Errors);
            Assert.IsType<List<ErrorDetails>>(result.Errors);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Equal("10199", result.Errors.First().Code);
            Assert.Contains("Enrolment Token Validation Failed due to Invalid or Expired Token.", result.Errors.First().Message);
        }
    }
}
