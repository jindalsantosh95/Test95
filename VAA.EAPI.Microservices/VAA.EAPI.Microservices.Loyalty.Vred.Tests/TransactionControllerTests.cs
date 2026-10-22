using Moq;
using Xunit;
using System;
using MediatR;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using VAA.EAPI.Microservices.Loyalty.Vred.Triggers.Http;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Tests
{
    public class TransactionControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly TransactionController _controller;
        private readonly Mock<FunctionContext> _context;

        public TransactionControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new TransactionController(_mockMediator.Object);
            _context = new Mock<FunctionContext>();
            VRedCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new TransactionController(null));
        }

        [Fact]
        public void CancelTransaction_Success()
        {
            var req = SetupTestDataRequests("VredMS\\Transaction\\CancelTransactionRequest", null);
            
            _mockMediator.Setup(m => m.Send(It.IsAny<CancelTransactionRequestCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<string> { });
            
            var Output = _controller.CancelTransaction(req.Object);
            
            Assert.NotNull(Output.Result);
        }

        [Fact]
        public void CancelTransaction_Failure()
        {
            var req = SetupTestDataRequests("VredMS\\Transaction\\CancelTransactionRequest", null);
            
            _mockMediator.Setup(m => m.Send(It.IsAny<CancelTransactionRequestCommand>(), System.Threading.CancellationToken.None)).ThrowsAsync(new Exception());
            
            var Output = _controller.CancelTransaction(req.Object);
            
            Assert.NotNull(Output.Exception);
            Assert.IsType<AggregateException>(Output.Exception);
        }

        [Fact]
        public async Task CreateTransaction_Success()
        {
            string requestBody = "{\"customerId\":\"D971F4C2-1765-84D1-E053-6941D90A4EF2\",\"TransactionId\":\"e81c1827-6c52-4041-9bbd-97e7271c4386\",\"memberNumber\":\"1078583299\",\"partnerCode\":\"DF\",\"productType\":\"VAADutyFree\",\"productName\":\"VAADutyFree\",\"activityDate\":\"14-May-2024\",\"comments\":\"Teat_123\",\"transactionSource\":\"RetailTherapy\",\"createdBy\":\"1078583299\",\"updatedBy\":\"1078583299\",\"tierPoints\":0,\"points\":100,\"productDescription\":\"VirginAtlanticDutyFreeShopping\",\"partnerName\":\"DutyFree\",\"TransactionChannel\":\"PAUSER\",\"partnerCategory\":\"GroundPartner\",\"productCategory\":\"ManualAdjustment\",\"activityType\":\"Accrual\"}";
            var req = SetupTestDataRequests(null, requestBody);

            _mockMediator.Setup(m => m.Send(It.IsAny<CreateTransactionRequestCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<string> {
                Status = Status.Accepted,
                HeaderParameters = new List<ResponseHeader> { new ResponseHeader { Key = Constants.Application.HeaaderParameterlocation, Value = "https://dummy.com/123"  } },
            });

            HttpResponseData result = await _controller.CreateTransactionAsync(req.Object);

            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);
            Assert.True(result.Headers.Contains(Constants.Application.HeaderParameterlocation));
            IEnumerable<string> values;
            if (result.Headers.TryGetValues("location", out values))
            {
                string location = values.First();
                Assert.Equal("https://dummy.com/123", location);
            }
        }

        private Mock<HttpRequestData> SetupTestDataRequests(string requestBodySampleFile, string requestBody)
        {
            var req = new Mock<HttpRequestData>(_context.Object);

            if (requestBodySampleFile != null)
            {
                string body = TestDataProvider.GetTestData<string>(requestBodySampleFile);
                var byteArray = Encoding.ASCII.GetBytes(body);
                var bodyStream = new MemoryStream(byteArray);
                req.Setup(m => m.Body).Returns(bodyStream);
            }
            if(!requestBody.IsNullOrEmpty())
            {
                var byteArray = Encoding.ASCII.GetBytes(requestBody);
                var bodyStream = new MemoryStream(byteArray);
                req.Setup(m => m.Body).Returns(bodyStream);
            }

            req.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(_context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                return response.Object;
            });

            return req;
        }
    }
}
