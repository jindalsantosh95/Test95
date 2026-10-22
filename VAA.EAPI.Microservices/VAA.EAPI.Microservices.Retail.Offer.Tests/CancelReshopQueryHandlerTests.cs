using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Models;
using VAA.EAPI.Infrastructure.PSS.Repository;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.CancelReshop;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{
    public class CancelReshopQueryHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<CancelReshopQueryHandler>> _logger;
        private readonly CancelReshopQueryHandler _handler;

        public CancelReshopQueryHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CancelReshopQueryHandler>>();
            _handler = new CancelReshopQueryHandler(_mediator.Object, _logger.Object);
            OfferTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CancelReshopQueryHandler(null, _logger.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CancelReshopQueryHandler(_mediator.Object, null));
        }


        [Fact]
        public async Task Handler_ShouldReturnResponse_WithRFCErrorCode()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<CancelReshopQuery>("OfferMS//CancelReshopRequest");
            var response = TestDataProvider.GetTestData<PNRCommonV6RetrieveResponse>("OfferMS//PNRRetrieveMSresponse");
            _mediator.Setup(x => x.Send(It.IsAny<PNRRetrieveHistoryRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(Constants.ErrorCodes.RFCErrorCode, result.Value.Errors.FirstOrDefault().Code);

        }

        [Fact]
        public async Task Handler_ShouldReturnResponse_WithReturnSuccess()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<CancelReshopQuery>("OfferMS//CancelReshopRequest");

            var pnrRetrieveMSResponse = TestDataProvider.GetTestData<PNRCommonV6RetrieveResponse>("OfferMS//PNRRetrieveMSresponse");
            var ticketV3MSResponse = TestDataProvider.GetTestData<TicketingV3RetrieveResponse>("OfferMS/TickeingV3OfferMSResponse");

            pnrRetrieveMSResponse.PnrType.HistoryCreditOrSignatures.FirstOrDefault().CreditText = $"-LON GS VA D027954 {DateTime.UtcNow.AddHours(-5).ToString(Constants.Application.PnrCreationDateTimeFormat).ToUpper()} 3E6330";

            _mediator.Setup(x => x.Send(It.IsAny<PNRRetrieveHistoryRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(pnrRetrieveMSResponse);
            _mediator.Setup(x => x.Send(It.IsAny<TicketingV3RetrieveRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(ticketV3MSResponse);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.True(result.IsSuccess);
        }
    }
}
