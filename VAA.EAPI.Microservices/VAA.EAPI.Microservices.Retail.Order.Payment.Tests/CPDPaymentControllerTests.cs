using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    public class CPDPaymentControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ISplitPnrPaymentCommand> _splitPnrPaymentMock;
        private readonly Mock<FunctionContext> _context;

        public CPDPaymentControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _splitPnrPaymentMock = new Mock<ISplitPnrPaymentCommand>();
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CPDPaymentController(null, _splitPnrPaymentMock.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenSplitPnrHandlerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CPDPaymentController(_mediator.Object, null));
        }

        [Fact]
        public void Controller_ShouldFail_WithBadRequest()
        {
            //Arrange
            var request = new Mock<HttpRequestData>(_context.Object);
            CPDPaymentController controller = new(_mediator.Object, _splitPnrPaymentMock.Object);

            //Act
            var result = controller.Authorize(request.Object);

            //Asserts
            Assert.False(result.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task AuthorizeHandler_ShouldReturnAcceptedWhenNoErrorsOccur()
        {
            //Arrange
            var requestBody = "{\"bookingReferences\":{\"rloc\":\"ABC123\"}}";            
            var byteArray = Encoding.ASCII.GetBytes(requestBody);
            var response = TestDataProvider.GetXmlTestData<Infrastructure.Models.CPD.PaymentResponse>("PaymentMS\\AuthorisationResponse");
            var bodyStream = new MemoryStream(byteArray);
            var request = new Mock<HttpRequestData>(_context.Object);
            
            request.Setup(r => r.Body).Returns(bodyStream);
            request.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(_context.Object);
                response.SetupProperty(r => r.Headers, []);
                response.SetupProperty(r => r.StatusCode);
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });

            //Act
            CPDPaymentController controller = new(_mediator.Object, _splitPnrPaymentMock.Object);
            var result = await controller.Authorize(request.Object);

            //Assert           
            Assert.Equal("2000", response.Status.First().Code.ToString());
            Assert.Equal("2000101", response.Status.First().SubCode.ToString());            
        }

        [Fact]
        public async Task CaptureHandler_ShouldReturnAcceptedWhenNoErrorsOccur()
        {
            //Arrange
            var requestBody = "{\"bookingReferences\":{\"rloc\":\"GWYG7P\"},\"orderId\":\"VS932GWYG7P24\",\"paymentDetails\":[{\"paymentTransactionId\":\"80262675\",\"amount\":{\"monetaryAmount\":\"7500\"}}],\"accountableDocumentRefs\":[{\"documentNumber\":\"987123456789\"},{\"documentNumber\":\"1234556789086\"}]}";
            var byteArray = Encoding.ASCII.GetBytes(requestBody);
            var response = TestDataProvider.GetXmlTestData<Infrastructure.Models.CPD.PaymentResponse>("PaymentMS\\CaptureResponse");
            var bodyStream = new MemoryStream(byteArray);
            var request = new Mock<HttpRequestData>(_context.Object);

            request.Setup(r => r.Body).Returns(bodyStream);
            request.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(_context.Object);
                response.SetupProperty(r => r.Headers, []);
                response.SetupProperty(r => r.StatusCode);
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });

            //Act
            CPDPaymentController controller = new(_mediator.Object, _splitPnrPaymentMock.Object);
            var result = await controller.CapturePayment(request.Object);

            //Assert           
            Assert.Equal("100", response.Status.First().Code.ToString());
            Assert.Equal("Operation Success", response.Status.First().Text);
        }


        [Fact]
        public async Task RefundHandler_ShouldReturnAcceptedWhenNoErrorsOccur()
        {
            //Arrange
            var requestBody = "{\"bookingReferences\":{\"rloc\":\"GWYG7P\"},\"orderId\":\"VS932GWYG7P24\",\"paymentDetails\":[{\"paymentTransactionId\":\"80262675\",\"amount\":{\"monetaryAmount\":\"7500\"}}],\"accountableDocumentRefs\":[{\"documentNumber\":\"987123456789\"},{\"documentNumber\":\"1234556789086\"}]}";
            var byteArray = Encoding.ASCII.GetBytes(requestBody);
            var response = TestDataProvider.GetXmlTestData<Infrastructure.Models.CPD.PaymentResponse>("PaymentMS\\CaptureResponse");
            var bodyStream = new MemoryStream(byteArray);
            var request = new Mock<HttpRequestData>(_context.Object);

            request.Setup(r => r.Body).Returns(bodyStream);
            request.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(_context.Object);
                response.SetupProperty(r => r.Headers, []);
                response.SetupProperty(r => r.StatusCode);
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });

            //Act
            CPDPaymentController controller = new(_mediator.Object, _splitPnrPaymentMock.Object);
            var result = await controller.RefundPayment(request.Object);

            //Assert           
            Assert.Equal("100", response.Status.First().Code.ToString());
            Assert.Equal("Operation Success", response.Status.First().Text);
        }

    }
}
