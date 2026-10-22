using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreateToken;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Mapper.CreateToken
{
    public static class CreateTokenMapper
    {
        public static TokenCreateRQRequest RequestMapper(string pcc)
        {
            return new()
            {
                MessageHeader = new()
                {
                    From = new From
                    {
                        PartyId = [new PartyId() { Value = ConfigReader.Get(Constants.Environment.FromPartyId) }]
                    },
                    To = new To
                    {
                        PartyId = [new PartyId() { Value = ConfigReader.Get(Constants.Environment.ToPartyId) }]
                    },
                    ConversationId = ConfigReader.Get(Constants.Environment.ConversationId),
                    Action = Constants.Application.CreateTokenAction,
                },
                Security = new()
                {
                    UsernameToken = new()
                    {
                        Username = SabrePCCCredentialsHelper.GetPCCUserName(pcc),
                        Password = SabrePCCCredentialsHelper.GetPCCPassword(pcc),
                        Organization = pcc,
                        Domain = ConfigReader.Get(Constants.Environment.Domain),
                    }
                },
                TokenCreateRQ = new()
                {
                    Version = "1.0.0"
                }
            };
        }
    }
}
