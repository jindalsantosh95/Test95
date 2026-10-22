using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignsDownload;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignsDownloadQueryHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ICampaignListDownloadService> _campaignListDownloadService;
        private readonly Mock<ILogger<CampaignsDownloadQueryHandler>> _mockLogger;
        private readonly CampaignsDownloadQueryHandler _handler;

        public CampaignsDownloadQueryHandlerTest()
        {
            _mockLogger = new Mock<ILogger<CampaignsDownloadQueryHandler>>();
            _mediator = new Mock<IMediator>();
            _campaignListDownloadService = new Mock<ICampaignListDownloadService>();
            _handler = new CampaignsDownloadQueryHandler(_mediator.Object, _campaignListDownloadService.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignsDownloadQueryHandler(_mediator.Object, _campaignListDownloadService.Object, null));

            Assert.Throws<ArgumentNullException>(() =>
            new CampaignsDownloadQueryHandler(null, _campaignListDownloadService.Object, _mockLogger.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new CampaignsDownloadQueryHandler(_mediator.Object, null, _mockLogger.Object));
        }

        [Fact]
        public async Task HandlerShouldSuccess()
        {
            var response = new CampaignsDownloadResponse() { CampaignsFileURL = "dummyurl" };
            CampaignSearchResponse campaignSearchResponse = TestDataProvider.GetTestData<CampaignSearchResponse>("CampaignManagementMS\\CampaignSearchResponse");

            _mediator.Setup(m => m.Send(It.IsAny<CampaignSearchQuery>(), CancellationToken.None)).ReturnsAsync(new Response<CampaignSearchResponse>() { Status = Status.OK, Value = campaignSearchResponse });
            _campaignListDownloadService.Setup(m => m.CreateXlStatementAndUploadToBlob(campaignSearchResponse)).ReturnsAsync("dummyurl");

            CampaignsDownloadQuery query = new() { };
            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
        }

        [Fact]
        public async Task HandlerShouldFailWhenNoDataFound()
        {
            _mediator.Setup(m => m.Send(It.IsAny<CampaignSearchQuery>(), CancellationToken.None)).ReturnsAsync(new Response<CampaignSearchResponse>() { Status = Status.OK, Value = null });

            CampaignsDownloadQuery query = new() { };
            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.NotNull(result.Error);
        }


        [Fact]
        public async Task HandlerShouldFailWhenExceptionOccurs()
        {
            _mediator.Setup(m => m.Send(It.IsAny<CampaignSearchQuery>(), CancellationToken.None)).Throws(new Exception());

            CampaignsDownloadQuery query = new() { };
            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Errors);
        }
    }
}
