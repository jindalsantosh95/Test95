using VAA.EAPI.Microservices.Loyalty.Sync.LoyaltyProfileSyncServiceRef;
using Shouldly;
using VAA.EAPI.Microservices.Loyalty.Sync.Application.Command;
using VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Repository.Loyalty.SyncRepository;
using VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Services.Loyalty.SyncService;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Tests
{
    public class LoyaltySyncMapperTests
    {
        public LoyaltySyncMapperTests()
        {
            LoyaltySyncTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void LoyaltySyncMapperSuccess()
        {
            var syncRequest = TestDataProvider.GetTestData<LoyaltySyncEventProcessCommand>("LoyaltySync.SuccessMapperRequest");
            LoyaltySyncRepoCommand repoCommand = new(syncRequest) { LoyaltySyncQuery = syncRequest };
            LoyaltySyncServiceCommand syncServiceCommand = new(repoCommand) { LoyaltySyncRepoQuery = repoCommand };
            AddRequestType memberSyncInput = LoyaltySyncRequestMapper.ToOcdbSyncRequest(syncServiceCommand);
            memberSyncInput.NameInfo.FirstName.ShouldBe("testVRedEAPI");
            memberSyncInput.NameInfo.LastName.ShouldBe("LastvRedEAPI");
        }

        [Fact]
        public void LoyaltySyncMapperFailure()
        {
            var syncRequest = TestDataProvider.GetTestData<LoyaltySyncEventProcessCommand>("LoyaltySyncFailureMapperRequest");
            LoyaltySyncRepoCommand repoCommand = new LoyaltySyncRepoCommand(syncRequest) { LoyaltySyncQuery = syncRequest };
            LoyaltySyncServiceCommand syncServiceCommand = new(repoCommand) { LoyaltySyncRepoQuery = repoCommand };
            AddRequestType memberSyncInput = LoyaltySyncRequestMapper.ToOcdbSyncRequest(syncServiceCommand);
            memberSyncInput.NameInfo.FirstName.ShouldNotBe("testVRedEAPI");
            memberSyncInput.NameInfo.LastName.ShouldNotBe("LastvRedEAPI");
        }
    }
}
