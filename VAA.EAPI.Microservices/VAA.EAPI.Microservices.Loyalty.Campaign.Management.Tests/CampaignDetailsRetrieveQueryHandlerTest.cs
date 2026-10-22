using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignDetailsRetrieveQueryHandlerTest
    {
        private readonly Mock<IReportingDBServices> _mockReportingDBServices;
        private readonly Mock<ILogger<CampaignDetailsRetrieveQueryHandler>> _mockLogger;
        private readonly Mock<IMediator> _mockMediator;
        private readonly CampaignDetailsRetrieveQueryHandler _handler;

        public CampaignDetailsRetrieveQueryHandlerTest()
        {
            _mockReportingDBServices = new Mock<IReportingDBServices>();
            _mockLogger = new Mock<ILogger<CampaignDetailsRetrieveQueryHandler>>();
            _mockMediator = new Mock<IMediator>();
            _handler = new CampaignDetailsRetrieveQueryHandler(_mockMediator.Object, _mockLogger.Object, _mockReportingDBServices.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
             new CampaignDetailsRetrieveQueryHandler(null, _mockLogger.Object, _mockReportingDBServices.Object));

            Assert.Throws<ArgumentNullException>(() =>
             new CampaignDetailsRetrieveQueryHandler(_mockMediator.Object, null, _mockReportingDBServices.Object));

            Assert.Throws<ArgumentNullException>(() =>
             new CampaignDetailsRetrieveQueryHandler(_mockMediator.Object, _mockLogger.Object, null));
        }

        [Fact]
        public async Task HandlerSuccessWithNoError()
        {
            CampaignSearchResponse campaignSearchResponse = TestDataProvider.GetTestData<CampaignSearchResponse>("CampaignManagementMS\\CampaignSearchResponse");

            _mockMediator.Setup(m => m.Send(It.IsAny<CampaignSearchQuery>(), CancellationToken.None)).ReturnsAsync(new Response<CampaignSearchResponse>() { Status = Status.OK, Value = campaignSearchResponse });
            _mockReportingDBServices.Setup(x => x.GetAssociatedMemberCount(It.IsAny<string>())).ReturnsAsync(4);
            CampaignDetailsRetrieveQuery query = new() { CampaignId = null, CampaignCode = "VIRGIN007" };

            var result = await _handler.Handle(query, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);
            Assert.Equal(4, result.Value.Value.AssociatedMemberCount);
        }

        [Fact]
        public async Task HandlerSuccessWithValidationFailure()
        {
            CampaignDetailsRetrieveQuery query = new() { CampaignId = null, CampaignCode = null };

            var result = await _handler.Handle(query, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Errors);
        }


        [Fact]
        public async Task HandlerFailureWithIncorrectCampaignCode()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CampaignSearchQuery>(), CancellationToken.None)).ReturnsAsync(new Response<CampaignSearchResponse>() { Status = Status.OK, Value = null });

            CampaignDetailsRetrieveQuery query = new() { CampaignId = null, CampaignCode = "VIRGIN007" };

            var result = await _handler.Handle(query, CancellationToken.None);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task HandlerFailureWithException()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CampaignSearchQuery>(), CancellationToken.None)).Throws(new Exception());

            CampaignDetailsRetrieveQuery query = new() { CampaignId = null, CampaignCode = "VIRGIN007" };

            var result = await _handler.Handle(query, CancellationToken.None);
            Assert.True(result.IsFailure);
        }
    }
}
