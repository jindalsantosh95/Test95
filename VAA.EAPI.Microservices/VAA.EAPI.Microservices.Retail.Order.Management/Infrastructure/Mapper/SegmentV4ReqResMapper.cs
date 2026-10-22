using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.SegmentV4Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain;
using Segment = VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange.Segment;


namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Mapper
{
    public class SegmentV4ReqResMapper
    {
        public static InsertRequestType InsertRequestTypeMapper(string sessionId, string transactionId, string location, int NumberInParty, List<Segment> segments, Channel channelInfo)
        {
            if (channelInfo == null) throw new ArgumentNullException(nameof(channelInfo));
            InsertRequestType insertRequestType = new InsertRequestType();
            insertRequestType.RequestInfo = new RequestInfoType()
            {
                ApplicationId = channelInfo.DlApplicationId,
                DebugOptions = ConfigReader.Get(Constants.Environment.DebugOptions),
                TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                SessionId = sessionId,
                Location = location,
                TransactionId = transactionId,
                HostContextParams = new HostContextParamsType { PoolType = channelInfo.DlPoolType }
            };

            int segmentnumber = 1;
            insertRequestType.SegmentInfo = (from segment in segments
                                             select new SegmentInfoType()
                                             {
                                                 AirlineCode = segment.MarketingCarrier,
                                                 ArrivalCity = segment.DestinationAirportCode,
                                                 ClassOfService = segment.ClassOfService,
                                                 Date = segment.ScheduledDepartureTime?.ToString("ddMMM"),
                                                 DepartureCity = segment.OriginAirportCode,
                                                 FlightNumber = segment.MarketingFlightNumber,
                                                 NumberInParty = NumberInParty.ToString(),
                                                 OpenFlag = ConfigReader.Get(Constants.Environment.SegmentV4InsertReqOpenFlag),
                                                 Status = ConfigReader.Get(Constants.Environment.SegmentV4InsertReqStatus),
                                                 SegmentNumber = Convert.ToString(segmentnumber),
                                                 SequenceNumber = Convert.ToString(segmentnumber++)
                                             }).ToArray();
            return insertRequestType;
        }
        public static DeleteRequestType DeleteRequestTypeMapper(string sessionId, string transactionId, string location, int NumberInParty, List<Segment> segments, Channel channelInfo)
        {
            if (channelInfo == null) throw new ArgumentNullException(nameof(channelInfo));
            DeleteRequestType deleteRequestType = new DeleteRequestType();
            deleteRequestType.RequestInfo = new RequestInfoType()
            {
                ApplicationId = channelInfo.DlApplicationId,
                DebugOptions = ConfigReader.Get(Constants.Environment.DebugOptions),
                TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                SessionId = sessionId,
                Location = location,
                TransactionId = transactionId,
                HostContextParams = new HostContextParamsType { PoolType = channelInfo.DlPoolType }
            };

            deleteRequestType.PnrRequestParams = new PnrRequestParamsType()
            {
                SegmentOffFlag = "false"
            };
            deleteRequestType.RequestOption = new RequestOptionType()
            {
                RequestOption1 = string.Empty
            };
            int segmentnumber = 0;
            deleteRequestType.SegmentInfo = (from segment in segments
                                             select new SegmentInfoType()
                                             {
                                                 SegmentNumber = Convert.ToString(++segmentnumber)
                                             }).ToArray();
            return deleteRequestType;
        }
        public static object InsertResponseTypeMapper(InsertResponseType insertResponseType)
        {
            //once Response structure is finalized, actual implementation will be done
            return insertResponseType;
        }
        public static object DeleteResponseTypeMapper(DeleteResponseType deleteResponseType)
        {
            //once Response structure is finalized, actual implementation will be done
            return deleteResponseType;
        }

    }
}
