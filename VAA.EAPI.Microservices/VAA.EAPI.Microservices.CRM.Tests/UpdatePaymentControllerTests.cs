using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.CRM.Application.Commands;
using VAA.EAPI.Microservices.CRM.Application.Query;
using VAA.EAPI.Microservices.CRM.Domain;
using VAA.EAPI.Microservices.CRM.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.CRM.Tests
{
    public class UpdatePaymentControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<UpdatePaymentController>> _logger;
        private readonly Mock<FunctionContext> _context;
        private readonly Mock<HttpRequestData> _request;

        public UpdatePaymentControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<UpdatePaymentController>>();
            _context = new Mock<FunctionContext>();

            string body = TestDataProvider.GetTestData<string>("PaymentMS\\UpdatePaymentControllerData");
            var byteArray = Encoding.ASCII.GetBytes(body);
            var bodyStream = new MemoryStream(byteArray);
            var channelId = new HttpHeadersCollection();

            _request = new Mock<HttpRequestData>(_context.Object);
            _request.Setup(m => m.Body).Returns(bodyStream);
            _request.Setup(m => m.Headers).Returns(channelId);
            _request.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(_context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });

            CRMTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdatePaymentController(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdatePaymentController(_mediator.Object, null));
        }
        [Fact]
        public void Controller_ShouldFailure_WithBadResponse()
        {
            //Arrange

            var request = new Mock<HttpRequestData>(_context.Object); //Empty request
            UpdatePaymentController controller = new UpdatePaymentController(_mediator.Object, _logger.Object);

            //Act

            var result = controller.UpdatePayment(request.Object, string.Empty);

            //Assert

            Assert.False(result.IsCompletedSuccessfully);

        }
        [Fact(Skip = "We cant run this as we are setting this environment variable in other tests outside this collection, this is causing a race condition and making the test fail.")]
        public async Task Controller_ShouldSuceed_WhenGoodrequest()
        {
            //Arrange
            var response = new Response<string>() { Status = Status.Accepted };
            _mediator.Setup(m => m.Send(It.IsAny<UpdatePaymentReqeustCommand>(), CancellationToken.None)).ReturnsAsync(response);
            System.Environment.SetEnvironmentVariable(CRM.Constants.Environment.CRMWebApiResponsiveFeature, "false");
            //Act
            UpdatePaymentController controller = new(_mediator.Object, _logger.Object);
            var result = await controller.UpdatePayment(_request.Object, string.Empty);

            //Assert
            Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);
            Assert.NotNull(result.Headers);
            Assert.NotNull(result.Body);
        }
        [Fact]
        public async Task Controller_ShouldSuceeed_WithCRMWebApiResponsiveFeature()
        {
            //Arrange
            var response = new Response<string>() { Status = Status.Accepted };
            _mediator.Setup(m => m.Send(It.IsAny<UpdatePaymentReqeustCommand>(), CancellationToken.None)).ReturnsAsync(response);
            System.Environment.SetEnvironmentVariable(CRM.Constants.Environment.CRMWebApiResponsiveFeature, "true");
            //Act
            UpdatePaymentController controller = new(_mediator.Object, _logger.Object);
            var result = await controller.UpdatePayment(_request.Object, string.Empty);

            //Assert
            Assert.NotNull(result.Headers);
            Assert.NotNull(result.Body);
        }
        [Fact]
        public async Task ControllerOrderPaymentStatusResponse_ShouldSuceeed_WithGoodRequest()
        {
            //Arrange
            var response = new Response<PaymentResponse>() { Status = Status.Accepted };
            _mediator.Setup(m => m.Send(It.IsAny<PaymentUpdateStatusResponseQuery>(), CancellationToken.None)).ReturnsAsync(response);
            //Act
            UpdatePaymentController controller = new(_mediator.Object, _logger.Object);
            var result = await controller.OrderPaymentStatusResponse(_request.Object, string.Empty);

            //Assert
            Assert.NotNull(result.Headers);
            Assert.NotNull(result.Body);
        }

    }
}
