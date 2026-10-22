using VAA.EAPI.Infrastructure.PSS.Proxy.HostContext;
using VAA.EAPI.Infrastructure.PSS.Repository.Command;

namespace VAA.EAPI.Infrastructure.PSS.Mapper
{
    public class HostContextMapper
    {
        public static CreateAndSwingRequestType CreateAndSwingRequestMapping(CreateAndSwingCityCommand createAndSwingCityCommand)
        {

            return new CreateAndSwingRequestType()
            {

                RequestInfo = new RequestInfoType()
                {
                    ApplicationId = createAndSwingCityCommand.Air4RequestContext.AppId,
                    DebugOptions = createAndSwingCityCommand.Air4RequestContext.DebugInfo,
                    TestLab = createAndSwingCityCommand.Air4RequestContext.TestLab,
                    Location = createAndSwingCityCommand.Air4RequestContext.Location,
                    SessionId = "",
                    HostContextParams = new HostContextParamsType()
                    {
                        City = createAndSwingCityCommand.PointOfSale,
                        PoolType = createAndSwingCityCommand.Air4RequestContext.PoolType,
                        TimeoutInactivity = createAndSwingCityCommand.Air4RequestContext.TimeoutInactivity,
                        TimeoutSession = createAndSwingCityCommand.Air4RequestContext.TimeoutSession,
                    }
                },

            };
        }

        public static CreateRequestType CreateRequestMapping(CreateAndSwingCityCommand createAndSwingCityCommand)
        {

            return new CreateRequestType()
            {

                RequestInfo = new RequestInfoType()
                {
                    AppChannelName = createAndSwingCityCommand.Air4RequestContext.AppChannelName,
                    ApplicationId = createAndSwingCityCommand.Air4RequestContext.AppId,
                    DebugOptions = createAndSwingCityCommand.Air4RequestContext.DebugInfo,
                    TestLab = createAndSwingCityCommand.Air4RequestContext.TestLab,
                },

            };
        }

        public static SwingCityDutyCodeRequestType SwingCityRequestMapping(CreateAndSwingCityCommand createAndSwingCityCommand, string sessionId)
        {

            return new SwingCityDutyCodeRequestType()
            {

                RequestInfo = new RequestInfoType()
                {
                    AppChannelName = createAndSwingCityCommand.Air4RequestContext.AppChannelName,
                    ApplicationId = createAndSwingCityCommand.Air4RequestContext.AppId,
                    DebugOptions = createAndSwingCityCommand.Air4RequestContext.DebugInfo,
                    TestLab = createAndSwingCityCommand.Air4RequestContext.TestLab,
                    SessionId = sessionId,
                    HostContextParams = new HostContextParamsType()
                    {
                        City = createAndSwingCityCommand.PointOfSale,
                    }
                },

            };
        }
    }
}


