using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRQ;
using VAA.EAPI.NDC.Offer.Validations.OrderReshop;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OrderReshop
{
    public class ValidateOrderReshopRQDistributionEntity
    {
        [Fact]
        public void ValidateAgentInRQ_ErrorWithNullPartyType_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OrderReshopRQ>("NDCOffer\\OrderReshop\\OrderReshopRQ");
            var ordinalValue=request.DistributionChain[0].Ordinal= "1";
            Participant participantRequest= new Participant()  { PartyType=null} ;

            var orderReshopRequest= new OrderReshopRequest() { Parties = new() { Participants =new List<Participant> { participantRequest } } };
            var validationErrors = ValidateOrderReshop.ValidateOrderReshopRQDistributionEntity(orderReshopRequest);

            Assert.True(validationErrors.Any());
            Assert.Equal("911_8", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void ValidateAgentInRQ_ErrorWithCharOrdinal_Request()
        {
            Participant participantRequest = new Participant() { Ordinal = "A" };
            var orderReshopRequest = new OrderReshopRequest() { Parties = new() { Participants = new List<Participant> { participantRequest } } };
            var validationErrors = ValidateOrderReshop.ValidateOrderReshopRQDistributionEntity(orderReshopRequest);

            Assert.True(validationErrors.Any());
            Assert.Equal("911_8", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void ValidateAgentInRQ_ErrorWithEmptyOrdinal_Request()
        {
            Participant participantRequest = new Participant() { Ordinal = " " };
            var orderReshopRequest = new OrderReshopRequest() { Parties = new() { Participants = new List<Participant> { participantRequest } } };
            var validationErrors = ValidateOrderReshop.ValidateOrderReshopRQDistributionEntity(orderReshopRequest);

            Assert.True(validationErrors.Any());
            Assert.Equal("911_8", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void ValidateAgentInRQ_ErrorWithNullAgencyID_Request()
        {
            Participant participantRequest = new Participant() { AgencyID = null };
            var orderReshopRequest = new OrderReshopRequest() { Parties = new() { Participants = new List<Participant> { participantRequest } } };
            var validationErrors = ValidateOrderReshop.ValidateOrderReshopRQDistributionEntity(orderReshopRequest);

            Assert.True(validationErrors.Any());
            Assert.Equal("911_8", validationErrors.First().ErrorCode);

        }


        [Fact]
        public void ValidateAgentInRQ_ErrorWithEmptyAgencyID_Request()
        {
            Participant participantRequest = new Participant() { AgencyID = " " };
            var orderReshopRequest = new OrderReshopRequest() { Parties = new() { Participants = new List<Participant> { participantRequest } } };
            var validationErrors = ValidateOrderReshop.ValidateOrderReshopRQDistributionEntity(orderReshopRequest);

            Assert.True(validationErrors.Any());
            Assert.Equal("911_8", validationErrors.First().ErrorCode);

        }
    }
}
