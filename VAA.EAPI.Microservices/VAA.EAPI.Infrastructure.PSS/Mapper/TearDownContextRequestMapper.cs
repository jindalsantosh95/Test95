using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;

namespace VAA.EAPI.Infrastructure.PSS.Mapper
{
    public static class TearDownContextRequestMapper
    {
        public static TeardownContextRequestType Mapper(string sessionId, bool isOrderEmail)
        {
            return new()
            {
                RequestInfo = new()
                {
                    ApplicationId = ConfigReader.Get(Constants.Environment.ApplicationId),
                    DebugOptions = ConfigReader.Get(Constants.Environment.DebugOptions),
                    TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                    SessionId = sessionId
                },
                EndRequest = isOrderEmail ? null : new() { RedisplayFlag = "Y" }
            };
        }
    }
}
