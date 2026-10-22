using Microsoft.Azure.Functions.Worker;
using Moq;
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.PromotionCampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class GetPromotionCommandHandlerTest
    {
        private readonly Mock<ICommonRefDBRepository> _mockCommonRefDBRepository;
        private readonly GetPromotionCommandHandler _handler;
        private readonly Mock<FunctionContext> _context;

        public GetPromotionCommandHandlerTest()
        {
            _mockCommonRefDBRepository = new Mock<ICommonRefDBRepository>();
            _context = new Mock<FunctionContext>();
            _handler = new GetPromotionCommandHandler(_mockCommonRefDBRepository.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new GetPromotionCommandHandler(null));
        }

        [Fact]
        public void HandlerSuccessWithNoError()
        {
            var req = TestDataProvider.GetTestData<GetPromotionCommand>("CampaignManagementMS\\PromotionSearchRequest");
            var promotionSearchResponse = TestDataProvider.GetTestData<PromotionSearchResponse>("CampaignManagementMS\\PromotionSearchResponse");
            _mockCommonRefDBRepository.Setup(x => x.GetPromotionSearch("1-19J90XO", null, null, null, null, null, null, null)).ReturnsAsync(new Response<PromotionSearchResponse> { Status = Status.OK, Value = promotionSearchResponse });
            var result = _handler.Handle(req, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
        }
    }
}
