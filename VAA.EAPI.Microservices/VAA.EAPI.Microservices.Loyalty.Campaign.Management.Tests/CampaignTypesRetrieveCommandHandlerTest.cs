using Microsoft.Azure.Functions.Worker;
using Moq;
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignTypeRetrieval;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.CampaignTypeRetrieval;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.PromotionCampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignTypesRetrieveCommandHandlerTest
    {
        private readonly Mock<ICommonRefDBRepository> _mockCommonRefDBRepository;
        private readonly CampaignTypesRetrieveCommandHandler _handler;
        private readonly Mock<FunctionContext> _context;

        public CampaignTypesRetrieveCommandHandlerTest()
        {
            _mockCommonRefDBRepository = new Mock<ICommonRefDBRepository>();
            _context = new Mock<FunctionContext>();
            _handler = new CampaignTypesRetrieveCommandHandler(_mockCommonRefDBRepository.Object);
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
            CampaignTypesRetrieveCommand req = null;
            var campaignTypeResponse = TestDataProvider.GetTestData<CampaignTypeResponse>("CampaignManagementMS\\CampaignTypeResponse");
            _mockCommonRefDBRepository.Setup(x => x.GetCampaignTypes()).ReturnsAsync(new Response<CampaignTypeResponse> { Status = Status.OK, Value = campaignTypeResponse });
            var result = _handler.Handle(req, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
        }
    }
}
