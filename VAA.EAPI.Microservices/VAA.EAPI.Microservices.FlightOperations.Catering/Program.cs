using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.FlightOperations.Catering.Application.Interface;
using VAA.EAPI.Microservices.FlightOperations.Catering.Application.Service;
using VAA.EAPI.Microservices.FlightOperations.Catering.Services;

namespace VAA.EAPI.Microservices.FlightOperations.Catering
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
                services.AddSingleton(typeof(IHttpConnector), typeof(HttpConnector));
                services.AddSingleton<ICateringService, CateringService>();
                services.AddScoped<IPopsDbService, PopsDbService>();
                services.AddDbContext<PopsDataContext>(options => options.UseSqlServer(ConfigReader.Get(Constants.Environment.PopsSqlDbConnString)));
            })
            .Build();

        return host.RunAsync();
    }
}

}
