using System;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.AutoAccrual;
using Distance = VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models.Distance;
using FlyingClubMemberDocument = VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember.FlyingClubMember;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.RuleEngine
{
    public class EarnAirlineAccrualCalculatorRequest
    {
        public IEnumerable<FlightSegment> FlightSegments { get; set; }

        public IEnumerable<CustomerDetail> CustomerDetails { get; set; }

        public static explicit operator EarnAirlineAccrualCalculatorRequest((AutoAccrualRequest autoAccrualRequest, FlyingClubMemberDocument flyingClubMember, Distance distance) source)
        {
            ArgumentNullException.ThrowIfNull(source.autoAccrualRequest);
            ArgumentNullException.ThrowIfNull(source.flyingClubMember);
            ArgumentNullException.ThrowIfNull(source.distance);
            return new EarnAirlineAccrualCalculatorRequest
            {
                CustomerDetails = new[]
                {
                    new CustomerDetail
                    {
                        MemberNumber = source.flyingClubMember.MemberNumber,
                        TierCode = source.flyingClubMember.TierCode,
                        MemberType = source.flyingClubMember.MemberType,
                        MemberStatus = source.flyingClubMember.Status,
                    }
                },
                FlightSegments = new[]
                {
                    new FlightSegment
                    {
                        OriginAirportCode = source.autoAccrualRequest.FlightSegment.Origin,
                        DestinationAirportCode = source.autoAccrualRequest.FlightSegment.Destination,
                        OriginCountryCode = source.distance.OriginCountryCode,
                        DestinationCountryCode = source.distance.DestinationCountryCode,
                        SegmentDistance = source.distance.DistanceInMiles,
                        OperatingCarrier = source.autoAccrualRequest.FlightDetails.OperatingFlight.OperatingCarrierCode,
                        MarketingCarrier = source.autoAccrualRequest.FlightDetails.MarketingFlight.MarketingCarrierCode,
                        OperatingFlightNumber = source.autoAccrualRequest.FlightDetails.OperatingFlight.OperatingFlightNumber,
                        MarketingFlightNumber = source.autoAccrualRequest.FlightDetails.MarketingFlight.MarketingFlightNumber,
                        ScheduledDepartureTime = source.autoAccrualRequest.ScheduledDepartureDate,
                        CabinClass = source.autoAccrualRequest.BookingDetails.CabinClassCode,
                        ClassOfService = source.autoAccrualRequest.BookingDetails.CabinClassCode,
                        OperatingBookingClass = source.autoAccrualRequest.BookingDetails.OperatingCarrierBookingClass,
                        MarketingBookingClass = source.autoAccrualRequest.BookingDetails.MarketingCarrierBookingClass,
                        SalesLocation = string.Empty,
                        AgentIataNumber = string.Empty
                    }
                }
            };
        }
    }
}
