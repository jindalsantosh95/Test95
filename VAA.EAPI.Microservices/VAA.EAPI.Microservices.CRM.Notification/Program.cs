using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Email.Client;
using VAA.EAPI.Infrastructure.Email.Interface;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.CRM.Notification.Application.Events;
using VAA.EAPI.Microservices.CRM.Notification.Infrastructure.Services;

namespace VAA.EAPI.Microservices.CRM.Notification
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
                })
                .ConfigureOpenApi()
                .ConfigureServices(services =>
                {
                    services.AddDefaultServices();                    
                    services.AddSingleton(typeof(INotificationEventCommand), typeof(NotificationEventCommandHandler));                   
                    services.AddSingleton(typeof(IAirshipService), typeof(AirshipService));
                    services.AddSingleton(typeof(IAircraftChangeNotifCommand), typeof(AircraftChangeNotifCommandHandler));
                    services.AddSingleton(typeof(IMessageCenterService), typeof(MessageCenterService));
                    services.AddSingleton<IACMClient, ACMClient>();                    
                    services.AddSingleton<ITokenHelper, TokenHelper>();
                    services.AddSingleton(typeof(IHttpConnector), typeof(HttpConnector));
                })
                .Build();

            return host.RunAsync();
        }
    }
}