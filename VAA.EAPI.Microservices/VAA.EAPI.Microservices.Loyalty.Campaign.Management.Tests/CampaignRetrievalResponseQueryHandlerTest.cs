using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRetrieval;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CampaignRetrieve.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignRetrievalResponseQueryHandlerTest
    {
        private readonly Mock<ILogger<MemberCampaignRetrievalResponseQueryHandler>> _logger;
        private readonly Mock<ICampaignRetrieveRepository> _campaignRetrieveRepository;
        private readonly MemberCampaignRetrievalResponseQueryHandler _handler;
        public CampaignRetrievalResponseQueryHandlerTest()
        {
            _logger = new Mock<ILogger<MemberCampaignRetrievalResponseQueryHandler>>();
            _campaignRetrieveRepository = new Mock<ICampaignRetrieveRepository>();
            _handler = new MemberCampaignRetrievalResponseQueryHandler(_campaignRetrieveRepository.Object, _logger.Object);
            Environment.SetEnvironmentVariable(Constants.Environment.ResponseDelayTimeInMS, "0");
            Environment.SetEnvironmentVariable(Constants.Environment.ResponsePollingCount, "1");
            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new MemberCampaignRetrievalResponseQueryHandler(_campaignRetrieveRepository.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
             new MemberCampaignRetrievalResponseQueryHandler(null, _logger.Object));
        }

        [Fact]
        public void Handler_Success_With_NoError()
        {
            MemberCampaignRetrievalQuery campaignQueryData = TestDataProvider.GetTestData<MemberCampaignRetrievalQuery>("CampaignManagementMS\\CampaignRetrievalResponseQuery");
            var memberCampaignData = TestDataProvider.GetTestData<MemberCampaignResponse>("CampaignManagementMS\\MemberCampaignList");
            _campaignRetrieveRepository.Setup(x => x.GetMemberCampaigns(It.IsAny<string>())).ReturnsAsync(new Response<MemberCampaignResponse> { Status = Status.OK, Value = memberCampaignData });
            var result = _handler.Handle(campaignQueryData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
        }
        [Fact]
        public void Handler_Faliure_With_Error()
        {
            MemberCampaignRetrievalQuery campaignQueryData = TestDataProvider.GetTestData<MemberCampaignRetrievalQuery>("CampaignManagementMS\\CampaignRetrievalResponseQueryInvalid");
            var memberCampaignData = TestDataProvider.GetTestData<MemberCampaignResponse>("CampaignManagementMS\\MemberCampaignList");
            _campaignRetrieveRepository.Setup(x => x.GetMemberCampaigns(It.IsAny<string>())).ReturnsAsync(new Response<MemberCampaignResponse> { Status = Status.OK, Value = memberCampaignData });
            var result = _handler.Handle(campaignQueryData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }

        [Fact]
        public void HandlerFaliureWithException()
        {
            MemberCampaignRetrievalQuery campaignQueryData = TestDataProvider.GetTestData<MemberCampaignRetrievalQuery>("CampaignManagementMS\\CampaignRetrievalResponseQuery");
            _campaignRetrieveRepository.Setup(x => x.GetMemberCampaigns(It.IsAny<string>())).Throws(new Exception());
            var result = _handler.Handle(campaignQueryData, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }
    }
}

