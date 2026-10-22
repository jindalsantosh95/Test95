using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    public class RefundPaymentCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<Application.Command.CPD.RefundPaymentCommandHandler>> _logger;
        private readonly Mock<IHttpConnector> _httpConnector;
        private readonly Application.Command.CPD.RefundPaymentCommandHandler _handler;

        public RefundPaymentCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<Application.Command.CPD.RefundPaymentCommandHandler>>();
            _httpConnector = new Mock<IHttpConnector>();
            _handler = new Application.Command.CPD.RefundPaymentCommandHandler(_mediator.Object, _logger.Object, _httpConnector.Object);
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Application.Command.CPD.RefundPaymentCommandHandler(null, _logger.Object, _httpConnector.Object));
        }

        [Fact]
        public void When_LoggerObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Application.Command.CPD.RefundPaymentCommandHandler(_mediator.Object, null, _httpConnector.Object));
        }

        [Fact]
        public void When_httpConnectorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Application.Command.CPD.RefundPaymentCommandHandler(_mediator.Object, _logger.Object, null));
        }

        [Fact]
        public async Task When_RefundHandler_ShouldFail()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<Domain.CPD.RefundPaymentRequest>("PaymentMS\\PaymentRequest");
            var response = new Domain.CPD.PaymentResponse { Errors = new List<ErrorDetails>() { new() { Code = "911" } } };

            //Act
            var result = await _handler.Handle(new Application.Command.CPD.RefundPaymentCommand() { RefundPaymentRequest = request }, new CancellationToken());

            //Assert
            Assert.True(result.IsFailure);
            Assert.NotNull(result.Error);
            Assert.Equal("911", response.Errors.First().Code);
        }

        [Fact]
        public async Task When_RefundPayment_ShouldSucceed()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<Domain.CPD.RefundPaymentRequest>("PaymentMS\\RefundPaymentRequest");
            var document = TestDataProvider.GetTestData<Domain.CPD.PaymentTransactionDocument>("PaymentMS\\RefundPaymentDocument");
            ConfigReader.Set("CPDClientId", "12300");
            ConfigReader.Set("servicebusconnectionstring", "servicebusconnectionstring_test");

            var refundResponse = TestDataProvider.GetXmlTestData<PaymentResponse>("PaymentMS\\RefundPaymentResponse");
            refundResponse.Status[0].Exception = null;
            var paymentResponse = new Response<PaymentResponse>
            {
                Status = EAPI.Domain.Entities.Common.Status.OK,
                Value = refundResponse,
                IgnoreNullValues = false,
            };

            _mediator.Setup(x => x.Send(It.IsAny<Infrastructure.Services.CPD.RefundPaymentCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(paymentResponse);
            _mediator.Setup(x => x.Send(It.IsAny<Infrastructure.Repositories.PaymentDocumentReaderQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(document);

            //Act
            var result = await _handler.Handle(new Application.Command.CPD.RefundPaymentCommand() { RefundPaymentRequest = request }, new CancellationToken());

            //Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("VS932GWYG7P24", result.Value.Value.OrderID);
            Assert.Equal("80262675-Refund-2", result.Value.Value.TransactionId);
            Assert.Equal("string", result.Value.Value.MaskedCardNumber);
            Assert.Equal(47.54, result.Value.Value.Amount);
        }

        [Fact]
        public async Task RefundPayment_ShouldFailed_With_Invalid_TransactionId()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<Domain.CPD.RefundPaymentRequest>("PaymentMS\\RefundPaymentRequest");
            ConfigReader.Set("CPDClientId", "12300");

            var refundResponse = TestDataProvider.GetXmlTestData<PaymentResponse>("PaymentMS\\RefundPaymentResponse");
            var paymentResponse = new Response<PaymentResponse>
            {
                Status = EAPI.Domain.Entities.Common.Status.OK,
                Value = refundResponse,
                IgnoreNullValues = false,
            };

            _mediator.Setup(x => x.Send(It.IsAny<Infrastructure.Services.CPD.RefundPaymentCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(paymentResponse);

            //Act
            var result = await _handler.Handle(new Application.Command.CPD.RefundPaymentCommand() { RefundPaymentRequest = request }, new CancellationToken());

            //Assert
            Assert.Equal("G066", result.Value.Errors[0].Code);
            Assert.Equal("Code: 400; Message: Refund Failed", result.Value.Errors.First().Message);
            Assert.Equal("NDC - CPD - PaymentMS", result.Value.Errors.First().ErrorSource);
        }

        [Fact]
        public async Task RefundPayment_ShouldFailed_With_Higher_Refund_Amount()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<Domain.CPD.RefundPaymentRequest>("PaymentMS\\RefundPaymentRequest");
            request.PaymentDetails.First().RefundAmount.MonetaryAmount = "50";
            ConfigReader.Set("CPDClientId", "12300");

            var refundResponse = TestDataProvider.GetXmlTestData<PaymentResponse>("PaymentMS\\RefundPaymentResponse");
            var paymentResponse = new Response<PaymentResponse>
            {
                Status = EAPI.Domain.Entities.Common.Status.OK,
                Value = refundResponse,
                IgnoreNullValues = false,
            };

            _mediator.Setup(x => x.Send(It.IsAny<Infrastructure.Services.CPD.RefundPaymentCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(paymentResponse);
            _mediator.Setup(x => x.Send(It.IsAny<Infrastructure.Repositories.PaymentDocumentReaderQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Domain.CPD.PaymentTransactionDocument { Id = "8009912-Refund-1" });

            //Act
            var result = await _handler.Handle(new Application.Command.CPD.RefundPaymentCommand() { RefundPaymentRequest = request }, new CancellationToken());

            //Assert
            Assert.Equal("BadRequest", result.Value.Status.ToString());
            Assert.Equal("G066", result.Value.Errors[1].Code);            
            Assert.Equal("Code: 53; Message: Amount is more than captured amount", result.Value.Errors[1].Message);
            Assert.Equal("NDC - CPD - PaymentMS",result.Value.Errors[1].ErrorSource);
        }
    }
}
