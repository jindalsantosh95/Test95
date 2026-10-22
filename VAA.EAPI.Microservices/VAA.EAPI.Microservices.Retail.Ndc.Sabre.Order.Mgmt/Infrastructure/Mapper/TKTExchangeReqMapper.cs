using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.TKTExchange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper
{
    public static class TKTExchangeReqMapper
    {
        public static ExchangeRQRequest MapToTKTExchangeRQRequest(string token, string ticket, string pcc,string nameId)
        {
            return new ExchangeRQRequest
            {
                MessageHeader = new()
                {
                    Action = "TKT_ExchangeRQ",
                    ConversationId = ConfigReader.Get(Constants.Environment.ConversationId),
                    From = new()
                    {
                        PartyId = [new() { Value = ConfigReader.Get(Constants.Environment.FromPartyId) }]
                    },
                    To = new()
                    {
                        PartyId = [new() { Value = ConfigReader.Get(Constants.Environment.ToPartyId) }]
                    }
                },
                Security = new() { BinarySecurityToken = new() { Value = token } },
                ExchangeRQ = new()
                {
                    version = "1.9.0",
                    POS = new()
                    {
                        Pseudo = pcc,
                        Company = "1S"
                    },
                    TransactionInfo = new()
                    {
                        Item = new RequestDocumentRefund()
                        {
                            Number = new() { Value = ticket }
                        },
                        Options = string.IsNullOrWhiteSpace(nameId)
                            ? null
                            : new()
                            {
                                PNRSelect = new() { Name = nameId },
                                Endorsement = new() { Value = "INVOLUNTARY CHANGE" }
                            }
                    }
                }
            };
        }
    }
}
