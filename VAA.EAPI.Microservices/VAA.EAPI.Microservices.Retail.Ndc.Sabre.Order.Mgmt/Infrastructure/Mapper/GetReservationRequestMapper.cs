using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper
{
    public static class GetReservationRequestMapper
    {
        public static GetReservationOperationRequest GetReservationRequest(string token, string pnr, bool isPartialFlown = false)
        {
            return new()
            {
                MessageHeader = new()
                {
                    Action = Constants.Application.GetReservationAction,
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
                GetReservationRQ = new()
                {
                    Locator = pnr,
                    Version = Constants.Application.GetReservationRQVersion,
                    RequestType = Constants.Application.RequestType,
                    ReturnOptions = new ReturnOptions()
                    {
                        ShowTicketStatus = true,
                        PriceQuoteServiceVersion = isPartialFlown ? "4.3.0" : null,
                        SubjectAreas = Constants.Application.SabreSubjectAreas,
                        ViewName = Constants.Application.SabreViewName
                    }
                }
            };
        }
    }
}
