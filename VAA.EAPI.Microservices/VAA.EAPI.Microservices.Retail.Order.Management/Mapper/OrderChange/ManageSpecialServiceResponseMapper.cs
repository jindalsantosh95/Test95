using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange
{
    public static class ManageSpecialServiceResponseMapper
    {
        public static OrderChangeResponse ResponseMapper(OrderChangeRequestData manageSpecialServiceRequestData)
        {
            return new OrderChangeResponse
            {
                Rloc = manageSpecialServiceRequestData.Rloc,
                OrderDetails = new()
                {
                    SpecialServiceRequestInfo = SpecialServiceRequestMapper(manageSpecialServiceRequestData)

                }
            };
        }

        private static IEnumerable<SpecialServiceRequestInfo> SpecialServiceRequestMapper(OrderChangeRequestData manageSpecialServiceRequestData)
        {

            return manageSpecialServiceRequestData.OrderAmendments.FirstOrDefault()?.SpecialServiceRequestInfo.Select(ssr => new SpecialServiceRequestInfo
            {
                AirlineCode = ssr.AirlineCode,
                AllPassengersFlag = ssr.AllPassengersFlag,
                AllSegmentsFlag = ssr.AllSegmentsFlag,
                FirstNameNumber = ssr.FirstNameNumber,
                LastNameNumber = ssr.LastNameNumber,
                SegmentNumber = ssr.SegmentNumber,
                RequestCode = ssr.RequestCode,
                StatusCode = ssr.StatusCode
            });
        }
    }
}
