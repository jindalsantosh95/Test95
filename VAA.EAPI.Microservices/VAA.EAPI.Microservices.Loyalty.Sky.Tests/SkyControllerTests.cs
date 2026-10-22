using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System.Text;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Sky.Application.Command;
using VAA.EAPI.Microservices.Loyalty.Sky.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Sky.Triggers.HTTP;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Sky.Tests
{
    public class SkyControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly SkyController _controller;
        private readonly Mock<FunctionContext> _context;

        public SkyControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _context = new Mock<FunctionContext>();
            _controller = new SkyController(_mediator.Object);
        }

        [Fact]
        public void EmptyMediator_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new SkyController(null));
        }

        [Fact]
        public async Task ControllerShouldReturnSuccessResponse_ForEligibileRequest()
        {
            //Arrange
            var request = SetupTestDataRequests("SkyMS\\CreateSkyTeamRetroRequest");
            request.Setup(r => r.Headers).Returns(new HttpHeadersCollection(new List<KeyValuePair<string, string>> { new("Content-Type", "application/json") }));

            var commandResponse = TestDataProvider.GetTestData<CheckEligibilityResponse>("SkyMS//EligibilityCheckResponse");
            var response = new Response<CheckEligibilityResponse>()
            {
                Status = Status.OK,
                Errors = null,
                Value = commandResponse
            };

            _mediator.Setup(m => m.Send(It.IsAny<CreateSkyTeamRetroCommand>(), CancellationToken.None)).ReturnsAsync(response);

            var output = await _controller.CheckClaimEligibility(request.Object);

            var result = await ReadResponseData<CheckEligibilityResponse>(output);

            Assert.Equal(System.Net.HttpStatusCode.OK, output.StatusCode);
            Assert.True(result.IsEligible);
            Assert.IsType<CheckEligibilityResponse>(result);
            Assert.IsType<SkyClaimResponse>(result.SkyClaimResponse);
            Assert.Equal("000", result.EligibilityDenialCode);
            Assert.Equal("ST67643563783743", result.SkyClaimResponse.UniqueIdentificationNumber);
            Assert.NotNull(result.TrackID);
        }

        [Fact]
        public async Task ControllerShouldReturnError_ForFailedEligibilityRequest()
        {
            //Arrange
            var request = SetupTestDataRequests("SkyMS\\CreateSkyTeamRetroRequest");
            request.Setup(r => r.Headers).Returns(new HttpHeadersCollection(new List<KeyValuePair<string, string>> { new("Content-Type", "application/json") }));

            var commandResponse = TestDataProvider.GetTestData<List<ErrorDetails>>("SkyMS//IneligibleErrorResponse");
            var response = new Response<CheckEligibilityResponse>()
            {
                Status = Status.BusinessException,
                Errors = commandResponse
            };

            _mediator.Setup(m => m.Send(It.IsAny<CreateSkyTeamRetroCommand>(), CancellationToken.None)).ReturnsAsync(response);

            var output = await _controller.CheckClaimEligibility(request.Object);

            var result = await ReadResponseData<List<ErrorDetails>>(output);

            Assert.Equal(System.Net.HttpStatusCode.UnprocessableEntity, output.StatusCode);
            Assert.IsType<List<ErrorDetails>>(result);
            Assert.Equal("10180", result.First().Code);
            Assert.Equal("Customer Ineligible for claim or Incorrect Ticket Information.", result.First().Message);
        }

        [Fact]
        public async Task ControllerShouldReturnSuccessResponse_ForSkyUpdateRequest()
        {
            //Arrange
            var request = SetupTestDataRequests("SkyMS\\SkyRetroFeedbackCommandRequest");
            request.Setup(r => r.Headers).Returns(new HttpHeadersCollection(new List<KeyValuePair<string, string>> { new("Content-Type", "application/json") }));

            var commandResponse = TestDataProvider.GetTestData<SkyOnlineRetroFeedbackResponse>("SkyMS//SkyUpdateResponse");
            var response = new Response<SkyOnlineRetroFeedbackResponse>()
            {
                Status = Status.OK,
                Errors = null,
                Value = commandResponse
            };

            _mediator.Setup(m => m.Send(It.IsAny<SkyRetroFeedbackCommand>(), CancellationToken.None)).ReturnsAsync(response);

            var output = await _controller.OnlineRetroCreditFeedback(request.Object);

            var result = await ReadResponseData<SkyOnlineRetroFeedbackResponse>(output);

            Assert.Equal(System.Net.HttpStatusCode.OK, output.StatusCode);
            Assert.True(result.IsSuccess);
            Assert.IsType<SkyOnlineRetroFeedbackResponse>(result);
            Assert.Equal("999", result.EligibilityDenialCode);
            Assert.Equal("ST4434357905628432344", result.UniqueIdentificationNumber);
            Assert.NotNull(result.TrackID);
        }

        [Fact]
        public async Task ControllerShouldReturnError_ForFailedSkyUpdateRequest()
        {
            //Arrange
            var request = SetupTestDataRequests("SkyMS\\CreateSkyTeamRetroRequest");
            request.Setup(r => r.Headers).Returns(new HttpHeadersCollection(new List<KeyValuePair<string, string>> { new("Content-Type", "application/json") }));

            var commandResponse = TestDataProvider.GetTestData<List<ErrorDetails>>("SkyMS//IneligibleErrorResponse");
            commandResponse.First().Code = "10188";
            commandResponse.First().Message = "Sky Retro Feedback Failed.";

            var response = new Response<SkyOnlineRetroFeedbackResponse>()
            {
                Status = Status.BusinessException,
                Errors = commandResponse
            };

            _mediator.Setup(m => m.Send(It.IsAny<SkyRetroFeedbackCommand>(), CancellationToken.None)).ReturnsAsync(response);

            var output = await _controller.OnlineRetroCreditFeedback(request.Object);

            var result = await ReadResponseData<List<ErrorDetails>>(output);

            Assert.Equal(System.Net.HttpStatusCode.UnprocessableEntity, output.StatusCode);
            Assert.IsType<List<ErrorDetails>>(result);
            Assert.Equal("10188", result.First().Code);
            Assert.Equal("Sky Retro Feedback Failed.", result.First().Message);
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

        private static async Task<T> ReadResponseData<T>(HttpResponseData responseData)
        {
            responseData.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(responseData.Body);
            var responseBody = await reader.ReadToEndAsync();
            return Json.Deserialize<T>(responseBody);
        }
    }
}
