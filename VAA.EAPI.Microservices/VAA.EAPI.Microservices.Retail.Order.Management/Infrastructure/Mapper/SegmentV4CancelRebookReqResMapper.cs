using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.SegmentV4Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain;
using Segment = VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange.Segment;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Mapper
{
    public class SegmentV4CancelRebookReqResMapper
    {
        public static CancelAndRebookRequestType CancelRebookReqMapper(string sessionId, int NumberInParty, List<Segment> segmentsTobeCancel, List<Segment> segmentsTobeRebook, Channel channelinfo)
        {
            if (channelinfo == null) throw new ArgumentNullException(nameof(channelinfo));
            if (segmentsTobeCancel == null) throw new ArgumentNullException(nameof(segmentsTobeCancel));
            if (segmentsTobeRebook == null) throw new ArgumentNullException(nameof(segmentsTobeRebook));

            CancelAndRebookRequestType cancelAndRebookRequestType = new()
            {
                RequestInfo = new RequestInfoType()
                {
                    ApplicationId = channelinfo.DlApplicationId,
                    DebugOptions = ConfigReader.Get(Constants.Environment.DebugOptions),
                    TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                    SessionId = sessionId,
                    HostContextParams = new HostContextParamsType { PoolType = channelinfo.DlPoolType }
                },

                SegmentToBeCancelled = (from segment in segmentsTobeCancel
                                        select new SegmentInfoType()
                                        {
                                            AirlineCode = segment.MarketingCarrier,
                                            ArrivalCity = segment.DestinationAirportCode,
                                            ClassOfService = segment.ClassOfService,
                                            Date = segment.ScheduledDepartureTime?.ToString("ddMMM"),
                                            DepartureCity = segment.OriginAirportCode,
                                            FlightNumber = segment.MarketingFlightNumber,
                                            NumberInParty = Convert.ToString(NumberInParty),
                                            OpenFlag = ConfigReader.Get(Constants.Environment.SegmentV4InsertReqOpenFlag),
                                            Status = ConfigReader.Get(Constants.Environment.SegmentV4InsertReqStatus),
                                            SegmentNumber = Convert.ToString(segment.SegmentId),
                                            SequenceNumber = Convert.ToString(segment.SegmentId)
                                        }).ToArray(),

                SegmentToBeRebooked = (from segment in segmentsTobeRebook
                                       select new SegmentInfoType()
                                       {
                                           AirlineCode = segment.MarketingCarrier,
                                           ArrivalCity = segment.DestinationAirportCode,
                                           ClassOfService = segment.ClassOfService,
                                           Date = segment.ScheduledDepartureTime?.ToString("ddMMM"),
                                           DepartureCity = segment.OriginAirportCode,
                                           FlightNumber = segment.MarketingFlightNumber,
                                           NumberInParty = Convert.ToString(NumberInParty),
                                           OpenFlag = ConfigReader.Get(Constants.Environment.SegmentV4InsertReqOpenFlag),
                                           Status = ConfigReader.Get(Constants.Environment.SegmentV4InsertReqStatus),
                                           SegmentNumber = Convert.ToString(segment.SegmentId),
                                           SequenceNumber = Convert.ToString(segment.SegmentId)
                                       }).ToArray()
            };
            return cancelAndRebookRequestType;
        }

        public static IEnumerable<Segment> CancelRebookResMapper(CancelAndRebookResponseType cancelAndRebookResponse)
        {
            IEnumerable<Segment> segments = Array.Empty<Segment>();
            if (cancelAndRebookResponse != null && cancelAndRebookResponse.Pnr.SegmentDl.Length > 0)
            {
                segments = (from SegmentDl in cancelAndRebookResponse.Pnr.SegmentDl
                            select new Segment()
                            {
                                MarketingCarrier = SegmentDl.AirlineCode,
                                MarketingFlightNumber = SegmentDl.FlightNumber,
                                OriginAirportCode = SegmentDl.DeparturePoint,
                                DestinationAirportCode = SegmentDl.ArrivalPoint,
                                ClassOfService = SegmentDl.ClassOfService,
                                ScheduledDepartureTime = DateTimeHelper.Air4DepartDateFormatConversion(SegmentDl.DepartureDate, SegmentDl.SegmentDepartureYear, SegmentDl.DepartureTime),
                                SegmentId = SegmentDl.SegmentNumber
                            });
            }
            return segments;
        }
    }
}
