using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.IgnoreTransaction;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Mapper.IgnoreTransaction
{
    public class IgnoreTransactionRQMapper
    {
        public static IgnoreTransactionRQRequest IgnoreTransactionRequestMapper(IgnoreTransactionCommand request)
        {
            return new IgnoreTransactionRQRequest()
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
                    CPAId = request.PCC.IsNullOrWhiteSpace()
                        ? ConfigReader.Get(Constants.Environment.PCC)
                        : request.PCC,
                    ConversationId = ConfigReader.Get(Constants.Environment.ConversationId),
                    Action = Constants.Application.IgnoreTransactionAction
                },
                Security = new Security1()
                {
                    BinarySecurityToken = request.BinarySecurityToken
                },
                IgnoreTransactionRQ = new()
            };
        }
    }
}
