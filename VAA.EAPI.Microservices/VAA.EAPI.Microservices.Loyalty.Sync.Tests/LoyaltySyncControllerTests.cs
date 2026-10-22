using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Sync.Application.Command;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Sync.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Tests
{
    public class LoyaltySyncControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<FunctionContext> _context;
        private readonly LoyaltySyncController _controller;

        public LoyaltySyncControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _context = new Mock<FunctionContext>();
            _controller = new LoyaltySyncController(_mockMediator.Object);
            LoyaltySyncTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new LoyaltySyncController(null));
        }

        [Fact]
        public async Task SyncSFPD_ShouldReturnAcceptedStatus()
        {
            //Arrange            
            var req = SetupChangeRequests("LoyaltySyncMS\\SFPDSyncRequest");

            var mockResponse = Result.Success(new Response<SFPDSyncCommandResponse>()
            {
                Status = Status.Accepted,
                HeaderParameters = new List<ResponseHeader> { new() { Key = Constants.Application.HeaderParameterlocation, Value = "http://localhost:7075/api/SyncSFPD/status/f7b3c8d5-4075-45c1-979a-e799ecfdadef" } },
            });

            _mockMediator.Setup(m => m.Send(It.IsAny<SFPDSyncCommand>(), CancellationToken.None)).ReturnsAsync(mockResponse);

            //Act            
            var result = await _controller.SyncSFPD(req.Object);

            //Assert
            Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);
            Assert.True(result.Headers.Contains(Constants.Application.HeaderParameterlocation));

            result.Headers.TryGetValues(Constants.Application.HeaderParameterlocation, out IEnumerable<string> locationValues);
            Assert.Equal(mockResponse.Value.HeaderParameters.Select(x=>x.Value).FirstOrDefault(), locationValues.FirstOrDefault());
        }

        [Fact]
        public void SyncSFPD_Failure()
        {
            //Arrange   
            var req = SetupChangeRequests("LoyaltySyncMS\\SFPDSyncRequest");

            var mockResponse = Result.Success(new Response<SFPDSyncCommandResponse>()
            {
                Status = Status.BusinessException,
                Errors = new List<ErrorDetails>
                 {
                    new ErrorDetails
                     {
                            Code = "G009",
                            DeveloperMessage = "There was an unexpected error.",
                            ErrorSource = "Miscellaneous technical error. | ErrorID: 65bafe78-02c5-4f5e-835c-31b02bc9a9d8 | TimeStamp: 8/29/2024 6:04:12 AM",
                             Message = "Source system"
                      }
             }
            });

            _mockMediator.Setup(m => m.Send(It.IsAny<SFPDSyncCommand>(), CancellationToken.None)).ReturnsAsync(mockResponse);

            //Act    
            var result = _controller.SyncSFPD(req.Object).Result;

            // Read in the body
            result.Body.Seek(0, SeekOrigin.Begin);
            StreamReader rdr = new StreamReader(result.Body);
            var body = rdr.ReadToEnd();
            result.Body.Close();            

            var errors = Json.Deserialize<List<ErrorDetails>>(body);            

            //Assert            
            Assert.Equal(mockResponse.Value.Errors.FirstOrDefault().Code, errors.FirstOrDefault().Code);
            Assert.Equal(mockResponse.Value.Errors.FirstOrDefault().ErrorSource, errors.FirstOrDefault().ErrorSource);
            Assert.Equal(mockResponse.Value.Errors.Count(), errors.Count);
        }

        private Mock<HttpRequestData> SetupChangeRequests(string requestBodySampleFile)
        {
            var req = new Mock<HttpRequestData>(_context.Object);

            string body = TestDataProvider.GetTestData<string>(requestBodySampleFile);
            var byteArray = Encoding.ASCII.GetBytes(body);
            var bodyStream = new MemoryStream(byteArray);
            req.Setup(m => m.Body).Returns(bodyStream);

            req.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(_context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });

            return req;
        }

    }
}
