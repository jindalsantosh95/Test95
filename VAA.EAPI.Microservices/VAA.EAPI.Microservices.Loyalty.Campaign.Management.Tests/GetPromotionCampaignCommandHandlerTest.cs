using Microsoft.Azure.Functions.Worker;
using Moq;
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionCampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.PromotionSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class GetPromotionCampaignCommandHandlerTest
    {
        private readonly Mock<ICommonRefDBRepository> _mockCommonRefDBRepository;
        private readonly GetPromotionCampaignCommandHandler _handler;
        private readonly Mock<FunctionContext> _context;

        public GetPromotionCampaignCommandHandlerTest()
        {
            _mockCommonRefDBRepository = new Mock<ICommonRefDBRepository>();
            _context = new Mock<FunctionContext>();
            _handler = new GetPromotionCampaignCommandHandler(_mockCommonRefDBRepository.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new GetPromotionCampaignCommandHandler(null));
        }

        [Fact]
        public void HandlerSuccessWithNoError()
        {
            var req = TestDataProvider.GetTestData<GetPromotionCampaignCommand>("CampaignManagementMS\\PromotionSearchRequest");
            var promoCampSearchResponse = TestDataProvider.GetTestData<PromotionCampaignSearchResponse>("CampaignManagementMS\\PromotionCampaignSearchResponse");
            _mockCommonRefDBRepository.Setup(x => x.GetPromotionCampaignSearch("1-19J90XO")).ReturnsAsync(new Response<PromotionCampaignSearchResponse> { Status = Status.OK, Value = promoCampSearchResponse });
            var result = _handler.Handle(req, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
        }
    }
}
