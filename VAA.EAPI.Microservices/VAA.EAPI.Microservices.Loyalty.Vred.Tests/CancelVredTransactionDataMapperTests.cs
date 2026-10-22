using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Mapper;
using Xunit;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredTransactions;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Tests
{
    public class CancelVredTransactionDataMapperTests
    {
        public CancelVredTransactionDataMapperTests()
        {
            VRedCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void CancelVredTransactionDataMapperEarn_Success()
        {
            var request = TestDataProvider.GetTestData<CancelTransactionRequest>("VRedMS\\Transaction\\CancelTransactionRequest");
            var earnRequest = TestDataProvider.GetTestData<CreditMetaData>("VRedMS\\Transaction\\EarnTransactionDetails");

            var vredTransactionData = CancelVredTransactionDataMapper.CancelVredTransactionRequestMapper(request, earnRequest, null);

            Assert.Equal("2105128793", vredTransactionData.MemberNumber);
            Assert.Equal("Accrual", vredTransactionData.ActivityType);
            Assert.Equal("AZ1716305618999", vredTransactionData.TransactionId);
            Assert.NotNull(vredTransactionData.Details);
        }

        [Fact]
        public void CancelVredTransactionDataMapperBurn_Success()
        {
            var request = TestDataProvider.GetTestData<CancelTransactionRequest>("VRedMS\\Transaction\\CancelTransactionRequest");
            var burnRequest = TestDataProvider.GetTestData<DebitMetaData>("VRedMS\\Transaction\\BurnTransactionDetails");

            var vredTransactionData = CancelVredTransactionDataMapper.CancelVredTransactionRequestMapper(request, new CreditMetaData(), burnRequest);

            Assert.Equal("2105128793", vredTransactionData.MemberNumber);
            Assert.Equal("Redemption", vredTransactionData.ActivityType);
            Assert.Equal("AZ1716305618999", vredTransactionData.TransactionId);
            Assert.NotNull(vredTransactionData.Details);
        }

        [Fact]
        public void CompensatoryBurnTransactionRequestMapper_Success()
        {
            var request = TestDataProvider.GetTestData<CancelTransactionRequest>("VRedMS\\Transaction\\CancelTransactionRequest");
            var earnRequest = TestDataProvider.GetTestData<CreditMetaData>("VRedMS\\Transaction\\EarnTransactionDetails");

            var burnTransactionData = CancelVredTransactionDataMapper.CompensatoryBurnTransactionRequestMapper(request,earnRequest,"test123");

            Assert.Equal("test123", burnTransactionData.Id);
            Assert.IsType<CreateTransactionRequest>(burnTransactionData.Request);
            Assert.Equal("Accrual Cancellation", burnTransactionData.Request.TransactionSubType);
            Assert.Equal("2105128793", burnTransactionData.Request.MemberNumber);
            Assert.Equal("DF", burnTransactionData.Request.PartnerCode);
            Assert.Equal("Duty Free", burnTransactionData.Request.PartnerName);
            Assert.Equal("Ground Partner", burnTransactionData.Request.PartnerCategory);
            Assert.Equal("RetailTherapy", burnTransactionData.Request.UpdatedBy);
            Assert.Equal("WEBSITE", burnTransactionData.Request.TransactionChannel);
        }

        [Fact]
        public void CompensatoryEarnTransactionRequestMapper_Success()
        {
            var request = TestDataProvider.GetTestData<CancelTransactionRequest>("VRedMS\\Transaction\\CancelTransactionRequest");
            var burnRequest = TestDataProvider.GetTestData<DebitMetaData>("VRedMS\\Transaction\\BurnTransactionDetails");

            var earnTransactionData = CancelVredTransactionDataMapper.CompensatoryEarnTransactionRequestMapper(request, burnRequest, "test123");

            Assert.Equal("test123", earnTransactionData.Id);
            Assert.IsType<CreateTransactionRequest>(earnTransactionData.Request);
            Assert.Equal("Redemption Cancellation", earnTransactionData.Request.TransactionSubType);
            Assert.Equal("2105128793", earnTransactionData.Request.MemberNumber);
            Assert.Equal("DF", earnTransactionData.Request.PartnerCode);
            Assert.Equal("Duty Free", earnTransactionData.Request.PartnerName);
            Assert.Equal("Ground Partner", earnTransactionData.Request.PartnerCategory);
            Assert.Equal("RetailTherapy", earnTransactionData.Request.UpdatedBy);
            Assert.Equal("WEBSITE", earnTransactionData.Request.TransactionChannel);
        }
    }
}
