using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.PromotionCampaignSearch;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class PromotionSearchResponseQueryHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<PromotionSearchResponseQueryHandler>> _mockLogger;
        private readonly PromotionSearchResponseQueryHandler _handler;

        public PromotionSearchResponseQueryHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<PromotionSearchResponseQueryHandler>>();
            _handler = new PromotionSearchResponseQueryHandler(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
             new PromotionSearchResponseQueryHandler(_mockMediator.Object, null));

            Assert.Throws<ArgumentNullException>(() =>
             new PromotionSearchResponseQueryHandler(null, _mockLogger.Object));
        }

        [Fact]
        public async Task HandlerSuccessWithNoError()
        {
            PromotionSearchQuery req = TestDataProvider.GetTestData<PromotionSearchQuery>("CampaignManagementMS\\PromotionSearchRequest");
            var promotionSearchResponse = TestDataProvider.GetTestData<PromotionSearchResponse>("CampaignManagementMS\\PromotionSearchResponse");
            _mockMediator.Setup(m => m.Send(It.IsAny<GetPromotionCommand>(), CancellationToken.None)).ReturnsAsync(new Response<PromotionSearchResponse> { Status = Status.OK, Value = promotionSearchResponse });
            PromotionSearchQuery query = new() { PromotionId = "1-19J90XO", PromotionName = null, StartDateF = null, StartDateT = null, EndDateF = null, EndDateT = null, PartnerName = null, Active = null };
            var result = await _handler.Handle(req, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);
        }
    }
}
