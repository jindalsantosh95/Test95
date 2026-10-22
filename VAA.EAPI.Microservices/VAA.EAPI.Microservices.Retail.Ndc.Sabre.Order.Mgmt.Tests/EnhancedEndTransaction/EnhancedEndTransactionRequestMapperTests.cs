using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Tests.EnhancedEndTransaction
{
    public class EnhancedEndTransactionRequestMapperTests
    {
        public EnhancedEndTransactionRequestMapperTests()
        {
            ConfigReader.Set("ConversationId", "ConversationId");
            ConfigReader.Set("FromPartyId", "FromPartyId");
            ConfigReader.Set("ToPartyId", "ToPartyId");
        }

        [Fact]
        public void GetEnhancedEndTransactionRQMapper_Returns_PositiveResponse()
        {
            //Arrange
            string token = "Shared/IDL:IceSess\\/SessMgr:1\\.0.IDL/Common/!ICESMS\\/ACPCRTD!ICESMSLB\\/CRT.LB!1736491182880!4762!545";

            //Act
            var result = EnhancedEndTransactionRequestMapper.GetEnhancedEndTransactionRequest(token);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Security);
            Assert.NotNull(result.EnhancedEndTransactionRQ);
            Assert.NotNull(result.MessageHeader);
            Assert.Equal("FromPartyId", result.MessageHeader.From.PartyId.First().Value);
            Assert.Equal("ToPartyId", result.MessageHeader.To.PartyId.First().Value);
            Assert.Equal(token, result.Security.BinarySecurityToken);
        }

        [Fact]
        public void GetEnhancedEndTransactionRQMapper_WithScheduleChange_SetsScheduleChangeIndicator()
        {
            // Arrange
            const string token = "test-token";

            // Act
            var result = EnhancedEndTransactionRequestMapper.GetEnhancedEndTransactionRequest(token, true);

            // Assert
            Assert.True(result.EnhancedEndTransactionRQ.EndTransaction.Ind);
            Assert.NotNull(result.EnhancedEndTransactionRQ.EndTransaction.ScheduleChange);
            Assert.True(result.EnhancedEndTransactionRQ.EndTransaction.ScheduleChange.Ind);
        }
    }
}

