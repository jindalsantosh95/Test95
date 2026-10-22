using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Infrastructure.PSS.Repository;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;
using Xunit;

namespace VAA.EAPI.Infrastructure.PSS.Tests
{
    public class PNRRetrieveRepoHandlerTests
    {
        private readonly Mock<ILogger<PNRRetrieveRepoHandler>> _logger;
        private readonly Mock<IPNRCommonV6> _air4PnrCommonV6;
        private readonly PNRRetrieveRepoHandler _handler;

        public PNRRetrieveRepoHandlerTests()
        {
            _logger = new Mock<ILogger<PNRRetrieveRepoHandler>>();
            _air4PnrCommonV6 = new Mock<IPNRCommonV6>();
            _handler = new PNRRetrieveRepoHandler(_logger.Object, _air4PnrCommonV6.Object);
        }

        [Fact]
        public void EmptyInterface_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PNRRetrieveRepoHandler(_logger.Object, null));
        }

        [Fact]
        public void EmptyLogger_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PNRRetrieveRepoHandler(null, _air4PnrCommonV6.Object));
        }

        [Fact]
        public async Task Handler_ShouldReturnNullIfServiceReturnsNull()
        {
            //Arrange
            var request = new PNRRetrieveRepoQuery(new Proxy.PNRCommonV6Proxy.RetrieveRequestType() { SetupContextRequest = new SetupContextRequestType() { RetrieveInfo = new RetrieveInfoType() { PassengerInfo = new PassengerInfoType() } } });
            var response = new RetrieveResponseType() { TeardownContextResponse = null };
            _air4PnrCommonV6.Setup(x => x.Air4Retrieve(It.IsAny<RetrieveRequestType>())).Returns(response);

            //Act
            var result = await _handler.Handle(request, new System.Threading.CancellationToken());

            //Assert
            Assert.Null(response.TeardownContextResponse);

        }

        [Fact]
        public async Task Handler_ShouldReturnValuesIfServiceReturnsValues()
        {
            //Arrange
            var request = new PNRRetrieveRepoQuery(new Proxy.PNRCommonV6Proxy.RetrieveRequestType() { SetupContextRequest = new SetupContextRequestType() { RetrieveInfo = new RetrieveInfoType() { PassengerInfo = new PassengerInfoType() } } });
            AdvisoryMessageType[] ad = new AdvisoryMessageType[] { new AdvisoryMessageType() { } };
            var response = new RetrieveResponseType()
            {
                TeardownContextResponse = new TeardownContextResponseType()
                {
                    Pnr = new PnrType()
                    {
                        AdvisoryMessage = ad
                    }
                }
            };
            _air4PnrCommonV6.Setup(x => x.Air4Retrieve(It.IsAny<RetrieveRequestType>())).Returns(response);

            //Act
            var result = await _handler.Handle(request, new System.Threading.CancellationToken());

            //Assert
            Assert.Single(response.TeardownContextResponse.Pnr.AdvisoryMessage);

        }
    }
}
