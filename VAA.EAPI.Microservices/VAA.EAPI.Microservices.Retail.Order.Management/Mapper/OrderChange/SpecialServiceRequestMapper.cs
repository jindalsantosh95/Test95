using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.TicketingProcessV3Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange
{
    public class SpecialServiceRequestMapper
    {
        public static List<SpecialServiceRequestInfo> Air4ToOrderChangeSpecialServiceRequestMapper(PerformRevenueResponseType performRevenueResponse, OrderChangeRequestData request)
        {
            List<SpecialServiceRequestInfo> specialServiceRequestInfos = new();
            if (performRevenueResponse.Pnr.SpecialServiceRequest != null)
            {

                foreach (var sd in performRevenueResponse.Pnr.SpecialServiceRequest)
                {
                    specialServiceRequestInfos.Add(new SpecialServiceRequestInfo
                    {
                        AirlineCode = sd.AirlineCode,
                        AllPassengersFlag = sd.AllPassengersFlag,
                        AllSegmentsFlag = sd.AllSegmentsFlag,
                        FirstNameNumber = sd.FirstNameNumber,
                        LastNameNumber = sd.LastNameNumber,
                        SegmentNumber = sd.SegmentNumber,
                        RequestCode = sd.RequestCode,
                        StatusCode = sd.StatusCode
                    });
                }
            }
            return specialServiceRequestInfos;
        }
    }
}
