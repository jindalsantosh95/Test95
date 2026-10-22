using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EndTransaction;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper
{
    public static class SplitPNREndTransactionRQMapper
    {
        public static EndTransactionRQRequest EndTransationRequest(string token)
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
                    Action = Constants.Application.EndTransactionAction,
                },
                Security = new() { BinarySecurityToken = token },
                EndTransactionRQ = new()
                {
                    ReturnHostCommand = Constants.Application.ReturnHostCommandvalue,
                    ReturnHostCommandSpecified = true,
                    Version = Constants.Application.EndTransactionVersion,
                    Source = new()
                    {
                        ReceivedFrom = "API"
                    }
                }
            };
        }
    }
}
