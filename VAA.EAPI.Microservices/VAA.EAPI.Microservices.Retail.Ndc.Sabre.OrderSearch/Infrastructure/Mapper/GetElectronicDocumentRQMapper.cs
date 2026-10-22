using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetElectronicDocument;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Infrastructure.Mapper
{
    public static class GetElectronicDocumentRQMapper
    {
        public static GetElectronicDocumentRQRequest GetElectronicDocumentRQRequest(string token, AccountableDocumentRef documentRef)
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
                Security = new() { BinarySecurityToken = new() { Value = token } },
                GetElectronicDocumentRQ = new()
                {
                    SearchParameters = new()
                    {
                        resultType = "W",
                        DocumentNumber = documentRef.DocumentNumber,
                        DocumentType = (int)documentRef.DocumentType == 0 ? Constants.Application.DocumentTypeTKT : Constants.Application.DocumentTypeEMD
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
