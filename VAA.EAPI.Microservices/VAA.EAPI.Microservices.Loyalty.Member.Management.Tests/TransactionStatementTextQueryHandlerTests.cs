using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.TransactionStatement;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class TransactionStatementTextQueryHandlerTests
    {
        private readonly Mock<ILogger<TransactionStatementTextQueryHandler>> _mockLogger;
        private readonly Mock<ITxnStatementTextService> _mockStmtService;
        private readonly TransactionStatementTextQueryHandler _handler;

        public TransactionStatementTextQueryHandlerTests()
        {
            _mockLogger = new Mock<ILogger<TransactionStatementTextQueryHandler>>();
            _mockStmtService = new Mock<ITxnStatementTextService>();
            _handler = new TransactionStatementTextQueryHandler(_mockLogger.Object, _mockStmtService.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TransactionStatementTextQueryHandler(null, _mockStmtService.Object));
            Assert.Throws<ArgumentNullException>(() => new TransactionStatementTextQueryHandler(_mockLogger.Object, null));
        }

        [Fact]
        public async Task HandlerShouldSuccess()
        {
            var request = TestDataProvider.GetTestData<StatementTextRequest>("MemberManagement\\StatementTextRequest");
            TransactionStatementTextQuery query = new TransactionStatementTextQuery { StatementDescriptionRequest = request };

            StatementTextResponse resposne = new StatementTextResponse { StatementText = "Point credit for reward voucher redemption" };

            _mockStmtService.Setup(x => x.GetTransactionStatementText(query)).ReturnsAsync(resposne);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.Equal(resposne.StatementText, result.Value.Value.StatementText);
        }

        [Fact]
        public async Task HandlerShouldSuccessWithValidationFailure()
        {
            var request = TestDataProvider.GetTestData<StatementTextRequest>("MemberManagement\\StatementTextRequest");
            request.TransactionType = "";

            TransactionStatementTextQuery query = new TransactionStatementTextQuery { StatementDescriptionRequest = request };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.Equal("G012", result.Value.Errors.FirstOrDefault().Code);
            Assert.Equal("One of the mandatory field is missing in the request.", result.Value.Errors.FirstOrDefault().Message);
        }

        [Fact]
        public async Task HandlerShouldFailWithException()
        {
            var request = TestDataProvider.GetTestData<StatementTextRequest>("MemberManagement\\StatementTextRequest");
            TransactionStatementTextQuery query = new TransactionStatementTextQuery { StatementDescriptionRequest = request };

            _mockStmtService.Setup(x => x.GetTransactionStatementText(query)).ThrowsAsync(new Exception());

            var result = await _handler.Handle(query, CancellationToken.None);

            var errorDetails = Json.Deserialize<List<ErrorDetails>>(result.Error).FirstOrDefault();

            Assert.Equal("G009", errorDetails.Code);
            Assert.Equal("There was an unexpected error.", errorDetails.Message);
        }
    }
}
