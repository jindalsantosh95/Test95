using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Application.OrderChange;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.EmailMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.IFGAgentFopLookUp;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OfferMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentIFGMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Order.Mapper.Order;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests.OrderChange
{
    public class OrderChangeCancelCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly OrderChangeCancelCommandHandler _handler;
        private readonly IHostEnvironment _hostEnvironment;

        public OrderChangeCancelCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _hostEnvironment = new HostingEnvironment { ContentRootPath = Directory.GetCurrentDirectory() };
            _serviceProvider = new ServiceCollection().AddLogging()
                .AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssemblyContaining<OrderChangeCancelCommandHandler>();
                })

                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IHostEnvironment>(_hostEnvironment)
                .AddSingleton<PaymentIFGMSOrderRefundCommandHandler>()
                .AddSingleton<CreatePaymentDocumentCommandHandler>()
                .AddSingleton<TransactionCommandHandler>()
                .AddSingleton<ICreatePaymentDocumentCommand, CreatePaymentDocumentCommandHandler>()
                .AddSingleton<PublishOrderDocMSCommandHandler>()
                .AddSingleton<IFGAgentFopLookUpCommandHandler>()
                .AddSingleton<ISendOrderEmailCommand, SendOrderEmailCommandHandler>()
                .AddSingleton<ITrackAnalyticsEventCommandHandler>(new Mock<ITrackAnalyticsEventCommandHandler>().Object)
                .AddSingleton<ITransactionCommand, TransactionCommandHandler>()
                .AddSingleton<ICurrencyConversionQuery, CurrencyConversionQueryHandler>()
                .AddSingleton<ICheckAgentStatusQuery, CheckAgentStatusQueryHandler>()
                .AddSingleton<AirportDetailsQueryHandler>()
                .AddSingleton<PaymentMSRetrieveQueryHandler>()
                .AddSingleton<IARCOrderDataMSQuery, ARCOrderDataMSQueryHandler>()
                .AddSingleton<OrderChangeCancelCommandHandler>()
                .AddSingleton<ValidationMSCommandHandler>()
                .AddSingleton<CancelReshopOfferMSQueryHandler>()
                .AddSingleton<OrderDataMSQueryHandler>()
                .AddSingleton<OrderSearchMSCommandHandler>()
                .AddSingleton<PaymentMSRefundCommandHandler>()

                .BuildServiceProvider();

            _handler = _serviceProvider.GetService<OrderChangeCancelCommandHandler>();
        }

        [Fact]
        public void Handler_ShouldBeResolved_From_ServiceProvider()
        {
            var handler = _serviceProvider.GetService<OrderChangeCancelCommandHandler>();
            Assert.NotNull(handler);
        }

        [Fact]
        public async Task PaymentIFGRefundHandler_ShouldReturnSuccess()
        {
            var refudnHandler = _serviceProvider.GetService<PaymentIFGMSOrderRefundCommandHandler>();

            var request = new PaymentIFGMSOrderRefundCommand()
            {
                Parties = new()
                {
                    Participants = [new() { Ordinal = "1", AgencyID = "9128103", Name = "Skyscanner" }]
                },
                PaymentDetails = new List<Payment>()
                {
                    new()
                    {
                        Amount = new()
                        {
                            MonetaryAmount = "1200",
                            BaseCurrency = "GBP2"
                        }
                    }
                },
                CustomerDetails = new List<Customer>()
                {
                    new()
                    {
                        CustId  ="ADULT_1",
                        Ptc  ="ADT"
                    }
                },
                OrderId = "VS932RVJLVA25"
            };

            ConfigReader.Set("PaymentIFGMSOrderRefundUrl", "https://Testresources.virginatlantic.com/uat/IFGPayment/IFGPaymentOrderRefund");
            ConfigReader.Set("PaymentIFGMSSubscriptionKey", "Test_PaymentIFGMSSubscriptionKey");

            ConfigReader.Set("ARCPaymentMSUrl", "https://Testresources.virginatlantic.com/dev/ARCPayment/");
            ConfigReader.Set("ARCPaymentMSSubscriptionKey", "Test_ARCPaymentMSSubscriptionKey");

            //Act 
            var result = await refudnHandler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.Equal("27be8079-9372-4b71-b80f-3064054f982f", result.PaymentDocument.TrackId);
        }

        [Fact]
        public async Task CreatePaymentDocumentCommandHanlder_ShouldReturnSuccess()
        {
            var createPaymentHandler = _serviceProvider.GetService<CreatePaymentDocumentCommandHandler>();
            //Arrange
            var request = new PaymentDocRequest()
            {
                Type = "PaymentRefundDoc",
                ProductType = Constants.ProductType.Ticket,
                OrderId = "VS932TIXLII25",
                Parties = new()
                {
                    Participants = [new() { Ordinal = "1", AgencyID = "9128103", Name = "Skyscanner" }]
                },
                PaymentDetails = new PaymentDetails[]
                {
                    new()
                    {
                        Amount = new()
                        {
                            MonetaryAmount = "125.34",
                            BaseCurrency = "GBP"
                        },
                        FormOfPayments = PaymentDetails.FormOfPayment.CashEnum,
                        TrackId = "b4925067-ba8f-4df7-b8a8-6b66e5cac614"
                    }
                }
            };

            ConfigReader.Set("CreatePaymentDocumentUrl", "https://Testresources.virginatlantic.com/DEV/orderpaymentmicroservice/CreatePaymentDocument");
            ConfigReader.Set("PaymentMSSubscriptionKey", "Test_PaymentMSSubscriptionKey");

            ConfigReader.Set("ARCPaymentMSUrl", "https://Testresources.virginatlantic.com/dev/ARCPayment/");
            ConfigReader.Set("ARCPaymentMSSubscriptionKey", "Test_ARCPaymentMSSubscriptionKey");

            //Act 
            var result = await createPaymentHandler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result.PaymentDocument.TransactionId);
            Assert.Equal("125.34", result.PaymentDocument.Amount.ToString());
        }

        [Fact]
        public void USDCurrencyMappings_ShouldReturn_Success_Tests()
        {
            var orderDocument = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderChange\\OrderDocUSCurrency");
            var result = OrderResponseMapper.MapOrderResponse(orderDocument, null);
            var response = ((OrderViewResponseType[])result.Items).First();

            Assert.NotNull(response.Order.First().OrderItem.First());
            Assert.Equal("USD", response.Order.First().TotalPrice.TotalAmount.CurCode.ToString());
            Assert.Equal("0.00", response.Order.First().TotalPrice.TotalAmount.Value.ToString());
            Assert.True(result.PaymentFunctions.All(x => x.PaymentProcessingSummary.Amount.CurCode == "USD"));
        }

        [Fact]
        public async Task TranscationAPIShouldReturnSuccess_ARC()
        {
            var transactionHandler = _serviceProvider.GetService<TransactionCommandHandler>();
            var transcationRequest = TestDataProvider.GetTestData<TransactionServiceRequest>("NDCOrder\\OrderChange\\TransactionRequest");

            ConfigReader.Set("ARCPaymentMSUrl", "https://Testresources.virginatlantic.com/dev/ARCPayment/");
            ConfigReader.Set("ARCPaymentMSSubscriptionKey", "Test_ARCPaymentMSSubscriptionKey");

            var result = await transactionHandler.Handle(transcationRequest);

            Assert.NotNull(result);
            Assert.Equal("f4da1759-68ba-49fb-bd5e-3bfc278f196b", result.RequestId);
            Assert.Equal("SUCCESS", result.Status);

        }

        [Fact]
        public void DisruptionCancel_ShouldReturn_Success_Tests()
        {
            var orderDocument = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderChange\\OrderDocDisruptionCancel");
            var result = OrderResponseMapper.MapOrderResponse(orderDocument, null);
            var response = ((OrderViewResponseType[])result.Items).First();

            Assert.NotNull(response.Order.First().OrderItem.First());
            Assert.Equal("CANCELLED", response.Order.First().OrderItem.First().StatusCode);
            Assert.True(result.PaymentFunctions.Count() == 2);
            Assert.Equal("GBP", response.Order.First().TotalPrice.TotalAmount.CurCode.ToString());
            Assert.Equal("0.00", response.Order.First().TotalPrice.TotalAmount.Value.ToString());
            Assert.Equal("CLOSED", response.Order.First().StatusCode);
        }

        [Fact]
        public async Task PaymentIFGCCRefundHandler_ShouldReturnSuccess()
        {
            var refundHandler = _serviceProvider.GetService<PaymentIFGMSOrderRefundCommandHandler>();
            var request = new PaymentIFGMSOrderRefundCommand()
            {
                Parties = new()
                {
                    Participants = [new() { Ordinal = "1", AgencyID = "9128103", Name = "Skyscanner" }]
                },
                PaymentDetails = new List<Payment>()
                {
                    new()
                    {
                        Amount = new()
                        {
                            MonetaryAmount = "61379",
                            BaseCurrency = "GBP2"
                        }
                    }
                },
                CustomerDetails = new List<Customer>()
                {
                    new()
                    {
                        CustId  ="ADULT_1",
                        Ptc  ="ADT"
                    }
                },
                OrderId = "VS932TYEOQS26"
            };

            ConfigReader.Set("PaymentIFGMSOrderRefundUrl", "https://Testresources.virginatlantic.com/uat/IFGPayment/IFGCCPaymentOrderRefund");
            ConfigReader.Set("PaymentIFGMSSubscriptionKey", "Test_PaymentIFGMSSubscriptionKey");

            //Act 
            var result = await refundHandler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.Equal("ec5c51d5-8ced-44e3-8e10-cfad5886427e", result.PaymentDocument.TrackId);
            Assert.Equal("81925305-Refund-1", result.PaymentDocument.TransactionId);
        }

        [Fact]
        public void PaymentIFGCCRefundMapperShouldReturnSuccess_()
        {
            var offerDoc = TestDataProvider.GetTestData<CancelReshopResponse>("NDCOrder\\OrderChange\\OfferDocIFGCC");
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderChange\\OrderDocIFGCC");
            var latestHistories = TestDataProvider.GetTestData<Order.Infrastructure.Model.OrderMS.OrderHistory>("NDCOrder\\OrderChange\\OrderDocIFGCC-History");
            var paymentDoc = TestDataProvider.GetTestData<PaymentTransactionDocument>("NDCOrder\\OrderChange\\PaymentDocIFGCC");

            var result = PaymentMSRequestMapper.MapOrderCancelCCIFGRefundRequest(offerDoc, orderDoc, latestHistories, paymentDoc);

            Assert.Equal("5e4f9bdc-bdc2-4fcb-ad4c-4ae1b7af798b", result.TrackId);
            Assert.Equal("VS932TXPAFV26", result.OrderId);
            Assert.True(result.IsRfc);
            Assert.NotNull(result.OrderHistory);
            Assert.True(result.PaymentDetails.First().FormOfPayments == PaymentDetails.FormOfPayment.CardEnum);
            Assert.Equal("829800", result.PaymentDetails.First().Amount.MonetaryAmount);
        }

        [Fact]
        public async Task OrderChangeCancelShouldReturnSuccessForPartialFlownPNR()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeCancelPartialFlownReq");

            ConfigReader.Set("OrderDataMSEndpoint", "https://Testresources.virginatlantic.com/dev/orderdata/orders/");
            ConfigReader.Set("OrderDataMSSubscriptionKey", "Test_OrderDataMSSubscriptionKey");
            ConfigReader.Set("ValidationMSUrl", "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/Validation");
            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("AgentRegistrationEnvironment", "PREPROD");
            ConfigReader.Set("CancelReshopOfferMSDocEndpoint", "https://Testresources.virginatlantic.com/dev/Offermicroservice/CancelReshopOffers/");
            ConfigReader.Set("OfferMSSubscriptionKey", "Test_OfferMSSubscriptionKey");
            ConfigReader.Set("IsIfgIntegrated", "false");
            ConfigReader.Set("OrderSearchMSUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/OrderSearch");
            ConfigReader.Set("OrderSearchMSSubscriptionKey", "Test_OrderSearchMSSubscriptionKey");
            ConfigReader.Set("PublishOrderDocUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/PublishOrderDoc");
            ConfigReader.Set("OrderCancelMSUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderManagement/ordercancel");
            ConfigReader.Set("OrderCancelMSSubscriptionKey", "Test_OrderCancelMSSubscriptionKey");
            ConfigReader.Set("PaymentMSSubscriptionKey", "Test_PaymentMSSubscriptionKey");
            ConfigReader.Set("PaymentMSRetrieveUrl", "https://Testresources.virginatlantic.com/dev/digital/orderpaymentmicroservice/payment/");
            ConfigReader.Set("PaymentMSRefundUrl", "https://Testresources.virginatlantic.com/dev/digital/orderpaymentmicroservice/RefundPayment");

            //Act
            var result = await _handler.Handle(new OrderChangeCancelCommand() { OrderChangeRQ = request }, CancellationToken.None);

            //Assert
            var response = (result.Value.Items.First()) as OrderViewResponseType;
            Assert.NotNull(result);
            Assert.Equal("VS932SIXTLB26", response.Order.First().OrderID);
            Assert.Equal("B", response.TicketDocInfo.First().Ticket.First().Coupon.First().CouponStatusCode);
            Assert.Equal("RF", response.TicketDocInfo.First().Ticket.First().Coupon.Last().CouponStatusCode);
            Assert.Equal("9322520403550", response.TicketDocInfo.First().Ticket.First().TicketNumber);
            Assert.Equal("CLOSED", response.Order.First().StatusCode);
            Assert.True(response.Order.First().OrderItem.All(x => x.StatusCode == "CLOSED"));
        }
    }
}
