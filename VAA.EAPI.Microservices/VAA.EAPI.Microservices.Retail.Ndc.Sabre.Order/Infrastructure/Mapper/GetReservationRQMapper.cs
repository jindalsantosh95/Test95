using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Constants;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Infrastructure.Mapper
{
    public static class GetReservationRQMapper
    {
        public static GetReservationOperationRequest GetReservationRequestMapper(string token, string pnr)
        {
            return new()
            {
                MessageHeader = new()
                {
                    Action = Constants.Application.GetReservationAction,
                    ConversationId = Constants.Application.GetReservationAction,
                    From = new()
                    {
                        PartyId = [new() { Value = Environment.FromPartyId }]
                    },
                    To = new()
                    {
                        PartyId = [new() { Value = Constants.Environment.ToPartyId }]
                    }
                },
                Security = new() { BinarySecurityToken = token },
                GetReservationRQ = new()
                {
                    Locator = pnr,
                    Version = Constants.Application.GetReservationRQVersion,
                    RequestType = Constants.Application.SabreRequestType,
                    ReturnOptions = new ReturnOptions()
                    {
                        SubjectAreas = Constants.Application.SabreSubjectAreas,
                        ViewName = Constants.Application.SabreViewName
                    }
                }

            };
        }
    }
}
