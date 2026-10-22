using StackExchange.Redis;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services
{
    public static class AccountLinkingRedisConfiguration
    {
        public static ConfigurationOptions Read()
        {
            var connection = ConfigReader.Get(VAA.EAPI.Infrastructure.Constants.Environment.RedisConnectionString);
            var options = ConfigurationOptions.Parse(connection);
            options.Ssl = true;
            options.AbortOnConnectFail = false;
            return options;
        }
    }
}
