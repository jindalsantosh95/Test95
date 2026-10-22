using MediatR;
using Moq;
using System;
using System.Threading.Tasks;
using System.Threading;
using VAA.EAPI.Microservices.Retail.Order.Search.Application.Query;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Infrastructure.PSS.Repository;
using VAA.EAPI.Infrastructure.PSS.Models;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Search.Application.Query;
using Microsoft.Extensions.Logging;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Tests
{
    public class PNRRetrieveQueryHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly PNRRetrieveQueryHandler _handler;
        private readonly Mock<ILogger<PNRRetrieveQueryHandler>> _logger;

        public PNRRetrieveQueryHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<PNRRetrieveQueryHandler>>();
            _handler = new PNRRetrieveQueryHandler(_mediator.Object, _logger.Object);
            OrderSearchTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PNRRetrieveQueryHandler(null, _logger.Object));
            Assert.Throws<ArgumentNullException>(() => new PNRRetrieveQueryHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task Handle_ShouldSucceed_GoodPNR()
        {
            PNRRetrieveQuery query = new PNRRetrieveQuery() { PNR = "dummy" };
            var response = TestDataProvider.GetTestData<RetrieveResponseType>("OrderSearchMS\\Air4PnrRetrieveResponse");
            
            _mediator.Setup(x => x.Send(It.IsAny<PNRRetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
            
            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.IsType<PNRCommonV6RetrieveResponse>(result.Value.Value);
            Assert.Equal("A dummy01", result.Value.Value.PnrType.FareCalcs.FirstOrDefault().FreeformText);
            Assert.Equal("FIRSTNAME1", result.Value.Value.PnrType.Passengers.FirstOrDefault().FirstName);
            Assert.Equal("A", result.Value.Value.PnrType.Ptcs.FirstOrDefault().PassengerTypeCode);
            Assert.Equal("dummy", result.Value.Value.PnrType.TicketInformation.FirstOrDefault().TicketNumber);
            Assert.Equal("GBP", result.Value.Value.PnrType.Dtis.FirstOrDefault().TotalFareCurrency);
            Assert.Equal("LHR", result.Value.Value.PnrType.SegmentDetails.FirstOrDefault().DeparturePoint);
            Assert.Equal("JFK", result.Value.Value.PnrType.SegmentDetails.FirstOrDefault().ArrivalPoint);

        }
    }
}
