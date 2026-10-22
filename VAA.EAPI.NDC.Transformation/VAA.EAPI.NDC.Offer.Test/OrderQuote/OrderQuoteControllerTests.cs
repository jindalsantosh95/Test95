using MediatR;
using Microsoft.Azure.Functions.Worker;
using Moq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.NDC.Offer.Application.Query.OrderQuote;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;
using VAA.EAPI.NDC.Offer.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OrderQuote
{
    public class OrderQuoteControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly OrderQuoteController _controller;
        private readonly Mock<FunctionContext> _context;

        public OrderQuoteControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _controller = new OrderQuoteController(_mediator.Object);
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderQuoteController(null));
        }

        [Fact]
        public async Task OrderQuote_ControllerShouldReturnSuccess()
        {
            //Arrange
            var responseData = TestDataProvider.GetXmlTestData("NDCOffer\\OrderQuote\\OrderReshopRS");
            var request = TestDataProvider.GetMockHttpRequestWithMockResponse("NDCOffer\\OrderQuote\\OrderQuoteRQ", _context, responseData, true);
            _mediator.Setup(x => x.Send(It.IsAny<OrderQuoteQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<IATA_OrderReshopRS>() { Status = EAPI.Domain.Entities.Common.Status.OK });

            //Act
            var result = await _controller.OrderQuote(request.Object);
            var resultReader = new StreamReader(result.Body);
            var response = Xml.Deserialize<IATA_OrderReshopRS>(await resultReader.ReadToEndAsync());

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
            Assert.True(response.Items.Any());

            var orderReshopResponse = response.Items[0] as OrderReshopResponseType;

            Assert.Equal("VS932EVDLV522", orderReshopResponse.Order.OrderID);

            var changeIndexType = orderReshopResponse.ReshopResults.Item as NoChangeIndType;

            Assert.True(changeIndexType.NoChangeInd);

        }

        [Fact]
        public async Task OrderQuote_ControllerShouldReturn422()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOffer\\OrderQuote\\OrderQuoteRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OrderQuoteQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<IATA_OrderReshopRS>() { Status = EAPI.Domain.Entities.Common.Status.BusinessException });

            //Act
            var result = await _controller.OrderQuote(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.UnprocessableEntity, result.StatusCode);
        }

        [Fact]
        public async Task OrderQuote_ControllerShouldReturn401()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOffer\\OrderQuote\\OrderQuoteRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OrderQuoteQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<IATA_OrderReshopRS>() { Status = EAPI.Domain.Entities.Common.Status.Unauthorized });

            //Act
            var result = await _controller.OrderQuote(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task OrderQuote_ControllerShouldReturn400()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOffer\\OrderQuote\\OrderQuoteRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OrderQuoteQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<IATA_OrderReshopRS>() { Status = EAPI.Domain.Entities.Common.Status.BadRequest });

            //Act
            var result = await _controller.OrderQuote(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task OrderQuote_ControllerShouldReturn500()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOffer\\OrderQuote\\OrderQuoteRQ", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OrderQuoteQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<IATA_OrderReshopRS>() { Status = EAPI.Domain.Entities.Common.Status.Error });

            //Act
            var result = await _controller.OrderQuote(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.InternalServerError, result.StatusCode);
        }
    }
}
