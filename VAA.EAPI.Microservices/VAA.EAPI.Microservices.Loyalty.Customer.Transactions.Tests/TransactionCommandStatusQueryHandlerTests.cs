using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Reflection.Metadata;
using System.Threading;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class TransactionCommandStatusQueryHandlerTests
    {
        protected readonly Mock<IMediator> _mockMediator = new();
        protected readonly Mock<ILogger<CommandStatusBaseQueryHandler<TransactionCommandQueryResponse>>> _mockLogger = new();
        private readonly TransactionCommandStatusQueryHandler _handler;

        public TransactionCommandStatusQueryHandlerTests()
        {
            _handler = new TransactionCommandStatusQueryHandler(_mockMediator.Object, _mockLogger.Object);
            Environment.SetEnvironmentVariable(EnvironmentSettings.CommandStatusPollingDelayMS, "0");
            Environment.SetEnvironmentVariable(EnvironmentSettings.CommandStatusPollingRetryCount, "1");
            CampaignTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TransactionCommandStatusQueryHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TransactionCommandStatusQueryHandler(_mockMediator.Object, null));
        }

        [Fact]
        public void Handler_Success()
        {
            TransactionCommandStatusQuery QueryData = new TransactionCommandStatusQuery { RequestId = "xxxx" };
            var response = TestDataProvider.GetTestData<string>("Transaction\\TransactionCreateCommandStatusResponse");
            
            _mockMediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), CancellationToken.None)).ReturnsAsync(new ServiceResponse { Status = ServiceResponseProcessStatus.Completed, JsonResponseData = response });
            
            var result = _handler.Handle(QueryData, CancellationToken.None);
            
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
            Assert.Equal("171862872013377702", result.Result.Value.Value.Transaction.TransactionId);
            Assert.Equal("170678665891194791", result.Result.Value.Value.Transaction.AwardId);
            Assert.Equal("PowerApps", result.Result.Value.Value.Transaction.TransactionChannel);
            Assert.Equal("Processed", result.Result.Value.Value.Transaction.StatusCode);
            Assert.Equal("test", result.Result.Value.Value.Transaction.Comments);
            Assert.Equal("995493454365436", result.Result.Value.Value.Transaction.FlightDetails.Ticketnumber);
            Assert.Equal("QWRTRE", result.Result.Value.Value.Transaction.FlightDetails.ExternalReservationNumber);
        }
    }
}
