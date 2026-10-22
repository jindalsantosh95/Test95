using System.Linq;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.SSRToPNR;
using Xunit;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SSRToPNR;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class FCSTampingDetailMapperTest
    {
        public FCSTampingDetailMapperTest()
        {
            MemberTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void FCSTampingDataMapper_Failure()
        {
            var request = TestDataProvider.GetTestData<AddFCNumberToPNRCommand>("MemberManagement\\MemberDetailsToAddSSRList");
            var addSSREvent = FCSTampingDetailMapper.FCSTampingDataMapper(request.MemberDeatilsToAddSSR.FirstOrDefault(), request.FileName,MemberDetailsToAddSSR.Statuses.Processed, new AddFCNumberToPNRQueryResponse());
            Assert.NotEqual("Failed", addSSREvent.Status);
        }
    }
}
