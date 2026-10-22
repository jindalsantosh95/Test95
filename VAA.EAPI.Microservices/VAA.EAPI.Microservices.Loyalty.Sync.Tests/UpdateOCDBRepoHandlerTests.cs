using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Repository.ManageAccount;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Tests
{
    public class UpdateOCDBRepoHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<UpdateOCDBSyncRepoCommandHandler>> _mockUpdateOCDBSyncRepoLogger;

        public UpdateOCDBRepoHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockUpdateOCDBSyncRepoLogger = new Mock<ILogger<UpdateOCDBSyncRepoCommandHandler>>();
        }

        [Fact]
        public void NullLogger_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdateOCDBSyncRepoCommandHandler(_mockMediator.Object, null));
        }

        [Fact]
        public void NullMediator_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdateOCDBSyncRepoCommandHandler(null, _mockUpdateOCDBSyncRepoLogger.Object));
        }


    }
}
