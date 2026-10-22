using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.RuleEngine;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Transaction;
using CustomerDocument = VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer.Customer;
using FlyingClubMemberDocument = VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember.FlyingClubMember;
using PartnerProfile = VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PartnersRetrieval.PartnerProfile;
using Product = VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayBonus.Product;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.AutoAccrual
{
    public class TransactionCreateEventData
    {
        public TransactionDocument TransactionDocument { get; set; }

        public string ProductName { get; set; }

        public string PartnerName { get; set; }

        public static explicit operator TransactionCreateEventData((CustomerDocument customer, FlyingClubMemberDocument flyingClubMember, AutoAccrualRequest request, SegmentResponse segment, Product product, PartnerProfile partner, int distanceInMiles) source)
        {
            ArgumentNullException.ThrowIfNull(source.customer);
            ArgumentNullException.ThrowIfNull(source.flyingClubMember);
            ArgumentNullException.ThrowIfNull(source.request);
            ArgumentNullException.ThrowIfNull(source.segment);
            ArgumentNullException.ThrowIfNull(source.product);
            ArgumentNullException.ThrowIfNull(source.partner);

            var now = DateTime.UtcNow;
            var user = Constants.Application.AutoAccrualUser;
            var operatingFlight = source.request.FlightDetails?.OperatingFlight;
            var marketingFlight = source.request.FlightDetails?.MarketingFlight;

            return new TransactionCreateEventData
            {
                ProductName = source.product.ProductName,
                PartnerName = source.partner.Name,
                TransactionDocument = new TransactionDocument
                {
                    TransactionId = Utility.GetId(),
                    CreatedOn = now,
                    CreatedBy = user,
                    LastUpdatedOn = now.ToString(Constants.Application.ISODateTimeFormat),
                    LastUpdatedBy = user,
                    CustomerId = source.customer.CustomerId,
                    MemberNumber = source.flyingClubMember.MemberNumber,
                    TransactionDate = now.ToString(Constants.Application.ISODateTimeFormat),
                    ProductId = source.product.ProductId,
                    PartnerId = source.partner.PartnerId,
                    TransactionType = Constants.Application.AccrualTrxnType,
                    TransactionSubType = Constants.Application.AccrualTrxnSubType,
                    TransactionValue = source.segment.PointsAward.ToString(),
                    TierPointsToCredit = source.segment.TierPointsAward.ToString(),
                    StatusCode = Constants.Application.SuccessTrxnStatus,
                    TransactionChannel = Constants.Application.AutoAccrualTransactionChannel,
                    ActivityDate = source.request.ScheduledDepartureDate,
                    ActivityNumber = operatingFlight?.OperatingFlightNumber,
                    ClassCode = source.request.BookingDetails?.OperatingCarrierBookingClass,
                    ProcessingComment = source.segment.ProcessingComments,
                    StatementText = string.Empty,
                    TransactionIntegrationId = source.request.UniqueIdentification?.UniqueIdentificationNumber,
                    FlightDetails = new Flightdetails
                    {
                        Distance = source.distanceInMiles.ToString(),
                        BookingClass = source.request.BookingDetails?.OperatingCarrierBookingClass,
                        MarketingBookingClass = source.request.BookingDetails?.MarketingCarrierBookingClass,
                        CabinClass = source.request.BookingDetails?.CabinClassCode,
                        OriginAirportcode = source.request.FlightSegment?.Origin,
                        DestinationAirport = source.request.FlightSegment?.Destination,
                        OperatingFlightNumber = operatingFlight?.OperatingFlightNumber,
                        MarketingFlightNumber = marketingFlight?.MarketingFlightNumber,
                        MarketingPartnerCode = marketingFlight?.MarketingCarrierCode,
                        CodeShareFlag = GetCodeShareFlag(source.request.FlightTypeIndicator, operatingFlight?.OperatingCarrierCode, marketingFlight?.MarketingCarrierCode),
                        DepartureDate = source.request.ScheduledDepartureDate,
                        Ticketnumber = source.request.DocumentDetails?.DocumentNumber,
                        BookingDate = source.request.DocumentDetails?.DocumentIssueDateTime,
                        OperatingAirlineAuthNumber = source.request.UniqueIdentification?.UniqueIdentificationNumber,
                        Quantity = "1",
                        NumberOfSegments = "1",
                        PassengerDetails = CreatePassengerDetails(source, user, now, operatingFlight, marketingFlight)
                    },
                    CreditDetails = MapCreditDetails(source.segment, user, now)
                }
            };
        }

        private static List<Passengerdetail> CreatePassengerDetails((CustomerDocument customer, FlyingClubMemberDocument flyingClubMember, AutoAccrualRequest request, SegmentResponse segment, Product product, PartnerProfile partner, int distanceInMiles) source, string user, DateTime now, Flight? operatingFlight, Flight? marketingFlight)
        {
            return new List<Passengerdetail>
            {
                new Passengerdetail
                {
                    PassengerId = Utility.GetId(),
                    CreatedOn = now.ToString(Constants.Application.ISODateTimeFormat),
                    CreatedBy = user,
                    LastUpdatedOn = now.ToString(Constants.Application.ISODateTimeFormat),
                    LastUpdatedBy = user,
                    PassengerName = $"{source.request.PassengerName?.GivenName} {source.request.PassengerName?.SurName}".Trim(),
                    NumberOfSegments = "1",
                    PhysicalTicketNumber = source.request.DocumentDetails?.DocumentNumber,
                    ValueToCredit = source.segment.PointsAward.ToString(),
                    SegmentDetails = new List<Segmentdetail>
                    {
                        new Segmentdetail
                        {
                            SegmentId = Utility.GetId(),
                            CreatedOn = now.ToString(Constants.Application.ISODateTimeFormat),
                            CreatedBy = user,
                            LastUpdatedOn = now.ToString(Constants.Application.ISODateTimeFormat),
                            LastUpdatedBy = user,
                            BookingClass = source.request.BookingDetails?.OperatingCarrierBookingClass,
                            DepartureDate = source.request.ScheduledDepartureDate,
                            DestinationAirportcode = source.request.FlightSegment?.Destination,
                            MarketingCarrierCode = marketingFlight?.MarketingCarrierCode,
                            MarketingFlightNumber = marketingFlight?.MarketingFlightNumber,
                            OperatingFlightNumber = operatingFlight?.OperatingFlightNumber,
                            OriginAirportcode = source.request.FlightSegment?.Origin,
                            PartnerCode = source.partner.PartnerCode,
                            SegmentMiles = source.distanceInMiles.ToString(),
                            CouponNumber = source.request.DocumentDetails?.DocumentCouponNumber.ToString()
                        }
                    }
                }
            };
        }

        private static List<Creditdetail> MapCreditDetails(SegmentResponse segment, string user, DateTime now)
        {
            var credits = new List<Creditdetail>();
            AddCredit(credits, segment.BasePoints, Constants.Application.MilesPointType, Constants.Application.BaseMilesPointSubType, Constants.Application.MilesQualifyingFlag, user, now);
            AddCredit(credits, segment.CabinBonusPoints, Constants.Application.MilesPointType, Constants.Application.CabinBonusPointSubType, Constants.Application.MilesQualifyingFlag, user, now);
            AddCredit(credits, segment.TierBonusPoints, Constants.Application.MilesPointType, Constants.Application.TierBonusPointSubType, Constants.Application.MilesQualifyingFlag, user, now);
            AddCredit(credits, segment.TierPointsAward, Constants.Application.TierPointsPointType, string.Empty, Constants.Application.TierPointsQualifyingFlag, user, now);
            return credits;
        }

        private static string GetCodeShareFlag(string flightTypeIndicator, string operatingCarrierCode, string marketingCarrierCode)
        {
            if (flightTypeIndicator.Is("Y") || flightTypeIndicator.Is("N"))
            {
                return flightTypeIndicator.ToUpperInvariant();
            }

            return operatingCarrierCode.Is(marketingCarrierCode) ? "N" : "Y";
        }

        private static void AddCredit(List<Creditdetail> credits, int value, string pointType, string pointSubType, string qualifyingFlag, string user, DateTime now)
        {
            if (value <= 0)
            {
                return;
            }

            credits.Add(new Creditdetail
            {
                CreditItemId = Utility.GetId(),
                CreatedBy = user,
                CreatedOn = now.ToString(Constants.Application.ISODateTimeFormat),
                LastUpdatedBy = user,
                LastUpdatedOn = now.ToString(Constants.Application.ISODateTimeFormat),
                CreditValue = value.ToString(),
                PointType = pointType,
                PointSubType = pointSubType,
                QualifyingFlag = qualifyingFlag
            });
        }
    }
}
