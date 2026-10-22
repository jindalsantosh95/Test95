using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.ReshopForCancellation;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{
    public class ReshopForCancellationQueryHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<ReshopForCancellationQueryHandler>> _logger;
        private readonly ReshopForCancellationQueryHandler _handler;
        private readonly Mock<IDistributedCacheService> _distributedCacheService;

        public ReshopForCancellationQueryHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<ReshopForCancellationQueryHandler>>();
            _distributedCacheService = new  Mock<IDistributedCacheService>();
            _handler = new ReshopForCancellationQueryHandler(_mediator.Object, _logger.Object, _distributedCacheService.Object);

            OfferTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ReshopForCancellationQueryHandler(null, _logger.Object, _distributedCacheService.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ReshopForCancellationQueryHandler(_mediator.Object, null, _distributedCacheService.Object));
        }

        [Fact]
        public async Task Handler_ValidRFCResponse()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<ReshopForCancellationQuery>("OfferMS\\OrderReshopCancellationValidRequest");
            var accountableDocument = request.ReshopOrder?.Select(order => order.AccountableDocumentRefs.FirstOrDefault());
            string scheduledDeparture = request.SegmentDetails?.First().ScheduledDepartureTime.LocalDateTime;
            string rLOCCreateDate = request.BookingReferences.RLOCCreateDate?.ToString("yyyy-MM-dd");
            string rLOCCreateDateTime = request.BookingReferences.RLOCCreateDateTime;

            var result = await _handler.Handle(request, CancellationToken.None);

            var validateRFC = DateTime.TryParseExact(rLOCCreateDate + " " + rLOCCreateDateTime, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime ticketIssueDateTime) && (DateTime.UtcNow - ticketIssueDateTime).TotalHours <= Convert.ToDouble(Constants.Application.RFCWindowInHours);
            if (accountableDocument.FirstOrDefault().DocumentType == Constants.Application.TicketDocumentType && validateRFC)
            {
                Assert.Equal(Status.OK, result.Value.Status);
            }
        }
    }
}
