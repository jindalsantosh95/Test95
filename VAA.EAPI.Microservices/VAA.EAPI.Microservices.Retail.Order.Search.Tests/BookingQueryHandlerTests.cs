using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Microservices.Retail.Order.Search.Application.Query;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Entity;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Response;
using VAA.EAPI.Microservices.Retail.Order.Search.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Search.Infrastructure.Repositories.SCVReferenceDB.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Tests
{
    public class BookingQueryHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<BookingQueryHandler>> _logger;
        private readonly BookingQueryHandler _handler;
        private readonly Mock<ISCVRefDBService> _refDBService;
        private readonly OrderIdRepoQueryHandler _repoHandler;
        private readonly Mock<ILogger<OrdersRepoQuery>> _ordersRepoQuery;
        private readonly Mock<TimeProvider> _timeProvider;

        public BookingQueryHandlerTests()
        {
            _refDBService = new Mock<ISCVRefDBService>();
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<BookingQueryHandler>>();
            _timeProvider = new Mock<TimeProvider>();
            _handler = new BookingQueryHandler(_mediator.Object, _timeProvider.Object);
            _ordersRepoQuery = new Mock<ILogger<OrdersRepoQuery>>();
            _repoHandler = new OrderIdRepoQueryHandler(_refDBService.Object);

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            OrderSearchTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new BookingQueryHandler(null, _timeProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new BookingQueryHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task FutureBookingQuery_ShouldPass_WithGoodRequest()
        {
            //Arrange
            _timeProvider.Setup(t => t.GetUtcNow()).Returns(new DateTime(2024, 10, 11));
            var request = TestDataProvider.GetTestData<FutureBookingQuery>("OrderSearchMS\\BookingQueryHandlerRequest");
            var orderDocResponse = TestDataProvider.GetTestData<IEnumerable<OrderDoc>>("OrderSearchMS\\BookingQueryOrderDocResponse");

            _mediator.Setup(x => x.Send(It.IsAny<OrderIdRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<string>()
            {
                "CW8TQ703NOV23", "CW7NXU03NOV23"
            });

            _mediator.Setup(x => x.Send(It.IsAny<OrdersRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(orderDocResponse);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.True(result.VaaBookingDetails.Any());

            var vaaBookingDetails = result.VaaBookingDetails.First();

            Assert.Equal("CW8TQ7", vaaBookingDetails.Rloc);

            Assert.True(vaaBookingDetails.VaaBookingSegmentDetails.Any());

            var vaaBookingSegmentDetails = vaaBookingDetails.VaaBookingSegmentDetails.First();

            Assert.Equal("DEL", vaaBookingSegmentDetails.DeparturePoint);
            Assert.Equal("LHR", vaaBookingSegmentDetails.ArrivalPoint);
        }

        [Fact]
        public async Task FutureBookingQuery_ShouldSucceed_WithGoodRequest()
        {
            //Arrange
            var apiResponse = new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.UnprocessableEntity };

            List<string> OrderID = new()
            {
                "EQQSA526JUN22"
            };

            var request = TestDataProvider.GetTestData<FutureBookingQuery>("OrderSearchMS\\GetBookingQueryRequest");
            var mockResponse = TestDataProvider.GetTestData<List<OrderDoc>>("OrderSearchMS\\OrderDocResponse");

            _mediator.Setup(m => m.Send(It.IsAny<OrderIdRepoQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(OrderID);
            _mediator.Setup(m => m.Send(It.IsAny<OrdersRepoQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(mockResponse);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);
            //Assert.True(result.IsSuccess == true);
            Assert.IsType<BookingResponse>(result);
            Assert.NotNull(result.VaaBookingDetails);
        }

        [Fact]
        public async Task LastNextBookingQuery_ShouldSucceed_WithGoodRequest()
        {
            //Arrange
            var apiResponse = new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.UnprocessableEntity };

            List<string> OrderID = new()
            {
                "EQQSA526JUN22"
            };

            var request = TestDataProvider.GetTestData<LastNextBookingQuery>("OrderSearchMS\\GetBookingQueryRequest");
            var mockResponse = TestDataProvider.GetTestData<List<OrderDoc>>("OrderSearchMS\\OrderDocResponse");

            _mediator.Setup(m => m.Send(It.IsAny<OrderIdRepoQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(OrderID);
            _mediator.Setup(m => m.Send(It.IsAny<OrdersRepoQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(mockResponse);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.IsType<BookingResponse>(result);
            Assert.NotNull(result.VaaCustomerDetails);
            Assert.NotNull(result.VaaBookingDetails);
        }

        [Fact]
        public async Task LastNextBookingQuery_ShouldThrowNullException_WithNullRequests()
        {
            var request = TestDataProvider.GetTestData<LastNextBookingQuery>("OrderSearchMS\\GetBookingQueryRequest");
            request.FCNumber = null;
            request.PhoneNo = null;

            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task LastNextBookingQuery_ShouldThrowNullException_WithEmptyRequests()
        {
            var request = TestDataProvider.GetTestData<LastNextBookingQuery>("OrderSearchMS\\GetBookingQueryRequest");
            request.FCNumber = "";
            request.PhoneNo = "";

            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(request, CancellationToken.None));
        }
    }
}
