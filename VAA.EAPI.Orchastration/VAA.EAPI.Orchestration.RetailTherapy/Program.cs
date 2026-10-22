using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Aws;
using VAA.EAPI.Orchestration.RetailTherapy.Infrastructure.Interfaces;
using VAA.EAPI.Orchestration.RetailTherapy.Infrastructure.Services;

namespace VAA.EAPI.Orchestration.RetailTherapy
{
    public sealed class Program
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
                        })
                        .ConfigureOpenApi()
                        .ConfigureServices(services =>
                        {
                            services.AddDefaultServices();
                            services.AddSingleton<IHttpConnector, HttpConnector>();
                            services.AddSingleton<IAwsS3Buckets, AwsS3Buckets>();
                            services.AddSingleton<ICrewMasterService, CrewMasterService>();
                            services.AddSingleton<IFlightCrewRosterService, FlightCrewRosterService>();
                            services.AddSingleton<IFlightScheduleConsolidatedService, FlightScheduleConsolidatedService>();
                            services.AddSingleton<IFlightScheduleECommerceService, FlightScheduleECommerceService>();
                        })
                        .Build();

            return host.RunAsync();
        }
    }
}