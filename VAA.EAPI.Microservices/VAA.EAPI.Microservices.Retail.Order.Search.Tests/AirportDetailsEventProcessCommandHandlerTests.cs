using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Retail.Order.Search.Application.Command.AirportDetails;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Tests
{
    public class AirportDetailsEventProcessCommandHandlerTests
    {
        private readonly Mock<TableStorageService<AirportLocation>> _tableStorageService;
        private readonly Mock<ILogger<AirportDetailsEventProcessCommandhandler>> _logger;
        private readonly AirportDetailsEventProcessCommandhandler _handler;

        public AirportDetailsEventProcessCommandHandlerTests()
        {
            OrderSearchTestsCommon.CreateTestEnvironmentSettings();
            _tableStorageService = new Mock<TableStorageService<AirportLocation>>();
            _logger = new Mock<ILogger<AirportDetailsEventProcessCommandhandler>>();
            _handler = new AirportDetailsEventProcessCommandhandler(_logger.Object, _tableStorageService.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AirportDetailsEventProcessCommandhandler(null, _tableStorageService.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTableStorageServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AirportDetailsEventProcessCommandhandler(_logger.Object, null));
        }
        [Fact]
        public async Task AirportDetailsEventProcessCommandHandler_ThrowsNullException_WithNullRequest()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _handler.Handle(null, CancellationToken.None));

        }
        [Fact]
        public async Task AirportDetailsEventProcessCommandHandler_ShouldSucceed_WithGoodRequest()
        {
            var request = TestDataProvider.GetTestData<AirportDetailsEventProcessCommand>("OrderSearchMS\\AirportDetailsEventProcessCommandData");

            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.NotNull(result.Value);
            Assert.True(result.IsSuccess);
        }
        [Fact]
        public async Task AirportDetailsEventProcessCommandHandler_ShouldFail_WithDeleteOpType()
        {
            var request = TestDataProvider.GetTestData<AirportDetailsEventProcessCommand>("OrderSearchMS\\AirportDetailsEventProcessCommandData");
            request.Data.OpType = "DELETE";

            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsFailure);
        }
        [Fact]
        public async Task AirportDetailsEventProcessCommandHandler_ShouldFail_WithMergeOpType()
        {
            var request = TestDataProvider.GetTestData<AirportDetailsEventProcessCommand>("OrderSearchMS\\AirportDetailsEventProcessCommandData");
            request.Data.OpType = "MERGE";

            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsFailure);
        }
    }
}
