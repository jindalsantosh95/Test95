using Microsoft.Extensions.DependencyInjection;
using System;
using VAA.EAPI.GraphQL.Common.Helper;

namespace VAA.EAPI.GraphQL.FunctionHost.WebSockets
{
    public static class NativeWebSocketRegistration
    {
        public static IServiceCollection AddNativeWebSocketSubscriptions(this IServiceCollection services)
        {
            services.AddSingleton(_ => new RedisSubscriptionSessionStore(
                ConfigReader.Get(Common.Constants.Environment.RedisCacheConnectionString),
                ConfigReader.Get("NativeWebSocketsRedisPrefix")));
            services.AddSingleton<NativeSubscriptionRegistry>();

            services.AddHostedService(provider => provider.GetRequiredService<NativeSubscriptionRegistry>());

            return services;
        }
    }
}
