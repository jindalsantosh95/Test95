using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.SegmentV4Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.Interfaces;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Tests
{
    public class SegmentV4CancelAndRebookHandlerTests
    {
        private readonly Mock<ILogger<SegmentV4CancelAndRebookHandler>> _logger;
        private readonly Mock<IAir4SegmentV4> _segmentV4RequestOperation;
        private readonly SegmentV4CancelAndRebookHandler _handler;
        public SegmentV4CancelAndRebookHandlerTests()
        {
            _segmentV4RequestOperation = new Mock<IAir4SegmentV4>();
            _logger = new Mock<ILogger<SegmentV4CancelAndRebookHandler>>();
            _handler = new SegmentV4CancelAndRebookHandler(_segmentV4RequestOperation.Object, _logger.Object);
            OrderManagementTestsCommon.CreateTestEnvironmentSettings();
        }
        [Fact]
        public async Task Handler_ShouldReturnSuccessWithNoException()
        {
            //Arrange
            var channelEntityData = TestDataProvider.GetTestData<Channel>("ChannelInfo");
            var request = new SegmentV4CancelAndRebookCommand()
            {
                SessionId = "123",
                NumberInParty = 1,
                CancelSegments = new List<Domain.OrderChange.Segment>(),
                RebookSegments = new List<Domain.OrderChange.Segment>(),
                ChannelInfo = channelEntityData
            };
            SegmentDl[] segment = new SegmentDl[] { new SegmentDl() { ClassOfService = "H" } };
            var response = new CancelAndRebookResponseType()
            {
                ResponseInfo = new ResponseInfoType[0],
                Pnr = new PnrType() { SegmentDl = segment }

            };
            _segmentV4RequestOperation.Setup(s => s.CancelAndRebookSegments(It.IsAny<CancelAndRebookRequestType>())).Returns(response);
            //Act

            var result = await _handler.Handle(request, new System.Threading.CancellationToken());
            //Assert
            Assert.Equal("H", result.FirstOrDefault().ClassOfService);
        }
        [Fact]
        public async Task Handler_ShouldThrowExceptionWithException()
        {
            //Arrange
            _segmentV4RequestOperation.Setup(s => s.CancelAndRebookSegments(It.IsAny<CancelAndRebookRequestType>())).Throws<ArgumentNullException>();
            //Act
            async Task result() => await _handler.Handle(new SegmentV4CancelAndRebookCommand(), new System.Threading.CancellationToken());
            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(result);
        }
    }
}
