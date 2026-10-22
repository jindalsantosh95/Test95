using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.OrderPayment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.VoidTransaction;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.RefundTicket;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.VoidTransaction;
using VAA.EAPI.Microservices.Retail.Order.Payment.Mapper;
using VAA.EAPI.Microservices.Retail.Order.Payment.Mapper.VoidTransaction;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    public class OrderPaymentMapperTests
    {
        public OrderPaymentMapperTests()
        {
            OrderPaymentTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void OrderPaymentCommandTOEventMapper_Success()
        {
            var orderPaymentCommand = TestDataProvider.GetTestData<OrderPaymentEventCommand>("PaymentMS\\OrderPaymentRequest");
            var orderEvent = OrderPaymentUpdateCommandToEventMapper.CommandToEventMapper(orderPaymentCommand);

            Assert.NotNull(orderEvent);
            Assert.True(orderEvent.Type == "VAA.Order.Payment.InitiateOrderPaymentUpdate");
        }

        [Fact]
        public void OrderPaymentStatusResponseMapper_Success()
        {
            var orderPaymentStatusRequestData = TestDataProvider.GetTestData<OrderPaymentStatusRequest>("PaymentMS\\OrderPaymentStatusRequest_Success");
            var correlationId = "123345666";
            var result = OrderPaymentResponseMapper.ResponseMapper(orderPaymentStatusRequestData, correlationId);
            Assert.NotNull(result);
            Assert.True(result.Status == "Succeeded");
            Assert.True(result.CorrelationId == "123345666");
        }

        [Fact]
        public void OrderPaymentResponseMapper_Success()
        {
            var correlationId = "1234657566";
            var status = Status.Accepted;
            var result = OrderPaymentUpdateEventCommandResponseMapper.OrderPaymentResponseMapper(correlationId, status);
            Assert.NotNull(result);
            Assert.True(result.Status == Status.Accepted);
            Assert.True(result.HeaderParameters == null);
        }

        [Fact]
        public void VoidOrderDataMapper_Success()
        {
            var voidOrderdata = TestDataProvider.GetTestData<VoidTransactionDetails>("PaymentMS\\VoidTransactionDetails");
            var result = VoidOrderDataMapper.MapToVoidOrderData(voidOrderdata);
            Assert.NotNull(result);
            Assert.NotNull(result.OrderRef);
            Assert.NotNull(result.FulfilmentId);
            Assert.NotNull(result.Payments);
        }

        [Fact]
        public void FulfillmentIdMapper_Success()
        {
            var fulfillmentDetails = TestDataProvider.GetTestData<VoidTransactionDetails>("PaymentMS\\VoidTransactionDetails");
            var result = VoidOrderDataMapper.MapToFulfilmentData(fulfillmentDetails);
            Assert.NotNull(result);
        }

        [Fact]
        public void PaymentMapper_Success()
        {
            var paymentDetails = TestDataProvider.GetTestData<VoidTransactionDetails>("PaymentMS\\VoidTransactionDetails");
            var result = VoidOrderDataMapper.MapToPayments(paymentDetails);
            Assert.NotNull(result);
        }

        [Fact]
        public void VoidOrderCommandToEventMapper_Success()
        {
            var voidOrderdata = TestDataProvider.GetTestData<VoidOrderUpdateData>("PaymentMS\\VoidOrderUpdateData");
            var request = TestDataProvider.GetTestData<VoidTransactionEventCommand>("PaymentMs\\VoidOrderDetails-CPD");
            var result = VoidTransactionMapper.MapToDataEvent(request, voidOrderdata);
            Assert.NotNull(result);
            Assert.True(result.Type == "VAA.Order.Payment.InitiateVoidOrderUpdate");
        }

        [Fact]
        public void EmailEventMapper_Success()
        {
            var request = TestDataProvider.GetTestData<VoidTransactionEventCommand>("PaymentMs\\VoidOrderDetails-CPD");
            var result = CPDBusinessErrorMapper.MapToEmailEvent(request);
            Assert.NotNull(result);
            Assert.True(result.Id == request.Id);
            Assert.True(result.Type == "VAA.Order.Payment.InitiateCPDErrorCommunication");
        }
    }
}
