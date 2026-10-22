using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Infrastructure.NDC.Sabre.Mapper.CreateSession;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Tests
{
    public class CreateSessionRQMapperTests
    {
        public CreateSessionRQMapperTests()
        {
            ConfigReader.Set("ConversationId", "ConversationId");
            ConfigReader.Set("FromPartyId", "FromPartyId");
            ConfigReader.Set("ToPartyId", "ToPartyId");
            ConfigReader.Set("SabreUsername", "SabreUsername");
            ConfigReader.Set("SabrePassword", "SabrePassword");
            ConfigReader.Set("PCC", "PCC");
            ConfigReader.Set("Domain", "Domain");
            ConfigReader.Set("keyvaulturi", "https://tempuri.org");

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public void CreateSessionRQMapper_Returns_PositiveResponse()
        {
            var request = CreateSessionRQMapper.SessionCreateRQMapper("testPcc");

            //Assert
            Assert.NotNull(request);
            Assert.NotNull(request.Security);
            Assert.NotNull(request.SessionCreateRQ);
            Assert.NotNull(request.MessageHeader);
            Assert.Equal("FromPartyId", request.MessageHeader.From.PartyId[0].Value);
            Assert.Equal("ToPartyId", request.MessageHeader.To.PartyId[0].Value);
            Assert.Equal("ConversationId", request.MessageHeader.ConversationId);
        }
    }
}
