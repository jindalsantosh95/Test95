using Moq;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.TransactionStatement;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class TxnStatementTextServiceTest
    {
        private readonly Mock<IBlobStorageServices> _mockBlobServices;
        private readonly TxnStatementTextService _service;

        public TxnStatementTextServiceTest()
        {
            _mockBlobServices = new Mock<IBlobStorageServices>();
            _service = new TxnStatementTextService(_mockBlobServices.Object);

            MemberTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowObjectNullError()
        {
            Assert.Throws<ArgumentNullException>(() => new TxnStatementTextService(null));
        }

        [Fact]
        public async Task GetTransactionStatementTextAccrual()
        {
            var request = TestDataProvider.GetTestData<StatementTextRequest>("MemberManagement\\StatementTextRequest");
            var stmtValues = TestDataProvider.GetTestData<StatementTextValues>("MemberManagement\\StatementTextValues");
            TransactionStatementTextQuery query = new TransactionStatementTextQuery { StatementDescriptionRequest = request };

            _mockBlobServices.Setup(m => m.Deserialize<StatementTextValues>()).ReturnsAsync(stmtValues);

            var result = await _service.GetTransactionStatementText(query);

            Assert.Equal("Point credit for reward voucher redemption", result.StatementText);
        }

        [Fact]
        public async Task GetTransactionStatementTextAccrualCodeshareFlight()
        {
            var request = TestDataProvider.GetTestData<StatementTextRequest>("MemberManagement\\StatementTextRequest");
            request.ProductName = "Flight";
            request.VACodeShareFlag = "Y";
            request.MarketingCarrierCode = "DL";
            request.CabinClass = "Economy";

            var stmtValues = TestDataProvider.GetTestData<StatementTextValues>("MemberManagement\\StatementTextValues");
            TransactionStatementTextQuery query = new TransactionStatementTextQuery { StatementDescriptionRequest = request };

            _mockBlobServices.Setup(m => m.Deserialize<StatementTextValues>()).ReturnsAsync(stmtValues);

            var result = await _service.GetTransactionStatementText(query);

            Assert.Equal("DL/Virgin Atlantic Airlines codeshare, LHR - SFO, Economy", result.StatementText);
        }

        [Fact]
        public async Task GetTransactionStatementTextRedemption()
        {
            var request = TestDataProvider.GetTestData<StatementTextRequest>("MemberManagement\\StatementTextRequest");
            request.TransactionType = "Redemption";
            request.ProductName = "Flight Redemption";

            var stmtValues = TestDataProvider.GetTestData<StatementTextValues>("MemberManagement\\StatementTextValues");
            TransactionStatementTextQuery query = new TransactionStatementTextQuery { StatementDescriptionRequest = request };

            _mockBlobServices.Setup(m => m.Deserialize<StatementTextValues>()).ReturnsAsync(stmtValues);

            var result = await _service.GetTransactionStatementText(query);

            Assert.Equal("VS Flight Redemption -Booking Ref DSADFSG ,1PAX ,LHR-SFO", result.StatementText);
        }

        [Fact]
        public async Task GetTransactionStatementTextCancellation()
        {
            var request = TestDataProvider.GetTestData<StatementTextRequest>("MemberManagement\\StatementTextRequest");
            request.TransactionType = "Redemption";
            request.TransactionSubType = "Accrual Cancellation";
            request.StatementText = "Point credit for reward voucher redemption";

            var stmtValues = TestDataProvider.GetTestData<StatementTextValues>("MemberManagement\\StatementTextValues");
            TransactionStatementTextQuery query = new TransactionStatementTextQuery { StatementDescriptionRequest = request };

            _mockBlobServices.Setup(m => m.Deserialize<StatementTextValues>()).ReturnsAsync(stmtValues);

            var result = await _service.GetTransactionStatementText(query);

            Assert.Equal("Cancelled: Point credit for reward voucher redemption", result.StatementText);
        }
    }
}
