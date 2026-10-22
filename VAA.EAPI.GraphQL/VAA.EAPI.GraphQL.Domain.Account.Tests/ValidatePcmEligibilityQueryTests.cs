using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.Campaign;
using VAA.EAPI.GraphQL.Domain.Account.Queries;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    public class ValidatePcmEligibilityQueryTests
    {
        private readonly Mock<ICampaignProvider> _campaignProviderMock;
        private readonly Mock<ILogger<ValidatePcmEligibilityQuery>> _loggerMock;
        private readonly ValidatePcmEligibilityQuery _query;

        public ValidatePcmEligibilityQueryTests()
        {
            _campaignProviderMock = new Mock<ICampaignProvider>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<ValidatePcmEligibilityQuery>>(MockBehavior.Loose);
            _query = new ValidatePcmEligibilityQuery(_campaignProviderMock.Object, _loggerMock.Object);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ValidateEligibility_ThrowsArgumentNullException_ForInvalidUniqueId(string? uniqueId)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _query.ValidateEligibility(uniqueId));
        }

        [Fact]
        public async Task ValidateEligibility_ReturnsResponse_WhenProviderSucceeds()
        {
            // Arrange
            var uniqueId = "unique-123";
            var expected = new ValidateEligibilityResponse
            {
                Valid = true,
                Claimed = false,
                FcNumber = "12089808",
                ClaimedDateTime = DateTime.UtcNow.AddDays(1).ToString("O")
            };

            _campaignProviderMock
                .Setup(p => p.ValidateCampaign(uniqueId))
                .ReturnsAsync(expected);

            // Act
            var result = await _query.ValidateEligibility(uniqueId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.Valid, result.Valid);
            Assert.Equal(expected.Claimed, result.Claimed);
            Assert.Equal(expected.FcNumber, result.FcNumber);
            Assert.Equal(expected.ClaimedDateTime, result.ClaimedDateTime);

            _campaignProviderMock.Verify(p => p.ValidateCampaign(uniqueId), Times.Once);
        }

        [Fact]
        public async Task ValidateEligibility_PropagatesException_WhenProviderThrowsAndLogsError()
        {
            // Arrange
            var uniqueId = "bad-id";
            var ex = new InvalidOperationException("provider failure");

            _campaignProviderMock
                .Setup(p => p.ValidateCampaign(uniqueId))
                .ThrowsAsync(ex);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _query.ValidateEligibility(uniqueId));

            _campaignProviderMock.Verify(p => p.ValidateCampaign(uniqueId), Times.Once);

            // Verify that an error was logged once (ILogger.LogError is an extension that calls ILogger.Log).
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}
