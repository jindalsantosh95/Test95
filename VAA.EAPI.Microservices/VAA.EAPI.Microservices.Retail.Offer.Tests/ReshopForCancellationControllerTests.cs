using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.ReshopForCancellation;
using VAA.EAPI.Microservices.Retail.Offer.Domain.OrderCancel;
using VAA.EAPI.Microservices.Retail.Offer.Triggers.Http.Ita;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{
    public class ReshopForCancellationControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<ReshopForCancellationController>> _logger;
        private readonly ReshopForCancellationController _controller;
        private readonly Mock<FunctionContext> _context;


        public ReshopForCancellationControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<ReshopForCancellationController>>();
            _controller = new ReshopForCancellationController(_mediator.Object, _logger.Object);
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void TestController_Failure()
        {
            //Arrange
            var request = new Mock<HttpRequestData>(_context.Object); //Empty request

            //Act
            var result = _controller.OrderReshopCancel(request.Object);

            //Assert
            Assert.False(result.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task TestController_PassedWithOKStatus()
        {
            //Arrange
            string responseData = Json.Serialize(new CancelReshopResponse { ShoppingResponseId = "Test123" });

            var request = TestDataProvider.GetMockHttpRequestWithMockResponse("OfferMS\\OrderReshopCancellationValidRequest", _context, responseData, false);

            _mediator.Setup(m => m.Send(It.IsAny<ReshopForCancellationQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<CancelReshopResponse>() { Status = Status.OK });

            //Act
            var result = await _controller.OrderReshopCancel(request.Object);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);

            var resultReader = new StreamReader(result.Body);
            var response = Json.Deserialize<CancelReshopResponse>(await resultReader.ReadToEndAsync());
            Assert.Equal("Test123", response.ShoppingResponseId);
        }

        [Fact]
        public async Task TestController_ShouldReturnErrorStatus()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("OfferMS\\OrderReshopCancellationValidRequest", _context, false);

            _mediator.Setup(m => m.Send(It.IsAny<ReshopForCancellationQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<CancelReshopResponse>() { Status = Status.Error, Errors = new List<ErrorDetails>() });

            //Act
            var result = await _controller.OrderReshopCancel(request.Object);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(System.Net.HttpStatusCode.InternalServerError, result.StatusCode);
        }
    }
}
