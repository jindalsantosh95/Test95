using System;
using System.Linq;
using System.Threading.Channels;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Proxy.SeatV8Proxy;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Repositories.SeatAvailability;
using Environment = VAA.EAPI.Microservices.Retail.Seat.Constants.Environment;

namespace VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Mapper
{
    public static class SeatV8ReqMapper
    {
        public static RetrievePassengerMapRequestType MapRequest(SeatV8RetrievePassengerMapQuery request)
        {
            if (request.ChannelInfo == null) throw new ArgumentNullException(nameof(request.ChannelInfo));

            var flightSegment = request.FlightSegments.FirstOrDefault();
            var travellerInfo = request.Customers.IsNullOrEmpty() ? null :
                request.Customers.Select(custInfo => new TravelerInfoType()
                {
                    AssociationID = custInfo.CustId,
                    PaxType = custInfo.Ptc,
                    LoyaltyInfo = custInfo.LoyaltyMemberDetails?.MemberNumber.IsNullOrEmpty() == false && custInfo.LoyaltyMemberDetails?.ProgramCode.IsNullOrEmpty() == false ? new[] { new LoyaltyInfoType() { LoyaltyAccountNumber = custInfo.LoyaltyMemberDetails?.MemberNumber, LoyaltyAirlineCode = custInfo.LoyaltyMemberDetails?.ProgramCode } } : null
                });

            return new()
            {
                RequestInfo = new()
                {
                    ApplicationId = request.ChannelInfo.DlApplicationId,
                    DebugOptions = ConfigReader.Get(Environment.DebugOptions),
                    HostContextParams = new HostContextParamsType { PoolType = request.ChannelInfo.DlPoolType },
                    TestLab = ConfigReader.Get(Environment.TestLab)
                },
                RecordLocator = request.RecordLocator ?? null,
                TravelerInfo = !travellerInfo.IsNullOrEmpty() ? travellerInfo.ToArray() : null,
                CurrencyCode = request.CurrencyCode,
                FlightInfo = new FlightInfoType
                {
                    AssociationID = "01",
                    SegmentNumber = flightSegment?.SegmentNumber,
                    DepartureAirportCd = flightSegment?.OriginAirportCode,
                    ArrivalAirportCd = flightSegment?.DestinationAirportCode,
                    MarketingCarrierCode = flightSegment?.MarketingCarrier,
                    OperatingCarrierCode = flightSegment?.OperatingCarrier,
                    MarketingFlightNumber = flightSegment?.MarketingFlightNumber,
                    OperatingFlightNumber = flightSegment?.OperatingFlightNumber,
                    DepartureDate = DateTime.Parse(flightSegment?.ScheduledDepartureTime.LocalDateTime).ToString("yyyy-MM-dd"),
                    ClassOfService = flightSegment?.ClassOfService,
                    FareBasisCode = flightSegment?.FareBasis,
                    FlightDepartureTime = flightSegment?.ScheduledDepartureTime.LocalDateTime.Split('T').Last()[..5]
                }
            };
        }

        public static DeleteRequestType MapDeleteRequest(OrderSeatV8DeleteSeatCommand request, Domain.Channel channelInfo = null)
        {
            return new()
            {
                DeleteSeatInfo = request.OrderAmendments.Select((oi, index) =>
                {
                    var cust = request.CustomerDetails.First(x => x.CustId == oi.CustomerReference.First());
                    return new DeleteSeatInfoType()
                    {
                        AssociationId = Convert.ToString(index+ 1).PadLeft(2,'0'),
                        FlightReferenceID = oi.FlightItems.First().Segments.SegmentId,
                        TravelerReferenceID = $"{cust.LastNameNumber}-{cust.FirstNameNumber}"
                    };
                }).ToArray(),
                FlightInfo = request.OrderAmendments.SelectMany(x => x.FlightItems).Select(x => x.Segments).Select((s , index) =>
                {
                    return new FlightInfoType()
                    {
                        DepartureAirportCd = s.OriginAirportCode,
                        DepartureDate = Convert.ToDateTime(s.ScheduledDepartureTime).ToString("ddMMM"),
                        ArrivalAirportCd = s.DestinationAirportCode,
                        AssociationID = Convert.ToString(index + 1).PadLeft(2, '0'),
                        ClassOfService = s.ClassOfService,
                        OperatingCarrierCode = s.OperatingCarrier,
                        OperatingFlightNumber = s.OperatingFlightNumber,
                        MarketingCarrierCode = s.MarketingCarrier,
                        MarketingFlightNumber = s.MarketingFlightNumber,
                        SegmentNumber = s.SegmentId.Replace("SEG","").PadLeft(2,'0')
                    };
                }).ToArray(),
                RequestInfo = request.ChannelId  == Constants.Application.NdcChannel ? new()
                {
                    AppChannelName = "NCD", 
                    ApplicationId = "ND",
                    Location = request.Pos.CityCode,
                    TestLab = ConfigReader.Get(Environment.TestLab),
                    TransactionId = request.Rloc,
                    SessionId = request.SessionId,
                    HostContextParams = new() { PoolType = Constants.Application.PoolType },
                    
                } : new()
                {
                    AppChannelName = channelInfo.ChannelName,
                    ApplicationId = channelInfo.DlApplicationId,
                    Location = request.Pos.CityCode,
                    TestLab = ConfigReader.Get(Environment.TestLab),
                    TransactionId = request.Rloc,
                    SessionId = request.SessionId,
                    HostContextParams = new() { PoolType = channelInfo.DlPoolType },

                },
                TravelerInfo = request.CustomerDetails.Select(c => new TravelerInfoType()
                {
                    AssociationID = c.LastNameNumber,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    FirstNameNumber = c.FirstNameNumber,
                    LastNameNumber = c.LastNameNumber,
                    PaxType = c.Ptc
                }).ToArray()
            };
        }
    }
}
