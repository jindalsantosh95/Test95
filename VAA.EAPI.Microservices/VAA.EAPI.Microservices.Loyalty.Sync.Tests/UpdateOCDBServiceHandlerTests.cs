using VAA.EAPI.Microservices.Loyalty.Sync.LoyaltyProfileSyncServiceRef;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Services.ManageAccount;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Tests
{
    public class UpdateOCDBServiceHandlerTests
    {
        private readonly Mock<ILogger<UpdateOCDBSyncServiceCommandHandler>> _mockUpdateOCDBSyncLogger;
        private readonly Mock<LoyaltyProfileSyncClient> _mockLoyaltySyncClient;

        public UpdateOCDBServiceHandlerTests()
        {
            _mockUpdateOCDBSyncLogger = new Mock<ILogger<UpdateOCDBSyncServiceCommandHandler>>();
            _mockLoyaltySyncClient = new Mock<LoyaltyProfileSyncClient>();
        }

        [Fact]
        public void NullLogger_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdateOCDBSyncServiceCommandHandler(_mockLoyaltySyncClient.Object, null));
        }

        [Fact]
        public void NullClient_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdateOCDBSyncServiceCommandHandler(null, _mockUpdateOCDBSyncLogger.Object));
        }


    }
}
