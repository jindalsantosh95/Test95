using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange
{
    public class SegmentV4RequestMapper
    {
        public static IEnumerable<Segment> GetDeleteAllSegments(OrderChangeRequestData orderChangeRequestData)
        {
            // Since delete logic is not confirmed by Delta so keeping the below code as workaround for integration testing
            return from orderamendment in orderChangeRequestData.OrderAmendments
                   from flightitem in orderamendment.FlightItems
                   where flightitem.ActionContext == FlightItems.ActionContexts.Delete
                   select flightitem.Segments;

        }

        public static IEnumerable<Segment> GetInsertAllSegments(OrderChangeRequestData orderChangeRequestData)
        {
            // Since Insert logic is not confirmed by Delta so keeping the below code as workaround for integration testing
            return from orderamendment in orderChangeRequestData.OrderAmendments
                   from flightitem in orderamendment.FlightItems
                   where flightitem.ActionContext == FlightItems.ActionContexts.Add
                   select flightitem.Segments;
        }
    }
}
