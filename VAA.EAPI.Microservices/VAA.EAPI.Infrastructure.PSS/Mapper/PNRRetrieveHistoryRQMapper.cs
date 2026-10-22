using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;

namespace VAA.EAPI.Infrastructure.PSS.Mapper
{
    public class PNRRetrieveHistoryRQMapper
    {
        public static RetrieveRequestType MapToPnrRetrieveRequest(string rLOC)
        {
            return new RetrieveRequestType()
            {
                SetupContextRequest = new SetupContextRequestType()
                {
                    RequestInfo = new RequestInfoType()
                    {
                        ApplicationId = ConfigReader.Get(Constants.Environment.ApplicationId),
                        DebugOptions = ConfigReader.Get(Constants.Environment.DebugOptions),
                        TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                        HostContextParams = new HostContextParamsType()
                        {
                            PoolType = ConfigReader.Get(Constants.Environment.PoolType)
                        }
                    },
                    RetrieveInfo = new RetrieveInfoType()
                    {
                        PassengerInfo = new PassengerInfoType()
                        {
                            RecordLocator = rLOC
                        },
                        PnrRequestParams = new PnrRequestParamsType()
                        {
                            HistoryOnFlag = ConfigReader.Get(Constants.Environment.HistoryOnFlag)
                        }
                    },

                    StampingInfo = new StampingInfoType()
                    {
                        UserAuthorizationData = new UserAuthorizationDataType()
                        {
                            AgentCityCode = ConfigReader.Get(Constants.Environment.AgentCityCode),
                            RoleIndicator = ConfigReader.Get(Constants.Environment.RoleIndicator),
                            SecurityId = ConfigReader.Get(Constants.Environment.SecurityId)
                        }
                    }
                }
            };
        }
    }
}
