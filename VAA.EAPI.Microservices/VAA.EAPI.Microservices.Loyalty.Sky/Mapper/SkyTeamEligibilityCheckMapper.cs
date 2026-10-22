using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.Sky.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Sky.Mapper
{
    public class SkyTeamEligibilityCheckMapper
    {
        public static OnlineRetroCreditRequestType SkyCheckEligibilityRequest(TicketInfo request)
        {
            return new OnlineRetroCreditRequestType
            {
                FFPCarrierCode = request.FFPCarrierCode,
                FFPAccountNumber = request.FFPAccountNumber,
                PassengerLastName = request.PassengerLastName,
                PassengerFirstName = request.PassengerFirstName,
                FFPMemberDateOfBirth = DateTime.Parse(request.FFPMemberDateOfBirth).ToString(Constants.Application.BritishDateFormat),
                OperatingCarrierCode = request.OperatingCarrierCode,
                OperatingFlightNumber = request.OperatingFlightNumber,
                ScheduledFlightSegmentDepartureDate = request.ScheduledFlightSegmentDepartureDate,
                FlightSegmentOrigin = request.FlightSegmentOrigin,
                FlightSegmentDestination = request.FlightSegmentDestination,
                EticketNumber = request.EticketNumber,
                FFPMemberEmailAddress = request.FFPMemberEmailAddress,
                SeatNumber = request.SeatNumber,
                SequenceNumber = request.SequenceNumber ?? 0,
                OperatingCarrierRecordLocator = request.OperatingCarrierRecordLocator,
                InternetProtocolAddress = request.InternetProtocolAddress
            };
        }

        public static CheckEligibilityResponse SkyCheckEligibilitytResponse(OnlineRetroCreditResponseType onlineRetroCreditResponseType)
        {
            Dictionary<ItemsChoiceType, string> matchedItems = new();

            for (int i = 0; i < onlineRetroCreditResponseType.Items?.Length; i++)
            {
                matchedItems[onlineRetroCreditResponseType.ItemsElementName[i]] = onlineRetroCreditResponseType.Items[i].ToString();
            }

            var response = matchedItems.Any() ? new SkyClaimResponse() : null;

            foreach (var pair in matchedItems)
            {
                switch (pair.Key)
                {
                    case ItemsChoiceType.UniqueIdentificationNumber:
                        response.UniqueIdentificationNumber = pair.Value;
                        break;
                    case ItemsChoiceType.FFPCarrierCode:
                        response.FFPCarrierCode = pair.Value;
                        break;
                    case ItemsChoiceType.FFPAccountNumber:
                        response.FFPAccountNumber = pair.Value;
                        break;
                    case ItemsChoiceType.PassengerLastName:
                        response.PassengerLastName = pair.Value;
                        break;
                    case ItemsChoiceType.PassengerFirstName:
                        response.PassengerFirstName = pair.Value;
                        break;
                    case ItemsChoiceType.MarketingCarrierCode:
                        response.MarketingCarrierCode = pair.Value;
                        break;
                    case ItemsChoiceType.MarketingFlightNumber:
                        response.MarketingFlightNumber = pair.Value;
                        break;
                    case ItemsChoiceType.OperatingCarrierCode:
                        response.OperatingCarrierCode = pair.Value;
                        break;
                    case ItemsChoiceType.OperatingFlightNumber:
                        response.OperatingFlightNumber = pair.Value;
                        break;
                    case ItemsChoiceType.ScheduledFlightSegmentDepartureDate:
                        response.ScheduledFlightSegmentDepartureDate = pair.Value;
                        break;
                    case ItemsChoiceType.CabinClassCode:
                        response.CabinClassCode = pair.Value;
                        break;
                    case ItemsChoiceType.MarketingCarrierBookingClass:
                        response.MarketingCarrierBookingClass = pair.Value;
                        break;
                    case ItemsChoiceType.MarketingCarrierBookingSubClass:
                        response.MarketingCarrierBookingSubClass = pair.Value;
                        break;
                    case ItemsChoiceType.OperatingCarierBookingClass:
                        response.OperatingCarierBookingClass = pair.Value;
                        break;
                    case ItemsChoiceType.OperatingCarrierBookingSubClass:
                        response.OperatingCarrierBookingSubClass = pair.Value;
                        break;
                    case ItemsChoiceType.FlightSegmentOrigin:
                        response.FlightSegmentOrigin = pair.Value;
                        break;
                    case ItemsChoiceType.FlightSegmentDestination:
                        response.FlightSegmentDestination = pair.Value;
                        break;
                    case ItemsChoiceType.EticketNumber:
                        response.EticketNumber = pair.Value;
                        break;
                    case ItemsChoiceType.EticketCouponNumber:
                        response.EticketCouponNumber = pair.Value;
                        break;
                    case ItemsChoiceType.EtktMatchingResult:
                        response.EtktMatchingResult = pair.Value;
                        break;
                    case ItemsChoiceType.FlightTypeIndicator:
                        response.FlightTypeIndicator = pair.Value;
                        break;
                    case ItemsChoiceType.PostingDate:
                        response.PostingDate = pair.Value;
                        break;
                    case ItemsChoiceType.PostingNonEligibilityIndicator:
                        response.PostingNonEligibilityIndicator = !pair.Value.IsNullOrEmpty() ? pair.Value == "R" ? SkyClaimResponse.PostingNonEligibilityIndicators.R : SkyClaimResponse.PostingNonEligibilityIndicators.Y : null;
                        break;
                }
            }

            if (response != null && (onlineRetroCreditResponseType.EligibilityDenialCode.Is("000") || onlineRetroCreditResponseType.EligibilityDenialCode.Is("001")) && string.IsNullOrEmpty(response?.OperatingCarierBookingClass))
            {
                response.OperatingCarierBookingClass = response.MarketingCarrierBookingClass;
            }           

            return new CheckEligibilityResponse
            {
                TrackID = onlineRetroCreditResponseType.TrackID,
                EligibilityDenialCode = onlineRetroCreditResponseType.EligibilityDenialCode,
                SkyClaimResponse = response,
                IsEligible = onlineRetroCreditResponseType.EligibilityDenialCode.Equals(Constants.Application.SkyTeamOnlineRetroCreditSuccessCode) || onlineRetroCreditResponseType.EligibilityDenialCode.Equals(Constants.Application.SkyTeamOnlineRetroCredit_SecondRequest_SuccessCode),
            };
        }
    }
}
