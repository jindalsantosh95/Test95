using MediatR;
using Moq;
using System;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;
using Xunit;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services.Transaction;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredTransactions;
using System.Collections.Generic;
using VAA.EAPI.Common.Serialisation;
using System.Net.Http;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Tests
{
    public class CancelVredTransactionServiceQueryHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly UpdateVredTransactionServiceHandler _handler;
        private readonly Mock<IHttpConnector> _httpConnector;
        private readonly Mock<ITokenHelper> _tokenHelper;

        public CancelVredTransactionServiceQueryHandlerTests()
        {
            VRedCommon.CreateTestEnvironmentSettings();
            _mediator = new Mock<IMediator>();
            _httpConnector = new Mock<IHttpConnector>();
            _tokenHelper = new Mock<ITokenHelper>();
            _handler = new UpdateVredTransactionServiceHandler(_httpConnector.Object,_tokenHelper.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenConnectorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdateVredTransactionServiceHandler(null,_tokenHelper.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenHelperObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdateVredTransactionServiceHandler(_httpConnector.Object, null));
        }

        [Fact]
        public void ServiceHandler_Success()
        {
            var request = TestDataProvider.GetTestData<CancelVredTransactionRequestData>("VRedMS\\Transaction\\CancelVredTransactionData");
            var response = TestDataProvider.GetTestData<UpdateTransactionResponse>("VRedMS\\Transaction\\CancelTransactionResponse");

            var vredRequest = (UpdateVredTransactionRequest)request;
            var serviceRequest = new UpdateVredTransactionService(vredRequest)
            {
                UpdateVredTransactionRequest = vredRequest
            };

            var detailsResponse = new HttpResponseMessage()
                {
                    Content = new StringContent(Json.Serialize(response.SuccessResponse)),
                    StatusCode = System.Net.HttpStatusCode.OK,
            };

            _tokenHelper.Setup(m => m.GetToken(It.IsAny<string>(),
                                      It.IsAny<string>(), It.IsAny<HttpContent>())).ReturnsAsync("abc");
            
            _httpConnector.Setup(m => m.SendAsync(It.IsAny<string>(),
                                                         It.IsAny<Dictionary<string, string>>(),
                                                         It.IsAny<string>(),
                                                         It.IsAny<HttpMethod>(),
                                                         It.IsAny<string>(),
                                                         It.IsAny<bool>())).Returns(Task.FromResult(detailsResponse));

            var result = _handler.Handle(serviceRequest, System.Threading.CancellationToken.None);

            Assert.NotNull(result.Result.SuccessResponse);
            Assert.Equal("AZ1716305170926", result.Result.SuccessResponse.PartnerTransactionId);
            Assert.Equal("VAA", result.Result.SuccessResponse.PartnerCode);
        }
    }
}
