using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignSearchResponseQueryHandlerTest
    {
        private readonly Mock<ICommonRefDBRepository> _mockCommonRefDBRepository;
        private readonly Mock<ILogger<CampaignSearchResponseQueryHandler>> _mockLogger;
        private readonly CampaignSearchResponseQueryHandler _handler;

        public CampaignSearchResponseQueryHandlerTest()
        {
            _mockCommonRefDBRepository = new Mock<ICommonRefDBRepository>();
            _mockLogger = new Mock<ILogger<CampaignSearchResponseQueryHandler>>();
            _handler = new CampaignSearchResponseQueryHandler(_mockCommonRefDBRepository.Object, _mockLogger.Object);

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
             new CampaignSearchResponseQueryHandler(_mockCommonRefDBRepository.Object, null));

            Assert.Throws<ArgumentNullException>(() =>
             new CampaignSearchResponseQueryHandler(null, _mockLogger.Object));
        }

        [Fact]
        public async Task HandlerSuccessWithNoError()
        {
            CampaignSearchResponse campaignSearchResponse = TestDataProvider.GetTestData<CampaignSearchResponse>("CampaignManagementMS\\CampaignSearchResponse");

            _mockCommonRefDBRepository.Setup(x => x.SearchCampaigns(null, "VIRGIN007", null, null, null, null, null, null, null)).ReturnsAsync(new Response<CampaignSearchResponse> { Status = Status.OK, Value = campaignSearchResponse });
            CampaignSearchQuery query = new() { CampaignId = null, CampaignName = "VIRGIN007", StartDateF = null, StartDateT = null, EndDateF = null, EndDateT = null, CampaignCode = null, Status = null, Type = null };

            var result = await _handler.Handle(query, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);
        }

        [Fact]
        public async Task HandlerFailureWithError()
        {
            _mockCommonRefDBRepository.Setup(x => x.SearchCampaigns(null, "VIRGIN007", null, null, null, null, null, null, null)).Throws(new Exception());
            CampaignSearchQuery query = new() { CampaignId = null, CampaignName = "VIRGIN007", StartDateF = null, StartDateT = null, EndDateF = null, EndDateT = null, CampaignCode = null, Status = null, Type = null };

            var result = await _handler.Handle(query, CancellationToken.None);
            Assert.True(result.IsFailure);
        }
    }
}
