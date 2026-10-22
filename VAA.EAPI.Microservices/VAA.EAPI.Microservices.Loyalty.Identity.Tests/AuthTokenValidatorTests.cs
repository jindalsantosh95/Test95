using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Validation;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    public class AuthTokenValidatorTests
    {
        private readonly Mock<IVerifyMemberStatusRepository> _mockVerifyMemberStatusRepository;
        private readonly Mock<ILogger<AuthTokenValidator>> _logger;
        private readonly AuthTokenValidator _validator;

        public AuthTokenValidatorTests()
        {
            _mockVerifyMemberStatusRepository = new Mock<IVerifyMemberStatusRepository>();
            _logger = new Mock<ILogger<AuthTokenValidator>>();
            _validator = new AuthTokenValidator(_logger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenloggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ValidateAuthTokenCommandHandler(_validator, null, _mockVerifyMemberStatusRepository.Object));
        }

        [Fact]
        public async Task Validator_ShouldReturnNull_ForFailedValidation()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<string>("IdentityMS//LoginToken");

            //Act

            var result = await _validator.Verify(request);

            //Assert
            //Assert.n

            Assert.Empty(result);
        }

        [Fact]
        public async Task Validator_ShouldThrowNullException_ForNullRequest()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _validator.Verify(null));
        }
    }
}
