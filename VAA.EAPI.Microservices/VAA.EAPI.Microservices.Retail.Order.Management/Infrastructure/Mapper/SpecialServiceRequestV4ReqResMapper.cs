using System;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.SpecialServiceRequestV4Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Mapper
{
    public class SpecialServiceRequestV4ReqResMapper
    {
        public static AddRequestType AddRequestV4ReqMapper(string sessionId, string transactionId, string location, SpecialServiceRequestInfo specialServiceRequestInfo, Channel channelInfo)
        {
            if (channelInfo == null) throw new ArgumentNullException(nameof(channelInfo));
            AddRequestType addRequestType = new()
            {
                RequestInfo = new RequestInfoType()
                {

                    ApplicationId = channelInfo.DlApplicationId,
                    DebugOptions = ConfigReader.Get(Constants.Environment.DebugOptions),
                    TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                    SessionId = sessionId,
                    Location = location,
                    TransactionId = transactionId,
                    HostContextParams = new HostContextParamsType { PoolType = ConfigReader.Get(Constants.Environment.PoolType) }
                },
                SpecialServiceRequestInfo = new SpecialServiceRequestInfoType
                {
                    AirlineCode = specialServiceRequestInfo.AirlineCode,
                    NameNumberInfo = new NameNumberInfoType[] {
                       new NameNumberInfoType()
                       {
                            FirstName = specialServiceRequestInfo.FirstName,
                            LastName = specialServiceRequestInfo.LastName,
                            FirstNameNumber = specialServiceRequestInfo.FirstNameNumber,
                            LastNameNumber = specialServiceRequestInfo.LastNameNumber,
                       }
                    },
                    FreeformText = specialServiceRequestInfo.FreeformText,
                    NumberInParty = specialServiceRequestInfo.NumberInParty,
                    RecordNumber = specialServiceRequestInfo.RecordNumber,
                    RequestCode = specialServiceRequestInfo.RequestCode,
                    StatusCode = specialServiceRequestInfo.StatusCode,
                    AllPassengersFlag = specialServiceRequestInfo.AllPassengersFlag,
                    AllSegmentsFlag = specialServiceRequestInfo.AllSegmentsFlag,
                    ProgramCode = specialServiceRequestInfo.ProgramCode,
                }
            };

            if (specialServiceRequestInfo.SegmentNumber != null)
            {
                addRequestType.SpecialServiceRequestInfo.SegmentNumberInfo = new SegmentNumberInfoType[] {
                        new SegmentNumberInfoType()
                        {
                            SegmentNumber = specialServiceRequestInfo.SegmentNumber,
                        }
                };
            }

            return addRequestType;
        }
    }
}
