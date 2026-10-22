using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Tests.SabreCommandLLS
{
    public class SabreCommandLLSRequestMapperTests
    {
        public SabreCommandLLSRequestMapperTests()
        {
            ConfigReader.Set("ConversationId", "ConversationId");
            ConfigReader.Set("FromPartyId", "FromPartyId");
            ConfigReader.Set("ToPartyId", "ToPartyId");
        }

        [Fact]
        public void SabreCommandLLSRequestMapper_Returns_PositiveResponse()
        {
            //Arrange
            string token = "Shared/IDL:IceSess\\/SessMgr:1\\.0.IDL/Common/!ICESMS\\/ACPCRTD!ICESMSLB\\/CRT.LB!1736491182880!4762!545";
            string hostcommand = "WFR9325831274950¥AVS¥N1.1¥PQ2";

            //Act
            var result = SabreCommandLLSRequestMapper.GetSabreCommandRequest(token, hostcommand);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Security);
            Assert.NotNull(result.MessageHeader);
            Assert.Equal("SabreCommandLLSRQ", result.MessageHeader.Action);
            Assert.Equal("ConversationId", result.MessageHeader.ConversationId);
            Assert.Equal("FromPartyId", result.MessageHeader.From.PartyId.First().Value);
            Assert.Equal("ToPartyId", result.MessageHeader.To.PartyId.First().Value);
            Assert.Equal(token, result.Security.BinarySecurityToken);
            Assert.Equal("WFR9325831274950¥AVS¥N1.1¥PQ2", result.SabreCommandLLSRQ.Request.HostCommand);
        }
    }
}
