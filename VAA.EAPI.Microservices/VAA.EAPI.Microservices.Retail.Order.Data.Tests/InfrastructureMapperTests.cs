using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Mapper.PaymentUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Mapper.RefundUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.PaymentUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.RefundUpdate;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Tests
{
    public class InfrastructureMapperTests
    {
        public InfrastructureMapperTests()
        {
            OrderDataTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void FulfillmentMapper_ShouldSucceed_WithGoodRequest()
        {
            var request1 = TestDataProvider.GetTestData<OrderCreateRequestData>("PaymentMs\\FulfillmentDetails");
            var result = FulfillmentMapper.MapFulfillmentDetails(request1);
            Assert.NotNull(result);
        }

        [Fact]
        public void EticketMapper_ShouldSucceed_WithGoodRequest()
        {
            var request1 = TestDataProvider.GetTestData<OrderCreateRequestData>("TicketMS\\EticketMapperData");
            var result = EticketMapper.MapEticket(request1);
            Assert.NotNull(result);
        }
        [Fact]
        public void CosmosReaderServiceQueryMapper_ShouldSucceed_WithGoodRequest()
        {
            var request1 = "string1";
            List<KeyValuePair<string, object>> keyValues = new List<KeyValuePair<string, object>>();
            keyValues.Add(new KeyValuePair<string, object>("string2", "ob"));
            var result = CosmosReaderServiceQueryMapper.Mapper(request1, keyValues);
            Assert.NotNull(result);
            Assert.True(result.SqlQuery == "string1");
        }
        [Fact]
        public void CosmosUpdateMapper_OrderUpdateShouldSucceed_WithGoodRequest()
        {
            var request1 = TestDataProvider.GetTestData<OrderDetails>("PaymentMs\\OrderDetailsData");
            var result = CosmosUpdateMapper.OrderUpdateServiceMapper(request1);
            Assert.NotNull(result);
        }
        [Fact]
        public void CosmosUpdateMapper_PnrSearchUpdateShouldSucceed_WithGoodRequest()
        {
            var request1 = TestDataProvider.GetTestData<PnrSearch>("PaymentMs\\PnrSearchData");
            var result = CosmosUpdateMapper.PnrSearchUpdateServiceMapper(request1);
            Assert.NotNull(result);
        }
        [Fact]
        public void PaymentUpdateOrderDocMapper_ShouldSucceed_WithGoodRequest()
        {
            var request1 = TestDataProvider.GetTestData<PaymentUpdateRepoCommand>("PaymentMs\\PaymentUpdateRepoCommand");
            var request2 = TestDataProvider.GetTestData<OrderDetails>("PaymentMs\\OrderDetailsData");
            var result = PaymentUpdateOrderDocMapper.MapToOrderDoc(request1, request2, "ID1234234");
            Assert.NotNull(result);
            Assert.True(result.SchemaVersion == "Version11");
        }
        [Fact]
        public void PaymentUpdatePnrSearchDocMapper_ShouldSucceed_WithGoodRequest()
        {
            var request1 = TestDataProvider.GetTestData<PaymentUpdateRepoCommand>("PaymentMs\\PaymentUpdateRepoCommand");
            var request2 = TestDataProvider.GetTestData<PnrSearch>("PaymentMs\\PnrSearchData");
            var result = PaymentUpdatePnrSearchDocMapper.MaptoPnrSeacrhDoc(request1, request2, "ID1234234");
            Assert.NotNull(result);
            Assert.True(result.OrderRef == "Ref1");
        }
        [Fact]
        public void RefundUpdateOrderDocMapper_ShouldSucceed_WithGoodRequest()
        {
            var request1 = TestDataProvider.GetTestData<RefundUpdateRepoCommand>("PaymentMs\\RefundUpdateRepoCommand");
            var request2 = TestDataProvider.GetTestData<OrderDetails>("PaymentMs\\OrderDetailsData");
            var result = RefundUpdateOrderDocMapper.MapToOrderDoc(request1, request2, "ID1234234");
            Assert.NotNull(result);
            Assert.True(result.OrderRef == "Ref1");
        }
        [Fact]
        public void RefundUpdatePnrSearchDocMapper_ShouldSucceed_WithGoodRequest()
        {
            var request1 = TestDataProvider.GetTestData<RefundUpdateRepoCommand>("PaymentMs\\RefundUpdateRepoCommand");
            var request2 = TestDataProvider.GetTestData<PnrSearch>("PaymentMs\\PnrSearchData");
            var result = RefundUpdatePnrSearchDocMapper.MaptoPnrSearchDoc(request1, request2, "ID1234234");
            Assert.NotNull(result);
            Assert.True(result.OrderRef == "Ref1");
        }
    }
}
