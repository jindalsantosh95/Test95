using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetElectronicDocument;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Infrastructure.Mapper
{
    public static class GetElectronicDocumentRQMapper
    {
        public static GetElectronicDocumentRQRequest GetElectronicDocumentRQRequest(string token, string ticketNum, string ticketType)
        {
            return new()
            {
                MessageHeader = new()
                {
                    Action = Constants.Application.GetElectronicDocumentAction,
                    ConversationId = ConfigReader.Get(Constants.Environment.ConversationId),
                    From = new()
                    {
                        PartyId = [new() { Value = Constants.Environment.FromPartyId }]
                    },
                    To = new()
                    {
                        PartyId = [new() { Value = Constants.Environment.ToPartyId }]
                    }
                },
                Security = new() { BinarySecurityToken = new() { Value = token} },
                GetElectronicDocumentRQ = new()
                {
                    SearchParameters = new()
                    {
                        resultType = "W",
                        DocumentNumber = ticketNum,
                        DocumentType = ticketType
                    },
                    POS = new()
                    {
                        Company = Constants.Application.OwnerAirline,
                        Pseudo = Constants.Application.TargetCityCode,
                    },
                    Version = "2.0.0"
                },
            };
        }
    }
}
