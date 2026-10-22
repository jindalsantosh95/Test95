using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.Transaction;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Tests
{
    public class CancelTransactionRequestCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<CancelTransactionRequestCommandHandler>> _logger;
        private readonly CancelTransactionRequestCommandHandler _handler;

        public CancelTransactionRequestCommandHandlerTests()
        {
            VRedCommon.CreateTestEnvironmentSettings();
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CancelTransactionRequestCommandHandler>>();
            _handler = new CancelTransactionRequestCommandHandler(_mediator.Object, _logger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CancelTransactionRequestCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CancelTransactionRequestCommandHandler(null, _logger.Object));
        }

        [Fact]
        public void CommandHandler_Success()
        {
            var request = TestDataProvider.GetTestData<CancelTransactionRequestCommand>("VRedMS\\Transaction\\CancelTransactionRequest");

            _mediator.Setup(m => m.Send(It.IsAny<CancelTransactionRequestCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });

            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
            Assert.Equal(Status.Accepted, result.Result.Value.Status);
        }


        [Fact]
        public void CommandHandler_Failure()
        {
            var request = TestDataProvider.GetTestData<CancelTransactionRequestCommand>("VRedMS\\Transaction\\CancelTransactionRequest");

            _mediator.Setup(m => m.Send(It.IsAny<CancelTransactionRequestCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), System.Threading.CancellationToken.None));
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            
            Assert.NotNull(result.Result.Error);
            Assert.Equal("G009",Json.Deserialize<List<ErrorDetails>>(result.Result.Error).FirstOrDefault().Code);
            Assert.Contains("unexpected error", Json.Deserialize<List<ErrorDetails>>(result.Result.Error).FirstOrDefault().Message);
        }
    }
}
