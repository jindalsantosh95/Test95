using System;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.MemberProfileUpdate;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class MemberProfileUpdateMapperTest
    {
        [Fact]
        public void MemberProfileUpdateMapperTest_ThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            MemberProfileUpdateMapper.MaptoMemberProfileEventData(null));
        }

        [Fact]
        public void MemberProfileUpdateMapperTest_SuccessInstantSilver()
        {
            var request = TestDataProvider.GetTestData<CampaignRegisterEventData>("CampaignManagementMS\\ProfileUpdateEventDataInstantSilver");
            var eventData = MemberProfileUpdateMapper.MaptoMemberProfileEventData(request);
            Assert.Equal("1-2423PY1", eventData.FlyingClubMemberDocument.InstantSilverCampaignId);
            Assert.Equal("c220a7fb-8b85-4f4f-bac8-db94252a3e45", eventData.CustomerId);
        }

        [Fact]
        public void MemberProfileUpdateMapperTest_SuccessInstantGold()
        {
            var request = TestDataProvider.GetTestData<CampaignRegisterEventData>("CampaignManagementMS\\ProfileUpdateEventDataInstantGold");
            var eventData = MemberProfileUpdateMapper.MaptoMemberProfileEventData(request);
            Assert.Equal("1-450HX6X", eventData.FlyingClubMemberDocument.VAInstantGoldCampaignId);
            Assert.Equal("c220a7fb-8b85-4f4f-bac8-db94252a3e45", eventData.CustomerId);
        }

        [Fact]
        public void MemberProfileUpdateMapperTest_SuccessFTG()
        {
            var request = TestDataProvider.GetTestData<CampaignRegisterEventData>("CampaignManagementMS\\ProfileUpdateEventDataFTG");
            var eventData = MemberProfileUpdateMapper.MaptoMemberProfileEventData(request);
            Assert.Equal("1-3W-207", eventData.FlyingClubMemberDocument.FastTrackId);
            Assert.Equal("c220a7fb-8b85-4f4f-bac8-db94252a3e45", eventData.CustomerId);
        }

        [Fact]
        public void MemberProfileUpdateMapperTest_SuccessS2SFTG()
        {
            var request = TestDataProvider.GetTestData<CampaignRegisterEventData>("CampaignManagementMS\\ProfileUpdateEventDataS2SFTG");
            var eventData = MemberProfileUpdateMapper.MaptoMemberProfileEventData(request);
            foreach (var attrib in eventData.FlyingClubMemberDocument.MemberAttributes)
            {
                Assert.Equal("S2SFTGFlag", attrib.AttributeName);
                Assert.Equal("Y", attrib.Value);
            }
            Assert.Equal("1-42-726", eventData.FlyingClubMemberDocument.FastTrackId);
            Assert.Equal("c220a7fb-8b85-4f4f-bac8-db94252a3e45", eventData.CustomerId);
        }

        [Fact]
        public void MemberProfileUpdateMapperTest_SuccessInstantSilverFTG()
        {
            var request = TestDataProvider.GetTestData<CampaignRegisterEventData>("CampaignManagementMS\\ProfileUpdateEventDataInstantSilverFTG");
            var eventData = MemberProfileUpdateMapper.MaptoMemberProfileEventData(request);
            Assert.Equal("1-38N2-2", eventData.FlyingClubMemberDocument.FastTrackId);
            Assert.Equal("1-38N2-2", eventData.FlyingClubMemberDocument.InstantSilverCampaignId);
            Assert.Equal("c220a7fb-8b85-4f4f-bac8-db94252a3e45", eventData.CustomerId);
        }

        [Fact]
        public void MemberProfileUpdateMapperTest_SuccessS2G()
        {
            var request = TestDataProvider.GetTestData<CampaignRegisterEventData>("CampaignManagementMS\\ProfileUpdateEventDataS2G");
            var eventData = MemberProfileUpdateMapper.MaptoMemberProfileEventData(request);
            foreach (var attrib in eventData.FlyingClubMemberDocument.MemberAttributes)
            {
                Assert.Equal("S2GFlag", attrib.AttributeName);
                Assert.Equal("Y", attrib.Value);
            }
            Assert.Equal("c220a7fb-8b85-4f4f-bac8-db94252a3e45", eventData.CustomerId);
        }

        [Fact]
        public void MemberProfileUpdateMapperTest_SuccessS2S()
        {
            var request = TestDataProvider.GetTestData<CampaignRegisterEventData>("CampaignManagementMS\\ProfileUpdateEventDataS2S");
            var eventData = MemberProfileUpdateMapper.MaptoMemberProfileEventData(request);
            foreach (var attrib in eventData.FlyingClubMemberDocument.MemberAttributes)
            {
                Assert.Equal("S2SFlag", attrib.AttributeName);
                Assert.Equal("Y", attrib.Value);
            }
            Assert.Equal("c220a7fb-8b85-4f4f-bac8-db94252a3e45", eventData.CustomerId);
        }

    }
}
