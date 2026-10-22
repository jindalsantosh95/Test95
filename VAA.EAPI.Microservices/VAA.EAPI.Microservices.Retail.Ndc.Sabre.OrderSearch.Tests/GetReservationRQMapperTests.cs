using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Infrastructure.Mapper;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Tests
{
    public class GetReservationRQMapperTests
    {
        public GetReservationRQMapperTests()
        {
            ConfigReader.Set("ConversationId", "ConversationId");
            ConfigReader.Set("FromPartyId", "FromPartyId");
            ConfigReader.Set("ToPartyId", "ToPartyId");
        }

        [Fact]
        public void GetReservationRQMapper_Returns_PositiveResponse()
        {
            //Arrange
            string token = "Shared/IDL:IceSess\\/SessMgr:1\\.0.IDL/Common/!ICESMS\\/ACPCRTD!ICESMSLB\\/CRT.LB!1736491182880!4762!545";
            var OrderSearchRQ = TestDataProvider.GetTestData<OrderSearchRequest>("OrderSearchMS\\OrderSearchRQ");

            //Act
            var result = GetReservationRQMapper.GetReservationRequest(token, OrderSearchRQ);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Security);
            Assert.NotNull(result.GetReservationRQ);
            Assert.NotNull(result.MessageHeader);
            Assert.Equal("Stateful",result.GetReservationRQ.RequestType);
            Assert.Equal("FromPartyId",result.MessageHeader.From.PartyId.First().Value);
            Assert.Equal(token, result.Security.BinarySecurityToken);
        }
    }
}
