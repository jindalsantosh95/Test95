using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Query;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Tests
{
    public class OrderChangeResponseQueryHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OrderChangeResponseQueryHandler>> _logger;
        private readonly OrderChangeResponseQueryHandler _handler;

        public OrderChangeResponseQueryHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OrderChangeResponseQueryHandler>>();
            _handler = new OrderChangeResponseQueryHandler(_mediator.Object, _logger.Object);
            ConfigReader.Set("ResponseDelayForOrderChangeTimeInMS", "3000");
            ConfigReader.Set("ResponseDelayTimeInMS", "100");
            ConfigReader.Set("ResponsePollingCount", "3");
        }
       

        [Fact]
        public void HandlerShouldThrowException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderChangeResponseQueryHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task HandlerShouldReturnOrderDocument_InSuccessScenario()
        {
            //Arrange
            var request = new OrderChangeResponseQuery() { CorrelationId = "ABC123", Type = "OrderCreate" };
            _mediator.Setup(x => x.Send(It.IsAny<ServiceResponseQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.ServiceResponse()
            {
                Status = EAPI.Domain.Entities.Common.ServiceResponseProcessStatus.Completed,
                JsonResponseData = TestDataProvider.GetTestData<string>("NDCOrderMS\\OrderDocument")
            });

            //Act
            var result = await _handler.Handle(request,new CancellationToken());

            //Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("VS932ACEB0524", result.Value.Value.Id);
            Assert.Single(result.Value.Value.DataList.CustomerDetails);
            Assert.Equal("LHR", result.Value.Value.DataList.OriginDestinationAssociations.First().OriginDestination.OriginAirport);
        }

        [Fact]
        public async Task HandlerShouldReturnFailureMessage_InErrorScenario()
        {
            //Arrange
            var request = new OrderChangeResponseQuery() { CorrelationId = "ABC123", Type = "OrderCreate" };
            _mediator.Setup(x => x.Send(It.IsAny<ServiceResponseQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.ServiceResponse()
            {
                Status = EAPI.Domain.Entities.Common.ServiceResponseProcessStatus.Error,
                JsonResponseData = "{\"IsFailure\":true,\"IsSuccess\":false,\"Error\":\"[{\\\"code\\\":\\\"G009\\\",\\\"message\\\":\\\"There was an unexpected error.\\\",\\\"developerMessage\\\":\\\"Miscellaneous technical error. | ErrorID: 31d8b690-7e68-4b93-b2c5-068daa7f54e6 | TimeStamp: 9/2/2024 6:57:22 AM\\\",\\\"errorSource\\\":\\\"Source system\\\"}]\"}"
            });

            //Act
            var result = await _handler.Handle(request, new CancellationToken());

            //Assert
            Assert.True(result.IsFailure);
            Assert.Contains("G009", result.Error);
        }

        [Fact]
        public async Task HandlerShouldReturnFailureMessage_InExceptionScenario()
        {
            //Arrange
            var request = new OrderChangeResponseQuery() { CorrelationId = "ABC123", Type = "OrderCreate" };
            _mediator.Setup(x => x.Send(It.IsAny<ServiceResponseQuery>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());
            //Act
            var result = await _handler.Handle(request, new CancellationToken());

            //Assert
            Assert.True(result.IsFailure);
            Assert.Contains("G009", result.Error);
        }
    }
}
