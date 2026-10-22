using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.CRM.Application.Query;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.CRM.Tests
{
    public class PaymentUpdateStatusResponseQueryHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<PaymentUpdateStatusResponseQueryHandler>> _logger;
        private readonly PaymentUpdateStatusResponseQueryHandler _handler;
        public PaymentUpdateStatusResponseQueryHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<PaymentUpdateStatusResponseQueryHandler>>();
            _handler = new PaymentUpdateStatusResponseQueryHandler(_mediator.Object, _logger.Object);
            CRMTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
           new PaymentUpdateStatusResponseQueryHandler(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PaymentUpdateStatusResponseQueryHandler(_mediator.Object, null));
        }
        [Fact]
        public async Task Handler_ShouldReturnFail_WithBadResponse()
        {
            var request = TestDataProvider.GetTestData<PaymentUpdateStatusResponseQuery>("PaymentMS\\PaymentUpdateStatusResponseQueryData");

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.NotNull(result.Error);
        }
        [Fact]
        public async Task Handler_ShouldReturnFail_ForUpdatePending()
        {
            var request = TestDataProvider.GetTestData<PaymentUpdateStatusResponseQuery>("PaymentMS\\PaymentUpdateStatusResponseQueryData");
            var response = TestDataProvider.GetTestData<ServiceResponse>("PaymentMS\\ServiceResponseQueryResponse");

            System.Environment.SetEnvironmentVariable(CRM.Constants.Environment.ResponsePollingCount, "1");

            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(response);
            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.NotNull(result.Error);
        }
        [Fact]
        public async Task Handler_ShouldReturnFail_ForProcessing()
        {
            var request = TestDataProvider.GetTestData<PaymentUpdateStatusResponseQuery>("PaymentMS\\PaymentUpdateStatusResponseQueryData");
            var response = TestDataProvider.GetTestData<ServiceResponse>("PaymentMS\\ServiceResponseQueryResponse");
            response.Status = (ServiceResponseProcessStatus)2;

            System.Environment.SetEnvironmentVariable(CRM.Constants.Environment.ResponsePollingCount, "1");

            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(response);
            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.NotNull(result.Error);
        }
        [Fact]
        public async Task Handler_ShouldReturnPaymentResponse_ForCompleted()
        {
            var request = TestDataProvider.GetTestData<PaymentUpdateStatusResponseQuery>("PaymentMS\\PaymentUpdateStatusResponseQueryData");
            var response = TestDataProvider.GetTestData<ServiceResponse>("PaymentMS\\ServiceResponseQueryResponse");
            response.Status = (ServiceResponseProcessStatus)3;

            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(response);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
        }
        [Fact]
        public async Task Handler_ShouldReturnFail_ForError()
        {
            var request = TestDataProvider.GetTestData<PaymentUpdateStatusResponseQuery>("PaymentMS\\PaymentUpdateStatusResponseQueryData");
            var response = TestDataProvider.GetTestData<ServiceResponse>("PaymentMS\\ServiceResponseQueryResponse");
            response.Status = (ServiceResponseProcessStatus)4;

            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(response);
            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.NotNull(result.Error);
        }
    }
}
