using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreateSession;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Mapper.CreateSession
{
    public class CreateSessionRQMapper
    {
        public static SessionCreateRQRequest SessionCreateRQMapper(string pcc)
        {
            return new SessionCreateRQRequest()
            {
                MessageHeader = new MessageHeader()
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
                    Action = Constants.Application.CreateSessionAction,
                },
                Security = new Proxy.CreateSession.Security()
                {
                    UsernameToken = new()
                    {
                        Username = SabrePCCCredentialsHelper.GetPCCUserName(pcc),
                        Password = SabrePCCCredentialsHelper.GetPCCPassword(pcc),
                        Organization = pcc,
                        Domain = ConfigReader.Get(Constants.Environment.Domain),
                    }
                },
                SessionCreateRQ = new()
                {
                    returnContextID = true
                }
            };
        }
    }
}
