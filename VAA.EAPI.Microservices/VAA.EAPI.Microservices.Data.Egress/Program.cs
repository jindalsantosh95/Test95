using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Microservices.Data.Egress.Constants;

namespace VAA.EAPI.Microservices.Data.Egress
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
                    worker.UseMiddleware<FunctionContextAccessorMiddleware>();
                    worker.UseMiddleware<ExceptionHandlerMiddleware>();
                })
                .ConfigureOpenApi()
                .ConfigureServices(services =>
                {
                    services.AddDefaultServices();
                    services.AddStackExchangeRedisCache(options => options.Configuration = ConfigReader.Get(Environment.RedisConnectionString));
                    services.AddSingleton(typeof(IDistributedCacheService), typeof(DistributedCacheService));
                    services.AddMediatR(config => config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
                })
                .Build();

            return host.RunAsync();
        }
    }
}
