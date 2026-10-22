using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionCampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.PromotionSearch;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class PromotionCampaignSearchQueryHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<PromotionCampaignSearchQueryHandler>> _mockLogger;
        private readonly Mock<IValidator<PromotionCampaignSearchRequestData>> _mockValidator;
        private readonly PromotionCampaignSearchQueryHandler _handler;

        public PromotionCampaignSearchQueryHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<PromotionCampaignSearchQueryHandler>>();
            _mockValidator = new Mock<IValidator<PromotionCampaignSearchRequestData>>();
            _handler = new PromotionCampaignSearchQueryHandler(_mockMediator.Object, _mockLogger.Object, _mockValidator.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
             new PromotionCampaignSearchQueryHandler(_mockMediator.Object, _mockLogger.Object, null));

            Assert.Throws<ArgumentNullException>(() =>
             new PromotionCampaignSearchQueryHandler(_mockMediator.Object, null, _mockValidator.Object));

            Assert.Throws<ArgumentNullException>(() =>
             new PromotionCampaignSearchQueryHandler(null, _mockLogger.Object, _mockValidator.Object));
        }

        [Fact]
        public async Task HandlerSuccessWithNoError()
        {
            PromotionCampaignSearchQuery req = TestDataProvider.GetTestData<PromotionCampaignSearchQuery>("CampaignManagementMS\\PromotionSearchRequest");
            _mockValidator.Setup(v => v.Validate(req).IsValid).Returns(true);
            var promoCampResponse = TestDataProvider.GetTestData<PromotionCampaignSearchResponse>("CampaignManagementMS\\PromotionCampaignSearchResponse");
            _mockMediator.Setup(m => m.Send(It.IsAny<GetPromotionCampaignCommand>(), CancellationToken.None)).ReturnsAsync(new Response<PromotionCampaignSearchResponse> { Status = Status.OK, Value = promoCampResponse });
            PromotionSearchQuery query = new() { PromotionId = "1-19J90XO" };
            var result = await _handler.Handle(req, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);
        }
    }
}
