using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Application.Commands;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Infrastructure.Repositories;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Tests
{
    public class IssueTicketCommandhandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<IssueTicketCommandhandler>> _logger;
        private readonly IssueTicketCommandhandler _command;

        public IssueTicketCommandhandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<IssueTicketCommandhandler>>();
            _command = new IssueTicketCommandhandler(_mediator.Object, _logger.Object);
            ConfigReader.Set("OrderDataServiceBusConnectionString", "dummyConnString");
            ConfigReader.Set("OrderDataServiceBusTopicName", "test");
        }

        [Fact]
        public void HandlerShouldThrowException_WhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new IssueTicketCommandhandler(null, _logger.Object));
        }

        [Fact]
        public void HandlerShouldThrowException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new IssueTicketCommandhandler(_mediator.Object, null));
        }


        [Fact]
        public async Task OrderDocumentCommandHandler_ShouldReturnAcceptedStatus_WithGoodRequest()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<IssueTicketCommand>("SabreOrderDocument\\IssueTicketCommandRequest");
            _mediator.Setup(x => x.Send(It.IsAny<IssueTicketRepoCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<IssueTicketResponse>()
            {
                Status = Status.OK,
                Value = new()
                {
                    AccountableDocumentRef = [new() {
                        DocumentNumber = "1234567890"
                    }]
                }
            });

            // Act
            var result = await _command.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("1234567890", result.Value.Value.AccountableDocumentRef.First().DocumentNumber);
        }
    }

}

