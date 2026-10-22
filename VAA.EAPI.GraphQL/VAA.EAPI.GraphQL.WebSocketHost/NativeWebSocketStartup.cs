using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

[assembly: HostingStartup(typeof(VAA.EAPI.GraphQL.WebSocketHost.NativeWebSocketStartup))]

namespace VAA.EAPI.GraphQL.WebSocketHost
{
    public sealed class NativeWebSocketStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {

            builder.ConfigureServices(services =>
            {
                services.AddSingleton(TransportOptions.FromEnvironment());
                services.TryAddEnumerable(ServiceDescriptor.Transient<IStartupFilter, WebSocketStartupFilter>());
            });
        }

        private sealed class WebSocketStartupFilter : IStartupFilter
        {
            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            {
                return app =>
                {
                    app.UseWebSockets();
                    app.UseMiddleware<NativeWebSocketMiddleware>();
                    app.UseMiddleware<NativeSseMiddleware>();
                    next(app);
                };
            }
        }
    }
}
