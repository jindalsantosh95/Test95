using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.RetroCredit;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Constants;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.RetroCredit;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Tests
{
    public class PartnerRetroCreditControllerTest
    {
        private readonly Mock<ILogger<PartnerRetroCreditController>> _mockLogger;
        private readonly Mock<IMediator> _mockMediator;
        private readonly PartnerRetroCreditController _controller;
        private readonly Mock<FunctionContext> _context;

        public PartnerRetroCreditControllerTest()
        {
            PDMTestHelpers.CreateTestEnvironmentSettings();
            _mockLogger = new Mock<ILogger<PartnerRetroCreditController>>();
            _mockMediator = new Mock<IMediator>();
            _context = new Mock<FunctionContext>();
            _controller = new PartnerRetroCreditController(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PartnerRetroCreditController(_mockMediator.Object, null));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PartnerRetroCreditController(null, _mockLogger.Object));
        }       

        [Fact]
        public async Task OnlineRetroCreditFeedback()
        {
            var req = SetupTestDataRequests("PartnerDataManagmentMS\\OnlineRetroCreditFeedbackRequest");
            req.Setup(r => r.Headers).Returns(new HttpHeadersCollection(new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Content-Type","application/json") }));
            _mockMediator.Setup(m => m.Send(It.IsAny<RetroFeedbackCommand>(), CancellationToken.None)).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<RetroFeedbackResponse> { Status = EAPI.Domain.Entities.Common.Status.OK, Value = new RetroFeedbackResponse { EligibilityDenialCode="999", Status = "Y", TrackID ="1212112", UniqueIdentificationNumber=Guid.NewGuid().ToString() } });
            
            var output = await _controller.OnlineRetroCreditFeedback(req.Object);

            var feedbackResult = await PDMTestHelpers.ReadResponseData<RetroFeedbackResponse>(output);

            Assert.Equal(System.Net.HttpStatusCode.OK, output.StatusCode);
            Assert.NotNull(feedbackResult);
            Assert.Equal(ErrorCodes.EDC999, feedbackResult.EligibilityDenialCode);
        }

        [Fact]
        public async Task OnlineRetroCredit()
        {
            var req = SetupTestDataRequests("PartnerDataManagmentMS\\OnlineRetroCreditRequest");
            req.Setup(r => r.Headers).Returns(new HttpHeadersCollection(new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Content-Type", "application/json") }));
            _mockMediator.Setup(m => m.Send(It.IsAny<RetroCreditCommand>(), CancellationToken.None)).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<RetroCreditResponse> { Value = new RetroCreditResponse { EligibilityDenialCode = "000", UniqueIdentificationNumber = "VS000008001230516", EticketNumber = "A12121212" } });

            var output = await _controller.OnlineRetroCredit(req.Object);

            var retroResult = await PDMTestHelpers.ReadResponseData<RetroCreditResponse>(output);

            Assert.Equal(System.Net.HttpStatusCode.OK, output.StatusCode);
            Assert.NotNull(retroResult);
            Assert.Equal(ErrorCodes.EDC000, retroResult.EligibilityDenialCode);
        }


        private Mock<HttpRequestData> SetupTestDataRequests(string requestBodySampleFile)
        {
            var req = new Mock<HttpRequestData>(_context.Object);
            if (requestBodySampleFile != null)
            {
                string body = TestDataProvider.GetTestData<string>(requestBodySampleFile);
                var byteArray = Encoding.ASCII.GetBytes(body);
                var bodyStream = new MemoryStream(byteArray);
                req.Setup(m => m.Body).Returns(bodyStream);
            }

            req.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(_context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode, System.Net.HttpStatusCode.OK);
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });

            return req;
        }
    }
}
