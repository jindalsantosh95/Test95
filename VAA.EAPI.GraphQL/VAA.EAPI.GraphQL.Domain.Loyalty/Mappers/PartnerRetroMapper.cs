using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mappers
{
    public class PartnerRetroMapper
    {
        public static RetroTxnCustomerDetails MapCustomerDetailsToRequest(CustomerCustMS customerDetails, RetroTxnCustomerDetails retroTxnCustomerDetails)
        {
            if (customerDetails is null) throw new ArgumentNullException(nameof(customerDetails));
            if (retroTxnCustomerDetails is null) throw new ArgumentNullException(nameof(retroTxnCustomerDetails));

            retroTxnCustomerDetails.CustomerId = customerDetails.CustomerId;
            retroTxnCustomerDetails.MemberNumber = customerDetails.LoyaltyNumber;
            retroTxnCustomerDetails.TierCode = customerDetails.MemberTier;
            retroTxnCustomerDetails.MemberStatus = customerDetails.Status;
            retroTxnCustomerDetails.MemberType = customerDetails.MemberAccountType;
            retroTxnCustomerDetails.EmailAddress = customerDetails.ContactDetails?.EmailDetails?.FirstOrDefault().emailId;
            retroTxnCustomerDetails.DateOfBirth = Convert.ToString(customerDetails.DateOfBirth.Date.ToShortDateString());

            return retroTxnCustomerDetails;
        }

        public static SkyPartnerMSRequest MapSkyPartnerRetroRequestToTicketDetails(SkyPartnerRetroRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));

            return new SkyPartnerMSRequest
            {
                ticketInfo = new()
                {
                    FFPCarrierCode = Common.Constants.Application.PartnerAutoRetroFFPCarrierCode,
                    FFPAccountNumber = request.LoyTransaction?.CustomerDetails?.MemberNumber,
                    PassengerLastName = request.LoyTransaction?.CustomerDetails?.DocumentSurname,
                    PassengerFirstName = request.LoyTransaction?.CustomerDetails?.DocumentFirstName,
                    FFPMemberEmailAddress = request.LoyTransaction?.CustomerDetails?.EmailAddress,
                    FFPMemberDateOfBirth = Convert.ToDateTime(request.LoyTransaction?.CustomerDetails?.DateOfBirth).Date.ToString(Common.Constants.Application.ISODateFormat),
                    OperatingCarrierCode = request.LoyTransaction?.BookingDetails?.FlightDetails?.OperatedAirlineCode,
                    OperatingFlightNumber = request.LoyTransaction?.BookingDetails?.FlightDetails?.OperatedFlightNumber,
                    ScheduledFlightSegmentDepartureDate = Convert.ToDateTime(request.LoyTransaction?.BookingDetails?.FlightDetails?.FlightActivityDate).Date.ToString(Common.Constants.Application.ISODateFormat).Replace("-", ""),
                    FlightSegmentOrigin = request.LoyTransaction?.BookingDetails?.FlightDetails?.OriginAirportCode,
                    FlightSegmentDestination = request.LoyTransaction?.BookingDetails?.FlightDetails?.DestinationAirportCode,
                    EticketNumber = request.LoyTransaction?.BookingDetails?.TicketNumber,
                    SeatNumber = request.LoyTransaction?.BookingDetails?.SeatNumber,
                    OperatingCarrierRecordLocator = request.LoyTransaction?.BookingDetails?.GdsRecordLocator
                }
            };
        }

        public static CabinClassDetailsRequest MapToGetCabinClassDetailsRequest(SkyPartnerRetroRequest request, FlyingActivityDetails flyingDetailsResponse)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            if (flyingDetailsResponse is null) throw new ArgumentNullException(nameof(flyingDetailsResponse));

            return new CabinClassDetailsRequest
            {
                OperatedBookingClass = flyingDetailsResponse?.SkyClaimResponse?.OperatingCarierBookingClass,
                MarketingBookingClass = flyingDetailsResponse?.SkyClaimResponse?.MarketingCarrierBookingClass,
                OperatedAirlineCode = request.LoyTransaction.BookingDetails?.FlightDetails?.OperatedAirlineCode,
                MarketingAirlineCode = flyingDetailsResponse?.SkyClaimResponse?.MarketingCarrierCode,
                FlightActivityDate = Convert.ToDateTime(request.LoyTransaction.BookingDetails?.FlightDetails?.FlightActivityDate).Date.ToShortDateString(),
                OriginAirportCode = request.LoyTransaction.BookingDetails?.FlightDetails?.OriginAirportCode,
                DestinationAirportCode = request.LoyTransaction.BookingDetails?.FlightDetails?.DestinationAirportCode
            };
        }

        public static AirlineEarnedPointsRequest MapToCalculateEarnedPointsRequest(SkyPartnerRetroRequest request, FlyingActivityDetails flyingDetailsResponse, AirportDistanceAndCountries airportsDistanceAndCountry, CabinClassDetailsResponse cabinClassDetails)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            if (flyingDetailsResponse is null) throw new ArgumentNullException(nameof(flyingDetailsResponse));

            return new()
            {
                FlightSegments = new List<FlightSegments>()
                {
                    new(){
                    OperatingCarrier = flyingDetailsResponse?.SkyClaimResponse?.OperatingCarrierCode,
                    MarketingCarrier = flyingDetailsResponse?.SkyClaimResponse?.MarketingCarrierCode,
                    MarketingFlightNumber = flyingDetailsResponse?.SkyClaimResponse?.MarketingFlightNumber,
                    OperatingFlightNumber = flyingDetailsResponse?.SkyClaimResponse?.OperatingFlightNumber,
                    ScheduledDepartureTime = Convert.ToDateTime(request.LoyTransaction.BookingDetails?.FlightDetails?.FlightActivityDate).Date.ToString(Common.Constants.Application.ISODateFormat),
                    OriginAirportCode = flyingDetailsResponse?.SkyClaimResponse?.FlightSegmentOrigin,
                    DestinationAirportCode = flyingDetailsResponse?.SkyClaimResponse?.FlightSegmentDestination,
                    OriginCountryCode = airportsDistanceAndCountry?.OriginCountryCode,
                    DestinationCountryCode = airportsDistanceAndCountry?.DestinationCountryCode,
                    ClassOfService = flyingDetailsResponse?.SkyClaimResponse?.CabinClassCode,
                    SegmentDistance = airportsDistanceAndCountry?.DistanceInMiles,
                    OperatingBookingClass = flyingDetailsResponse?.SkyClaimResponse?.OperatingCarierBookingClass,
                    MarketingBookingClass = flyingDetailsResponse?.SkyClaimResponse?.MarketingCarrierBookingClass,
                    CabinClass = cabinClassDetails?.CabinClass ?? flyingDetailsResponse?.SkyClaimResponse?.CabinClassCode
                    }
                },
                CustomerDetails = new List<CustomerDetails>()
                {
                    new(){
                    MemberNumber = request.LoyTransaction?.CustomerDetails?.MemberNumber,
                    TierCode = request.LoyTransaction?.CustomerDetails?.TierCode,
                    MemberType = request.LoyTransaction?.CustomerDetails?.MemberType,
                    MemberStatus = request.LoyTransaction?.CustomerDetails?.MemberStatus
                    }
                }
            };
        }

        public static TransactionRequest MapToAccrualTransactionRequest(SkyPartnerRetroRequest request, FlyingActivityDetails flyingDetails, CreditPointsResponse points, string productId, PartnerProfileDetails partnerProfile, string airportsDistanceInMiles, string travelClass)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            if (flyingDetails is null) throw new ArgumentNullException(nameof(flyingDetails));
            if (points is null) throw new ArgumentNullException(nameof(points));
            if (productId is null) throw new ArgumentNullException(nameof(productId));
            if (partnerProfile is null) throw new ArgumentNullException(nameof(partnerProfile));

            List<CreditDetails> credits = new List<CreditDetails>();

            if (Convert.ToInt32(points.SegmentResponses?.FirstOrDefault().BasePoints) > 0)
            {
                CreditDetails credit = new()
                {
                    CreatedBy = request.CreatedBy,
                    LastUpdatedBy = request.CreatedBy,
                    CreditValue = points.SegmentResponses?.FirstOrDefault().BasePoints,
                    PointType = Common.Constants.Application.MilesType,
                    PointSubType = Common.Constants.Application.BaseMilesType,
                    QualifyingFlag = Common.Constants.Application.MilesQualifyingFlag
                };
                credits.Add(credit);
            }

            if (Convert.ToInt32(points.SegmentResponses?.FirstOrDefault().CabinBonusPoints) > 0)
            {
                CreditDetails credit = new()
                {
                    CreatedBy = request.CreatedBy,
                    LastUpdatedBy = request.CreatedBy,
                    CreditValue = points.SegmentResponses?.FirstOrDefault().CabinBonusPoints,
                    PointType = Common.Constants.Application.MilesType,
                    PointSubType = Common.Constants.Application.CabinBonusType,
                    QualifyingFlag = Common.Constants.Application.MilesQualifyingFlag
                };
                credits.Add(credit);
            }

            if (Convert.ToInt32(points.SegmentResponses?.FirstOrDefault().TierBonusPoints) > 0)
            {
                CreditDetails credit = new()
                {
                    CreatedBy = request.CreatedBy,
                    LastUpdatedBy = request.CreatedBy,
                    CreditValue = points.SegmentResponses?.FirstOrDefault().TierBonusPoints,
                    PointType = Common.Constants.Application.MilesType,
                    PointSubType = Common.Constants.Application.TierBonusType,
                    QualifyingFlag = Common.Constants.Application.MilesQualifyingFlag
                };
                credits.Add(credit);
            }

            if (Convert.ToInt32(points.SegmentResponses?.FirstOrDefault().TierPointsAward) > 0)
            {
                CreditDetails credit = new()
                {
                    CreatedBy = request.CreatedBy,
                    LastUpdatedBy = request.CreatedBy,
                    CreditValue = points.SegmentResponses?.FirstOrDefault().TierPointsAward,
                    PointType = Common.Constants.Application.TierPointsType,
                    PointSubType = "",
                    QualifyingFlag = Common.Constants.Application.MilesQualifyingFlagForTierPoints
                };
                credits.Add(credit);
            }

            List<SegmentDetails> segmentDetails = new List<SegmentDetails>()
            {
                new()
                {
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = request.CreatedBy,
                    LastUpdatedOn= DateTime.UtcNow,
                    LastUpdatedBy = request.CreatedBy,
                    BookingClass = flyingDetails?.SkyClaimResponse?.CabinClassCode,
                    DepartureDate = request.LoyTransaction.BookingDetails?.FlightDetails?.FlightActivityDate,
                    DestinationAirportcode = request.LoyTransaction.BookingDetails?.FlightDetails?.DestinationAirportCode,
                    MarketingCarrierCode= flyingDetails.SkyClaimResponse?.MarketingCarrierCode,
                    MarketingFlightNumber= flyingDetails.SkyClaimResponse?.MarketingFlightNumber,
                    OperatingFlightNumber= flyingDetails.SkyClaimResponse?.OperatingFlightNumber,
                    OriginAirportcode= request.LoyTransaction.BookingDetails?.FlightDetails?.OriginAirportCode,
                    PartnerCode = partnerProfile.PartnerCode,
                    SegmentMiles = airportsDistanceInMiles
                }
            };

            return new()
            {
                TransactionDocument = new()
                {
                    TransactionId = Utility.GetId(),
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = request.CreatedBy,
                    LastUpdatedBy = request.CreatedBy,
                    LastUpdatedOn = DateTime.UtcNow,
                    CustomerId = request.LoyTransaction.CustomerDetails?.CustomerId,
                    Comments = request.LoyTransaction?.VsComments,
                    ActivityNumber = request.LoyTransaction.BookingDetails?.FlightDetails?.OperatedFlightNumber,
                    MemberNumber = request.LoyTransaction.CustomerDetails?.MemberNumber,
                    TransactionValue = points.SegmentResponses?.FirstOrDefault().PointsAward,
                    PartnerId = partnerProfile.RowId,
                    TransactionDate = DateTime.UtcNow,
                    ProductId = productId,
                    StatusCode = Common.Constants.Application.TxnProcessingStatus,
                    TransactionSubType = Common.Constants.Application.TxnSubType,
                    TransactionChannel = request.TransactionChannel,
                    ActivityDate = request.LoyTransaction.BookingDetails?.FlightDetails.FlightActivityDate,
                    TransactionType = Common.Constants.Application.AccrualTransactionType,
                    TierPointsToCredit = points.SegmentResponses?.FirstOrDefault().TierPointsAward,
                    ProcessingComment = points.SegmentResponses?.FirstOrDefault()?.ProcessingComments,
                    FlightDetails = new()
                    {
                        Distance = airportsDistanceInMiles,
                        BookingClass = flyingDetails?.SkyClaimResponse?.OperatingCarierBookingClass,
                        OriginAirportcode = request.LoyTransaction.BookingDetails?.FlightDetails?.OriginAirportCode,
                        DestinationAirport = request.LoyTransaction.BookingDetails?.FlightDetails?.DestinationAirportCode,
                        ExternalReservationNumber = request.LoyTransaction.BookingDetails?.GdsRecordLocator,
                        Ticketnumber = request.LoyTransaction.BookingDetails?.TicketNumber,
                        OperatingFlightNumber = flyingDetails?.SkyClaimResponse?.OperatingFlightNumber,
                        DepartureDate = request.LoyTransaction.BookingDetails?.FlightDetails?.FlightActivityDate,
                        RetroFlag = Common.Constants.Application.RetroFlag,
                        CabinClass = flyingDetails?.SkyClaimResponse?.CabinClassCode,
                        TravelClass = travelClass,
                        BookingDate = !string.IsNullOrEmpty(request.LoyTransaction.BookingDetails?.TicketIssueDate) ? Convert.ToDateTime(request.LoyTransaction.BookingDetails?.TicketIssueDate) : null,
                        GDSRecordLocator = request.LoyTransaction.BookingDetails?.GdsRecordLocator,
                        MarketingFlightNumber = flyingDetails?.SkyClaimResponse?.MarketingFlightNumber,
                        DocumentType = request.LoyTransaction.CustomerDetails?.PrimaryDocumentType,
                        RetroOverrideFlag = request.NonVARetroOverrideFlag.ToString(),
                        FirstNameAsPerDocument = request.LoyTransaction.CustomerDetails?.DocumentFirstName,
                        LastNameAsPerDocument = request.LoyTransaction.CustomerDetails?.DocumentSurname,
                        SecondaryDocumentType = request.LoyTransaction.CustomerDetails?.SecondaryDocumentType,
                        Seatnumber = request.LoyTransaction.BookingDetails?.SeatNumber,
                        CodeShareFlag = request.LoyTransaction.CodeshareFlag,
                        MarketingBookingClass = flyingDetails?.SkyClaimResponse?.MarketingCarrierBookingClass,
                        SecondaryDocumentStatus = request.LoyTransaction.CustomerDetails?.SecondaryDocumentStatus,
                        PrimaryDocumentStatus = request.LoyTransaction.CustomerDetails?.PrimaryDocumentStatus,
                        BoardingNumber = request.LoyTransaction.BookingDetails?.BoardingNumber,
                        MarketingPartnerCode = flyingDetails?.SkyClaimResponse?.MarketingCarrierCode,
                        VAPostingTypeCode = Common.Constants.Application.VAPostingTypeCode,
                        OperatingAirlineAuthNumber = flyingDetails?.SkyClaimResponse?.UniqueIdentificationNumber,
                        PNRName = request.LoyTransaction.CustomerDetails?.DocumentSurname + "/" + request.LoyTransaction.CustomerDetails?.DocumentFirstName,
                        PassengerDetails = new List<PassengerDetails>()
                        {
                            new()
                            {
                            CreatedOn = DateTime.UtcNow,
                            CreatedBy = request.CreatedBy,
                            LastUpdatedOn = DateTime.UtcNow,
                            LastUpdatedBy = request.CreatedBy,
                            PassengerName = request.LoyTransaction.CustomerDetails?.DocumentFirstName + " " + request.LoyTransaction.CustomerDetails?.DocumentSurname,
                            Status = request.LoyTransaction.CustomerDetails?.MemberStatus,
                            PhysicalTicketNumber = request.LoyTransaction.BookingDetails?.TicketNumber,
                            ValueToCredit = points.SegmentResponses?.FirstOrDefault().PointsAward,
                            SegmentDetails = segmentDetails,
                            NumberOfSegments = Convert.ToString(segmentDetails.Count)
                            }
                        }
                    },
                    CreditDetails = credits
                }
            };
        }

        public static SkyMSFeedbackRequest MapToSkyMSFeedbackRequest(FlyingActivityDetails flyingDetailsResponse, CreditPointsResponse points, string airportsDistanceInMiles)
        {
            if (flyingDetailsResponse is null) throw new ArgumentNullException(nameof(flyingDetailsResponse));

            return new()
            {
                feedbackRequest = new()
                {
                    UniqueIdentificationNumber = flyingDetailsResponse?.SkyClaimResponse?.UniqueIdentificationNumber,
                    FFPCarrierCode = Common.Constants.Application.PartnerAutoRetroFFPCarrierCode,
                    OperatingCarrierCode = flyingDetailsResponse?.SkyClaimResponse?.OperatingCarrierCode,
                    PostingStatus = Common.Constants.Application.PostingStatusFlag,
                    BaseMiles = points?.SegmentResponses?.FirstOrDefault()?.BasePoints,
                    ClassOfServiceBonusMiles = points?.SegmentResponses?.FirstOrDefault()?.CabinBonusPoints,
                    EliteTierBonusMiles = points?.SegmentResponses?.FirstOrDefault()?.TierBonusPoints
                }
            };
        }

        public static SkyPartnerRetroResponse MapToSkyPartnerRetroResponse(Transaction transactionData, IEnumerable<ErrorDetails> errors)
        {
            if (errors is not null)
            {
                return new SkyPartnerRetroResponse()
                {
                    Errors = new List<ErrorDetails>()
                    {
                         new ErrorDetails()
                         {
                             Code = errors?.FirstOrDefault()?.Code,
                             Message= errors?.FirstOrDefault()?.Message,
                             DeveloperMessage= errors?.FirstOrDefault()?.DeveloperMessage
                         }
                    }
                };
            }

            return new SkyPartnerRetroResponse()
            {
                TransactionId = transactionData.TransactionId,
                CustomerId = transactionData.CustomerId
            };
        }
    }
}
