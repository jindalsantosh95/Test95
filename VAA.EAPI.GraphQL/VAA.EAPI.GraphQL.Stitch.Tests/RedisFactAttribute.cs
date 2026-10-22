using VAA.EAPI.GraphQL.Stitch.Core.Extensions;
using VAA.EAPI.GraphQL.Stitch.Core.Helpers;
using Xunit;

namespace VAA.EAPI.GraphQL.Stitch.Tests
{
    public sealed class RedisFactAttribute : FactAttribute
    {
        public RedisFactAttribute()
        {
            if (ConfigReader.Try("RedisSubscriptionTestConnectionString").IsNullOrWhiteSpace())
            {
                Skip = "Set RedisSubscriptionTestConnectionString to run isolated Redis integration tests.";
            }
        }
    }
}
