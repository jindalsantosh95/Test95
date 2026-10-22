using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Services.Loyalty.SyncService;
using VAA.EAPI.Microservices.Loyalty.Sync.LoyaltyProfileSyncServiceRef;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Tests
{
    public class LoyaltySyncServiceHandlerTests
    {
        private readonly Mock<ILogger<LoyaltySyncServiceHandler>> _mockLoyaltySyncLogger;
        private readonly Mock<LoyaltyProfileSyncClient> _mockLoyaltySyncClient;

        public LoyaltySyncServiceHandlerTests()
        {
            _mockLoyaltySyncLogger = new Mock<ILogger<LoyaltySyncServiceHandler>>();
            _mockLoyaltySyncClient = new Mock<LoyaltyProfileSyncClient>();
        }

        [Fact]
        public void NullLogger_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new LoyaltySyncServiceHandler(_mockLoyaltySyncClient.Object, null));
        }

        [Fact]
        public void NullClient_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new LoyaltySyncServiceHandler(null, _mockLoyaltySyncLogger.Object));
        }
    }
}
