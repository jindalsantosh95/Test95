using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SSRToPNR;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.SSRToPNR
{
    public class SSRtoPNREventDataMapper
    {
        public static FQTVSSRRequestData MemberDetailsToFQTVSSREventDataMapper(MemberDetailsToAddSSR memberDeatils)
        {
            if (memberDeatils == null) throw new ArgumentNullException(nameof(memberDeatils));

            List<SpecialServiceRequestInfo> ssrs = new()
            {
                new SpecialServiceRequestInfo()
                {
                    AirlineCode = Constants.Application.PartnerCode,
                    AllPassengersFlag = Constants.Application.AddSSRPassengersFlag,
                    AllSegmentsFlag = Constants.Application.AddSSRSegmentsFlag,
                    FirstName = memberDeatils.FirstName,
                    LastName = memberDeatils.LastName,
                    FreeformText = memberDeatils.MemberNumber,
                    RequestCode = Constants.Application.AddSSRRequestCode,
                    StatusCode = Constants.Application.AddSSRStatusCode,
                    ProgramCode = Constants.Application.PartnerCode
                }
            };
            List<OrderAmendment> orderAmendments = new()
            {
                new OrderAmendment()
                {
                    ProductType = Constants.Application.AddSSRProductType,
                    SpecialServiceRequestInfo = ssrs
                }
            };

            return new FQTVSSRRequestData()
            {
                Pos = new Pos { CityCode = Constants.Application.SeatUptakePos },
                Rloc = memberDeatils.PNR,
                ChannelId = ConfigReader.Get(Constants.Environment.ChannelIdToAddSSR),
                OrderAmendments = orderAmendments
            };
        }
    }
}
