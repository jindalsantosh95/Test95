using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Reflection;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Operations.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Operations.Infrastructure.Services;

namespace VAA.EAPI.Microservices.Operations
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddCommandLine(args);
                    builder.AddEnvironmentConfiguration();
                })
                .ConfigureFunctionsWorkerDefaults(worker =>
                {
                    worker.AddDefaultMiddleware();
                    worker.UseWhen<AuthenticationMiddleware>(x => !Debugger.IsAttached);
                    worker.UseWhen<AuthorizationMiddleware>(x => !Debugger.IsAttached);
                })
                .ConfigureOpenApi()
                .ConfigureServices(services =>
                {
                    services.AddDefaultServices();
                    services.AddSingleton<IHttpConnector, HttpConnector>();
                    services.AddSingleton<IAirportInfoProvider, AirportInfoProvider_DLWeb>();
                    services.AddSingleton<IMemoryCache, MemoryCache>();
                    services.AddSingleton<IFileSystem, FileSystem>();
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(Application.Query.AirportInfoQuery));
                    });
                })
                .Build();

            return host.RunAsync();
        }
    }
}
