using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.LoungeManagement.SkyTeam.Application.Query;
using VAA.EAPI.LoungeManagement.SkyTeam.Domain;
using VAA.EAPI.LoungeManagement.SkyTeam.Domain.TicketData;

namespace VAA.EAPI.LoungeManagement.SkyTeam.Mapper
{
    public static class TicketResponseMapper
    {
        public static RetrieveTicketDetailsResponse GetTicketResponseMapper(TicketFeedResponse ticketFeedResponse, RetriveTicketQuery request)
        {
            if (ticketFeedResponse.Coupons == null)
            {
                return GetDenialResponse(request);
            }

            RetrieveTicketDetailsResponse objRetrieveTicketDetails = new RetrieveTicketDetailsResponse();

            var ticketData = new EticketData()
            {
                ETKTNumber = ticketFeedResponse.Document.TktDocNb,
                PassengerFirstName = ticketFeedResponse.Document.PaxFrstNm,
                PassengerLastName = ticketFeedResponse.Document.PaxLstNm,
                ErrorOrAccessDenialCode = Constants.Application.EdcForTicketFound,
                ErrorOrAccessDenialMessage = Constants.Application.EdmForTicketFound,
            };

            objRetrieveTicketDetails.TrackId = request.RetrieveTicketDetailsRequest.TrackID;
            objRetrieveTicketDetails.EticketData = ticketData;
            objRetrieveTicketDetails.EticketData.FlightSegmentDetails = GetTicketSegments(ticketFeedResponse);

            return objRetrieveTicketDetails;
        }

        private static RetrieveTicketDetailsResponse GetDenialResponse(RetriveTicketQuery request)
        {
            return new RetrieveTicketDetailsResponse
            {
                TrackId = request.RetrieveTicketDetailsRequest.TrackID,
                ErrorOrAccessDenial = new ErrorOrAccessDenial()
                {
                    BenefitEligibilityFromAirlineRES = Constants.Application.EligibilityAirlineRES,
                    ErrorOrAccessDenialCodeFromAirlineRES = Constants.Application.EdcForTicketNotFound,
                    ErrorOrAccessDenialMessageFromAirlineRES = Constants.Application.EdmForTicketNotFound,
                    ErrorInformation = Constants.Application.ErrorInfo,
                }
            };
        }

        public static Func<TicketFeedResponse, IEnumerable<FlightSegmentDetails>> GetTicketSegments = (ticketFeedResponse) =>
        {
            if (ticketFeedResponse.AttachedEmdDocuments!=null)
            {
                foreach (var item in ticketFeedResponse.AttachedEmdDocuments)
                {
                    foreach (var emdCpn in item.Coupons)
                    {
                        if (emdCpn.EtCpnCrrRole!=null)
                        {
                            var asCpn = ticketFeedResponse.Coupons.FirstOrDefault(c => c.EtCpnCrrRole.FirstOrDefault().ROrigTrpnLocTxt.Equals(emdCpn.EtCpnCrrRole.FirstOrDefault().ROrigTrpnLocTxt) && c.EtCpnCrrRole.FirstOrDefault().RDestTrpnLocTxt.Equals(emdCpn.EtCpnCrrRole.FirstOrDefault().RDestTrpnLocTxt));
                            if (asCpn != null)
                            {
                                asCpn.EtCpnCrrRole.FirstOrDefault().VolantaryUpgrade = 1;
                                asCpn.EtCpnCrrRole.FirstOrDefault().UpgradedClassOfService = emdCpn.EtCpnCrrRole.FirstOrDefault().EqOwrTktgCOSCd;
                            }  
                        }
                    }
                } 
            }

            var allSegments = ticketFeedResponse.Coupons.Select(c => (FlightSegmentDetails)c).ToList();

            if (!ticketFeedResponse.SecondaryTickets.IsNullOrEmpty() && ticketFeedResponse.SecondaryTickets.Any(s => !s.Coupon.IsNullOrEmpty()))
            {
                allSegments.AddRange(ticketFeedResponse.SecondaryTickets.SelectMany(st => st.Coupon.Select(c => (FlightSegmentDetails)c)));
            }

            return allSegments;
        };
    }
}

