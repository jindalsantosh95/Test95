using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Search.Application.Query;
using VAA.EAPI.Microservices.Retail.Order.Search.Infrastructure.Repositories.SCVReferenceDB.Model;
using VAA.EAPI.Microservices.Retail.Order.Search.Infrastructure.Repositories.SCVReferenceDB.Services;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Tests
{
    public class SCVRefDBServiceTests
    {
        public readonly Mock<SCVReferenceDBContext> _refDbContext;
        //private readonly Mock<ISCVRefDBService> _irefDbContext;
        private readonly Mock<ILogger<SCVRefDBService>> _logger;
        private readonly SCVRefDBService _handler;

        public SCVRefDBServiceTests()
        {
            _refDbContext = new Mock<SCVReferenceDBContext>();
            //_irefDbContext = new Mock<ISCVRefDBService>();
            _logger = new Mock<ILogger<SCVRefDBService>>();
            _handler = new SCVRefDBService(_refDbContext.Object, _logger.Object);
            //_irefDbContext = new Mock<ISCVRefDBService>();

            OrderSearchTestsCommon.CreateTestEnvironmentSettings();
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenDBContextObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new SCVRefDBService(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new SCVRefDBService(_refDbContext.Object, null));
        }
        [Fact]
        public void SCVRefDBService_ShouldSucceed_WithGoodRequest()
        {

            var response = TestDataProvider.GetTestData<List<Orders>>("OrderSearchMS\\CustomerOrderData");
            var data = response.AsQueryable();

            var mockSet = new Mock<DbSet<Orders>>();
            mockSet.As<IQueryable<Orders>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Orders>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Orders>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Orders>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<SCVReferenceDBContext>();
            mockContext.Setup(m => m.Orders).Returns(mockSet.Object);


            var request = TestDataProvider.GetTestData<LastNextBookingQuery>("OrderSearchMS\\GetBookingQueryData");
            var result = new SCVRefDBService(mockContext.Object, _logger.Object);
            var orderResult = result.GetCustomerOrders(request);
            Assert.NotNull(orderResult);
        }

        [Fact]
        public void SCVRefDBService_ShouldSucceed_WithNullPhoneNumber()
        {
            var response = TestDataProvider.GetTestData<List<Orders>>("OrderSearchMS\\CustomerOrderData");
            var data = response.AsQueryable();

            var mockSet = new Mock<DbSet<Orders>>();
            mockSet.As<IQueryable<Orders>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Orders>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Orders>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Orders>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<SCVReferenceDBContext>();
            mockContext.Setup(m => m.Orders).Returns(mockSet.Object);

            var request = TestDataProvider.GetTestData<LastNextBookingQuery>("OrderSearchMS\\GetBookingQueryData");
            request.PhoneNo = null;
            var result = new SCVRefDBService(mockContext.Object, _logger.Object);
            var orderResult = result.GetCustomerOrders(request);
            Assert.NotNull(orderResult);
        }
        [Fact]
        public void SCVRefDBService_ShouldSucceed_WithNullFCNumber()
        {
            var response = TestDataProvider.GetTestData<List<Orders>>("OrderSearchMS\\CustomerOrderData");
            var data = response.AsQueryable();

            var mockSet = new Mock<DbSet<Orders>>();
            mockSet.As<IQueryable<Orders>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Orders>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Orders>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Orders>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<SCVReferenceDBContext>();
            mockContext.Setup(m => m.Orders).Returns(mockSet.Object);

            var request = TestDataProvider.GetTestData<LastNextBookingQuery>("OrderSearchMS\\GetBookingQueryData");
            request.FCNumber = null;
            var result = new SCVRefDBService(mockContext.Object, _logger.Object);
            var orderResult = result.GetCustomerOrders(request);
            Assert.NotNull(orderResult);
        }

        [Fact]
        public void ShouldSucceed_WithEmptyPhoneNumberAndEmptyFCNumber()
        {
            var request = TestDataProvider.GetTestData<LastNextBookingQuery>("OrderSearchMS\\GetBookingQueryData");
            request.FCNumber = "";
            request.PhoneNo = "";
            var mockContext = new Mock<SCVReferenceDBContext>();
            var result = new SCVRefDBService(mockContext.Object, _logger.Object);
            var orderResult = result.GetCustomerOrders(request);
            Assert.NotNull(orderResult);
        }

        [Fact]
        public async Task SCVRefDBService_ShouldThrowException_WithBadRequest()
        {
            var request = TestDataProvider.GetTestData<LastNextBookingQuery>("OrderSearchMS\\GetBookingQueryData");
            await Assert.ThrowsAsync<NullReferenceException>(() => _handler.GetCustomerOrders(request));

        }
    }
}
