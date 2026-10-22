using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.TicketingProcessV3Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;
using static VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange.FlightItems;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange
{
    public class SegmentMapper
    {
        public static List<Segment> Air4ToOrderChangeSegmentMapper(PerformRevenueResponseType performRevenueResponse, OrderChangeRequestData request)
        {
            List<Segment> segments = new();
            foreach (var sd in performRevenueResponse.Pnr.SegmentDl)
            {
                foreach (var oa in request.OrderAmendments)
                {
                    var seg = oa.FlightItems.Where(x => x.ActionContext == ActionContexts.Add || x.ActionContext == ActionContexts.Retain);
                    foreach (var item in seg)
                    {
                        if (item.Segments.OriginAirportCode == sd.DeparturePoint && item.Segments.DestinationAirportCode == sd.ArrivalPoint)
                        {
                            segments.Add(new()
                            {
                                OriginDestId = item.Segments.OriginDestId,
                                SegmentId = sd.SegmentNumber,
                                MarketingCarrier = sd.AirlineCode,
                                MarketingFlightNumber = sd.FlightNumber,
                                OperatingCarrier = item.Segments.OperatingCarrier,
                                OperatingFlightNumber = item.Segments.OperatingFlightNumber,
                                OriginAirportCode = sd.DeparturePoint,
                                DestinationAirportCode = sd.ArrivalPoint,
                                ScheduledDepartureTime = DateTimeHelper.Air4DepartDateFormatConversion(sd.DepartureDate, sd.SegmentDepartureYear, sd.DepartureTime),
                                ScheduledArrivalTime = item.Segments.ScheduledArrivalTime,
                                ClassOfService = sd.ClassOfService
                            });
                        }
                    }
                }
            }
            return segments;
        }
    }
}
