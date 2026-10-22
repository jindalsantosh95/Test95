using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Infrastructure.Mapper
{
    public static class GetReservationRequestMapper
    {
        public static GetReservationOperationRequest Map(string token, string pnr)
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
                    RequestType = Constants.Application.GetReservationRequestType,
                    ReturnOptions = new()
                    {
                        SubjectAreas = Constants.Application.GetReservationSubjectAreas,
                        ViewName = Constants.Application.GetReservationViewName
                    }
                }
            };
        }
    }
}
