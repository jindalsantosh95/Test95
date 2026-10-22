using System.Reflection;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace VAA.Facade.AzureB2C
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration(configurationBuilder =>
                {
                    configurationBuilder.AddCommandLine(args);                  
                })
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureOpenApi()
                .ConfigureServices(services =>
                {
                    services.AddHttpClient();
                    services.AddMediatR(Assembly.GetExecutingAssembly());
                    services.AddLogging();                   
                })               
                .Build();     
            host.Run();       
            return host.RunAsync();
        }
    }
}