using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;
using VAA.EAPI.NDC.Offer.Validations.OfferPrice;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OfferPrice_Validations
{
    public class ValidateAgentV2Tests
    {
        [Fact]
        public void ValidateAgentV2_DirectError1_Request()
        {
            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOffer\\ValidationResponse");

            Participant participant1 = new Participant() { AgencyID = "" };

            var airlineOffers = new OfferResponse() { Parties = new() { Participants = new List<Participant> { participant1 } } };
            var validationErrors = ValidateOfferPrice.ValidateAgentV2(airlineOffers, validationRS);

            Assert.True(validationErrors.Any());
            Assert.Equal("368_1", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void ValidateAgentV2_DirectError2_Request()
        {
            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOffer\\ValidationResponse");

            Participant participant1 = new Participant() { AgencyID = "" };
            Participant participant2 = new Participant() { AgencyID = "abc" };

            var airlineOffers = new OfferResponse() { Parties = new() { Participants = new List<Participant> { participant1, participant2 } } };
            var validationErrors = ValidateOfferPrice.ValidateAgentV2(airlineOffers, validationRS);

            Assert.True(validationErrors.Any());
            Assert.Equal("368_1", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void ValidateAgentV2_NotDirectError1_Request()
        {
            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOffer\\ValidationResponse");

            validationRS.ValidateResponse.KeyType = "INDIRECT";

            Participant participant1 = new Participant() { AgencyID = "" };

            var airlineOffers = new OfferResponse() { Parties = new() { Participants = new List<Participant> { participant1 } } };
            var validationErrors = ValidateOfferPrice.ValidateAgentV2(airlineOffers, validationRS);

            Assert.True(validationErrors.Any());
            Assert.Equal("368_1", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void ValidateAgentV2_NotDirectError2_Request()
        {
            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOffer\\ValidationResponse");

            validationRS.ValidateResponse.KeyType = "INDIRECT";

            Participant participant1 = new Participant() { AgencyID = "" };
            Participant participant2 = new Participant() { AgencyID = "abc" };

            var airlineOffers = new OfferResponse() { Parties = new() { Participants = new List<Participant> { participant1, participant2 } } };
            var validationErrors = ValidateOfferPrice.ValidateAgentV2(airlineOffers, validationRS);

            Assert.True(validationErrors.Any());
            Assert.Equal("368_1", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void ValidateAgentV2_NoError_Request()
        {
            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOffer\\ValidationResponse");

            validationRS.ValidateResponse.KeyType = "INDIRECT";

            Participant participant1 = new Participant() { AgencyID = "91281035" };
            Participant participant2 = new Participant() { AgencyID = "91281035" };

            var airlineOffers = new OfferResponse() { Parties = new() { Participants = new List<Participant> { participant1, participant2 } } };
            var validationErrors = ValidateOfferPrice.ValidateAgentV2(airlineOffers, validationRS);

            Assert.True(validationErrors.Count == 0);
        }

        [Fact]
        public void ValidateAgentV2_SellerError_Request()
        {
            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOffer\\ValidationResponse");

            validationRS.ValidateResponse.KeyType = "INDIRECT";

            Participant participant1 = new Participant() { AgencyID = "", PartyType = Participant.PartyTypeEnum.Seller };
            Participant participant2 = new Participant() { AgencyID = "abc", PartyType = Participant.PartyTypeEnum.Seller };

            var airlineOffers = new OfferResponse() { Parties = new() { Participants = new List<Participant> { participant1, participant2 } } };
            var validationErrors = ValidateOfferPrice.ValidateAgentV2(airlineOffers, validationRS);

            Assert.True(validationErrors.Any());
            Assert.Equal("368_1", validationErrors.First().ErrorCode);

        }
    }
}
