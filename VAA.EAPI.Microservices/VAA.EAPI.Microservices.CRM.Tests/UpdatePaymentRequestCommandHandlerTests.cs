using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.CRM.Application.Commands;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.CRM.Tests
{
    public class UpdatePaymentRequestCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<UpdatePaymentReqeustCommandHandler>> _logger;
        private readonly UpdatePaymentReqeustCommandHandler _handler;
        public UpdatePaymentRequestCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<UpdatePaymentReqeustCommandHandler>>();
            _handler = new UpdatePaymentReqeustCommandHandler(_mediator.Object, _logger.Object);

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            CRMTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
           new UpdatePaymentReqeustCommandHandler(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdatePaymentReqeustCommandHandler(_mediator.Object, null));
        }
        [Fact]
        public async Task UpdatePaymentReqeustCommandHandler_ShouldReturnSuccessStatusResponse_WithGoodResponse()
        {
            var request = TestDataProvider.GetTestData<UpdatePaymentReqeustCommand>("PaymentMS\\PaymentRequestData");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }
        [Fact]
        public async Task UpdatePaymentReqeustCommandHandler_ShouldReturnErrorList_WithBadResponse()
        {
            var request = TestDataProvider.GetTestData<UpdatePaymentReqeustCommand>("PaymentMS\\PaymentRequestDatawithError");

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.True(result.Value.IgnoreNullValues);
        }
    }
}