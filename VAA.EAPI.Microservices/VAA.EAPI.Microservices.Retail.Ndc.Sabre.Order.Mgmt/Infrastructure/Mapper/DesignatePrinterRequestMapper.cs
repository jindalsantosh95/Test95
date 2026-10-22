using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.DesignatePrinter;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper
{
    public class DesignatePrinterRequestMapper
    {
        public static DesignatePrinterRQRequest Map(string token, string pcc)
        {
            return new()
            {
                MessageHeader = new()
                {
                    Action = Constants.Application.DesignatePrinterAction,
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
                Security = new() { BinarySecurityToken = token },
                DesignatePrinterRQ = new()
                {
                    Profile = new()
                    {
                        Number = ConfigReader.Get(Constants.Environment.DirectTicketingPCC).Split(",").Contains(pcc) ? Constants.Application.DirectTicketingDesignatePrinterProfileNumber : Constants.Application.DesignatePrinterProfileNumber
                    }
                }
            };
        }
    }
}
