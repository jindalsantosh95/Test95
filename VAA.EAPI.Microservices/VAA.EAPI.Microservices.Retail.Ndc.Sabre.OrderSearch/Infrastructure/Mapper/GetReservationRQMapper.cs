using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Infrastructure.Mapper
{
    public static class GetReservationRQMapper
    {
        public static GetReservationOperationRequest GetReservationRequest(string token, OrderSearchRequest request)
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
                    Locator = request.SearchFilterCriteria.BookingReferenceCriteria.BookingReference.RLOC,
                    Version = Constants.Application.GetReservationRQVersion,
                    RequestType = request.SearchFilterCriteria.HistoryFlag ? "Stateless": Constants.Application.SabreRequestType,
                    ReturnOptions = new ReturnOptions()
                    {
                        ShowTicketStatus = true,
                        SubjectAreas = request.SearchFilterCriteria.HistoryFlag ? [.. Constants.Application.SabreSubjectAreas, "HISTORICAL"] : Constants.Application.SabreSubjectAreas,
                        ViewName = Constants.Application.SabreViewName
                    }
                }
            };
        }
    }
}