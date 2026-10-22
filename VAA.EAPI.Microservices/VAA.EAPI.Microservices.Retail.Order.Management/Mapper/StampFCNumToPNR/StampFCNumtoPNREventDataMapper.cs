using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.StampFCNumberToPNR;
using static VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange.OrderAmendment;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Mapper.StampFCNumToPNR
{
    public class StampFCNumtoPNREventDataMapper
    {
        public static OrderChangeRequestData MemberDetailsToFQTVSSREventDataMapper(FCStampingDetails memberDetails)
        {
            if (memberDetails == null) throw new ArgumentNullException(nameof(memberDetails));

            List<SpecialServiceRequestInfo> ssrs = new()
            {
                new SpecialServiceRequestInfo()
                {
                    AirlineCode = Constants.Application.VSCarrierCode,
                    AllPassengersFlag = Constants.Application.AddSSRPassengersFlag,
                    AllSegmentsFlag = Constants.Application.AddSSRSegmentsFlag,
                    FirstName = memberDetails.FirstName,
                    LastName = memberDetails.LastName,
                    FreeformText = memberDetails.MemberNumber,
                    RequestCode = Constants.Application.AddSSRRequestCode,
                    StatusCode = Constants.Application.AddSSRStatusCode,
                    ProgramCode = Constants.Application.VSCarrierCode
                }
            };

            List<OrderAmendment> orderAmendments = new()
            {
                new OrderAmendment()
                {
                    ProductType = Enum.Parse<ProductTypes>(Constants.Application.AddSSRProductType),
                    SpecialServiceRequestInfo = ssrs
                }
            };

            return new OrderChangeRequestData()
            {
                Pos = new Pos { CityCode = Constants.Application.SeatUptakePos },
                Rloc = memberDetails.PNR,
                ChannelId = ConfigReader.Get(Constants.Environment.ChannelIdToAddSSR),
                OrderAmendments = orderAmendments
            };
        }
    }
}
