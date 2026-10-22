using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Mutations;
using VAA.EAPI.GraphQL.Domain.Book.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Book.Tests
{
    public class OrderMutationTests
    {
        private readonly OrderMutation _mutation;
        private readonly Mock<ILogger<OrderMutation>> _mockLogger;
        private readonly Mock<IOrderMutationProvider> _mockOrderMutationProvider;
        private readonly Mock<IOfferServiceProvider> _mockOfferServiceProvider;
        private readonly Mock<IPaymentServiceProvider> _mockPaymentServiceProvider;
        private readonly Mock<IBasketServiceProvider> _mockBasketServiceProvider;
        private readonly Mock<IRefServiceProvider> _mockReferenceServiceProvider;
        private readonly Mock<IDistributedCache> _distributedCache;
        private readonly Mock<ITokenB2CProvider<string>> _mockB2CProvider;
        private readonly Mock<IOAuthTokenProvider> _mocktokenB2CProvider;
        private readonly Mock<IHttpConnector> _mockConnector;
        private readonly Mock<ISeatServiceProvider> _mockSeatServiceProvider;
        private readonly Mock<ICustomerProvider> _mockCustomerProvider;
        private readonly Mock<ICustomerServiceProvider> _mockCustomerEnrolmentProvider;
        private readonly Mock<IAnalyticsService> _analyticsService;
        private readonly Mock<IVoucherRedemptionProvider> _mockVoucherRedemptionProvider;

        public OrderMutationTests()

        {
            _mockLogger = new Mock<ILogger<OrderMutation>>();
            _mockOfferServiceProvider = new Mock<IOfferServiceProvider>();
            _mockOrderMutationProvider = new Mock<IOrderMutationProvider>();
            _mockBasketServiceProvider = new Mock<IBasketServiceProvider>();
            _mockPaymentServiceProvider = new Mock<IPaymentServiceProvider>();
            _mockReferenceServiceProvider = new Mock<IRefServiceProvider>();
            _distributedCache = new Mock<IDistributedCache>();
            _mockSeatServiceProvider = new Mock<ISeatServiceProvider>();
            _mockB2CProvider = new Mock<ITokenB2CProvider<string>>();
            _mockCustomerProvider = new Mock<ICustomerProvider>();
            _mocktokenB2CProvider = new Mock<IOAuthTokenProvider>();
            _mockCustomerEnrolmentProvider = new Mock<ICustomerServiceProvider>();
            _analyticsService = new Mock<IAnalyticsService>();
            _mockVoucherRedemptionProvider = new Mock<IVoucherRedemptionProvider>();
            _mockConnector = new Mock<IHttpConnector>();
            _mutation = new OrderMutation(_mockOrderMutationProvider.Object, _mockOfferServiceProvider.Object, _mockBasketServiceProvider.Object, _mockPaymentServiceProvider.Object, _mockLogger.Object, _mockReferenceServiceProvider.Object, _mockB2CProvider.Object, _distributedCache.Object, _mockSeatServiceProvider.Object, _mockCustomerProvider.Object, _mockCustomerEnrolmentProvider.Object, _analyticsService.Object, _mockVoucherRedemptionProvider.Object);

        }

        [Fact]
        public void ConstructorShouldThrowNullException_OrderMutationProvider()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderMutationProvider(null));
        }


        [Fact]
        public void ConstructorShouldThrowException_OrderMutation()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderMutation(null, null, null, null, null, null, null, null, null, null, null, null, _mockVoucherRedemptionProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new OrderMutation(_mockOrderMutationProvider.Object, null, null, null, null, null, null, null, null, null, null, null, _mockVoucherRedemptionProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new OrderMutation(_mockOrderMutationProvider.Object, _mockOfferServiceProvider.Object, null, null, null, null, null, null, null, null, null, null, _mockVoucherRedemptionProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new OrderMutation(_mockOrderMutationProvider.Object, _mockOfferServiceProvider.Object, _mockBasketServiceProvider.Object, null, null, null, null, null, null, null, null, null, _mockVoucherRedemptionProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new OrderMutation(_mockOrderMutationProvider.Object, _mockOfferServiceProvider.Object, _mockBasketServiceProvider.Object, _mockPaymentServiceProvider.Object, null, null, null, null, null, null, null, null, _mockVoucherRedemptionProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new OrderMutation(_mockOrderMutationProvider.Object, _mockOfferServiceProvider.Object, _mockBasketServiceProvider.Object, _mockPaymentServiceProvider.Object, _mockLogger.Object, null, null, null, null, null, null, null, _mockVoucherRedemptionProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new OrderMutation(_mockOrderMutationProvider.Object, _mockOfferServiceProvider.Object, _mockBasketServiceProvider.Object, _mockPaymentServiceProvider.Object, _mockLogger.Object, _mockReferenceServiceProvider.Object, _mockB2CProvider.Object, null, null, null, null, null, _mockVoucherRedemptionProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new OrderMutation(_mockOrderMutationProvider.Object, _mockOfferServiceProvider.Object, _mockBasketServiceProvider.Object, _mockPaymentServiceProvider.Object, _mockLogger.Object, _mockReferenceServiceProvider.Object, _mockB2CProvider.Object, _distributedCache.Object, null, null, null, null, _mockVoucherRedemptionProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new OrderMutation(_mockOrderMutationProvider.Object, _mockOfferServiceProvider.Object, _mockBasketServiceProvider.Object, _mockPaymentServiceProvider.Object, _mockLogger.Object, _mockReferenceServiceProvider.Object, _mockB2CProvider.Object, _distributedCache.Object, _mockSeatServiceProvider.Object, null, null, null, _mockVoucherRedemptionProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new OrderMutation(_mockOrderMutationProvider.Object, _mockOfferServiceProvider.Object, _mockBasketServiceProvider.Object, _mockPaymentServiceProvider.Object, _mockLogger.Object, _mockReferenceServiceProvider.Object, _mockB2CProvider.Object, _distributedCache.Object, _mockSeatServiceProvider.Object, _mockCustomerProvider.Object, null, null, _mockVoucherRedemptionProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new OrderMutation(_mockOrderMutationProvider.Object, _mockOfferServiceProvider.Object, _mockBasketServiceProvider.Object, _mockPaymentServiceProvider.Object, _mockLogger.Object, _mockReferenceServiceProvider.Object, _mockB2CProvider.Object, _distributedCache.Object, _mockSeatServiceProvider.Object, _mockCustomerProvider.Object, _mockCustomerEnrolmentProvider.Object, null, _mockVoucherRedemptionProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new OrderMutation(_mockOrderMutationProvider.Object, _mockOfferServiceProvider.Object, _mockBasketServiceProvider.Object, _mockPaymentServiceProvider.Object, _mockLogger.Object, _mockReferenceServiceProvider.Object, _mockB2CProvider.Object, _distributedCache.Object, _mockSeatServiceProvider.Object, _mockCustomerProvider.Object, _mockCustomerEnrolmentProvider.Object, _analyticsService.Object, null));
        }
    }
}
