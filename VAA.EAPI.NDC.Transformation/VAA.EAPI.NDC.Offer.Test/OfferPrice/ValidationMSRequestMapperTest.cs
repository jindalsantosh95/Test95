using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.NDC.Offer.Infrastructure.Mapper.ValidationMS;
using Xunit;

namespace VAA.EAPI.NDC.Offer.Tests.OfferPrice
{
    public class ValidationMSRequestMapperTests
    {

        public ValidationMSRequestMapperTests()
        {
            ConfigReader.Set("checkagenttktauthority", "true");
        }


        [Fact]
        public void MapValidationRequest_ShouldReturnCorrectValidationRequest_WhenCheckTktAuthorityIsTrue()
        {
            var parties = new Parties
            {
                Participants = new List<Participant>
                {
                    new Participant { AgencyID = "91269021", Name = "go4travel", PartyType = Participant.PartyTypeEnum.Retail }
                }
            };

            var result = ValidationMSRequestMapper.MapValidationRequest(parties, checkTktAuthority: true);

            Assert.NotNull(result);
            Assert.Equal("91269021", result.Parties.Participants.First().AgencyID);
            Assert.Equal("go4travel", result.Parties.Participants.First().Name);
            Assert.Equal("Retail", result.Parties.Participants.First().PartyType.ToString());
        }


        [Fact]
        public void GetParticipants_ShouldReturnSingleParticipant_WhenOneParticipantInRequest()
        {
            var parties = new Parties
            {
                Participants = new List<Participant>
                {
                    new Participant { AgencyID = "91269021", Name = "go4travel", PartyType = Participant.PartyTypeEnum.Retail }
                }
            };

            var participants = ValidationMSRequestMapper.GetParticipants(parties);

            Assert.NotNull(participants);
            Assert.Equal("91269021", participants.First().AgencyID);
        }

        [Fact]
        public void GetSeatParties_ShouldReturnCorrectParties_WhenDistributionChainsAreProvided()
        {
            var distributionChains = new[]
            {
                new Domain.RequestResponseModels.SeatAvailabilityRQ.DistributionChainLinkType
                {
                    Ordinal = "1",
                    OrgRole = Domain.RequestResponseModels.SeatAvailabilityRQ.DistributionChainRoleCodeType.Seller,
                    ParticipatingOrg = new Domain.RequestResponseModels.SeatAvailabilityRQ.ParticipatingOrgType
                    {
                        OrgID = "91269021",
                        Name = "go4travel"

                    }
                }
            };

            var corporateAccount = new CorporateAccount { };
            var parties = ValidationMSRequestMapper.GetSeatParties(distributionChains, corporateAccount);

            Assert.NotNull(parties);
            Assert.Equal("91269021", parties.Participants.First().AgencyID);
        }

        [Fact]
        public void MapValidationRQQuote_ShouldReturnValidationRQ_WithCorrectParticipantsAndCorporateAccount()
        {
            var parties = new Parties
            {
                Participants = new List<Participant>
                {
                    new Participant { AgencyID = "91269021", Name = "go4travel", PartyType = Participant.PartyTypeEnum.Retail }
                }
            };

            var corporateAccount = new CorporateAccount { };
            var validationRequest = ValidationMSRequestMapper.MapValidationRQQuote(parties, corporateAccount, checkTktAuthority: true);

            Assert.NotNull(validationRequest);
            Assert.Equal("DIRECT", validationRequest.Keytype);
            Assert.Equal("91269021", validationRequest.Parties.Participants.First().AgencyID);
        }
    }
}
