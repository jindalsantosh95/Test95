using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.VoidAirTicket;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper
{
    public static class VoidAirTicketRequestMapper
    {
        public static VoidTicketRQRequest GetVoidAirTicketRequest(string token, string index_number)
        {
            return new()
            {
                VoidTicketRQ = GetVoidTicketRQ(index_number),
                MessageHeader = new()
                {
                    Action = Constants.Application.VoidAirTicketAction,
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
                Security = new() { BinarySecurityToken = token }
            };
        }

        private static VoidTicketRQ GetVoidTicketRQ(string index_number)
        {
            return new()
            {
                Ticketing = new VoidTicketRQTicketing()
                {
                    RPH = index_number
                }
            };
        }
    }
}
