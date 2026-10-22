using MediatR;
using Moq;
using Newtonsoft.Json;
using System;
using System.Linq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Query.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Tests
{
    public class TransactionResponseQueryHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly TransactionResponseQueryHandler _handler;

        public TransactionResponseQueryHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _handler = new TransactionResponseQueryHandler(_mediator.Object);
            Environment.SetEnvironmentVariable(Constants.Environment.ResponseDelayTimeInMS, "0");
            Environment.SetEnvironmentVariable(Constants.Environment.ResponsePollingCount, "1");
            VRedCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new TransactionResponseQueryHandler(null));
        }

        [Fact]
        public void Handler_Success()
        {
            TransactionResponseQuery QueryData = new TransactionResponseQuery { CorrelationId = "xxxx", Type = "VAA.Vred.CancelTransaction" };
            var response = TestDataProvider.GetTestData<TransactionQueryResponse>("VRedMS\\Transaction\\CancelTxnResponse");
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new ServiceResponse { Status = ServiceResponseProcessStatus.Completed, JsonResponseData = JsonConvert.SerializeObject(response) });
            var result = _handler.Handle(QueryData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
            Assert.Equal("AZ1716891124355", Json.Deserialize<TransactionResponse>(result.Result.Value.Value.Value).PartnerTransactionId);
            Assert.Equal("VAA", Json.Deserialize<TransactionResponse>(result.Result.Value.Value.Value).PartnerCode);
            Assert.Equal("2105128793", Json.Deserialize<TransactionResponse>(result.Result.Value.Value.Value).PartnerUserId);
        }

        [Fact]
        public void Handler_ErrorResponse()
        {
            TransactionResponseQuery QueryData = new TransactionResponseQuery { CorrelationId = "xxxx", Type = "VAA.Vred.CancelTransaction" };
            var response = TestDataProvider.GetTestData<TransactionQueryResponse>("VRedMS\\Transaction\\CancelTxnErrorResponse");
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new ServiceResponse { Status = ServiceResponseProcessStatus.Error, JsonResponseData = JsonConvert.SerializeObject(response) });
            var result = _handler.Handle(QueryData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.NotNull(result.Result.Value.Errors);
            Assert.Contains("schema violations found", result.Result.Value.Errors.FirstOrDefault().Message);
            Assert.Equal("400", result.Result.Value.Errors.FirstOrDefault().Code);
        }
    }
}
