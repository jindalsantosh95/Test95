using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRetrieval;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CampaignRetrieve;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignRetrieveRepositoryTest
    {
        private readonly Mock<ITargetedMemberDocumentService> _targetedMemberDocumentService;
        private readonly Mock<ILogger<CampaignRetrieveRepository>> _logger;
        private readonly Mock<ICommonRefDBRepository> _commonRefDBRepository;
        private readonly CampaignRetrieveRepository _campaignRetrieveRepository;

        public CampaignRetrieveRepositoryTest()
        {
            _targetedMemberDocumentService = new Mock<ITargetedMemberDocumentService>();
            _logger = new Mock<ILogger<CampaignRetrieveRepository>>();
            _commonRefDBRepository = new Mock<ICommonRefDBRepository>();
            _campaignRetrieveRepository = new CampaignRetrieveRepository(_commonRefDBRepository.Object, _targetedMemberDocumentService.Object, _logger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignRetrieveRepository(null, _targetedMemberDocumentService.Object, _logger.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new CampaignRetrieveRepository(_commonRefDBRepository.Object, null, _logger.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new CampaignRetrieveRepository(_commonRefDBRepository.Object, _targetedMemberDocumentService.Object, null));
        }

        [Fact]
        public void GetMemberCampaignsShouldSucceed()
        {
            var memberCampaignData = TestDataProvider.GetTestData<TargetedMembershipResponse>("CampaignManagementMS\\TargetedMembershipDocResponse");
            var campaignData = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            _targetedMemberDocumentService.Setup(x => x.GetMemberCampaigns(It.IsAny<string>())).ReturnsAsync(memberCampaignData);
            _commonRefDBRepository.Setup(x => x.GetCampaignDetails(It.IsAny<string>())).ReturnsAsync(campaignData);
            var result = _campaignRetrieveRepository.GetMemberCampaigns("1-XXX");
            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
        }

        [Fact]
        public void GetMemberCampaignsShouldFail()
        {
            TargetedMembershipResponse memberCampaignData = null;
            var campaignData = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            _targetedMemberDocumentService.Setup(x => x.GetMemberCampaigns(It.IsAny<string>())).ReturnsAsync(memberCampaignData);
            _commonRefDBRepository.Setup(x => x.GetCampaignDetails(It.IsAny<string>())).ReturnsAsync(campaignData);
            var result = _campaignRetrieveRepository.GetMemberCampaigns("1-XXX");
            Assert.True(result.Result.IsFailure);
        }

    }
}
