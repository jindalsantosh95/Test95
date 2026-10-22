using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetElectronicDocument;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper
{
    public class GetElectronicDocumentRequestMapper
    {
        public static GetElectronicDocumentRQRequest GetElectronicDocumentMapper(string token, AccountableDocumentRef documentRef)
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
                    Version = "2.0.0",
                    SearchParameters = new()
                    {
                        resultType = "W",
                        DocumentNumber = documentRef.DocumentNumber,
                        DocumentType = (int)documentRef.DocumentType == 0 ? Constants.Application.DocumentTypeTKT : Constants.Application.DocumentTypeEMD
                    },
                    POS = new()
                    {
                        Company = Constants.Application.OwnerAirline,
                        Pseudo = Constants.Application.TargetCityCode
                    }
                }
            };
        }
    }
}
