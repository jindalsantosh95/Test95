using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.ServiceModel;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreateSession;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.IgnoreTransaction;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.QueueAccess;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.SessionClose;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Infrastructure.Services;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt
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
                    services.AddHealthChecks();
                    services.AddSingleton(typeof(ISessionClose), typeof(SessionClose));
                    services.AddSingleton(typeof(IHttpConnector), typeof(HttpConnector));
                    services.AddSingleton(typeof(ICreateSession), typeof(CreateSession));
                    services.AddSingleton(typeof(IQueueAccess), typeof(QueueAccess));
                    services.AddSingleton(typeof(IQueueAccessCommand), typeof(QueueAccessCommandHandler));
                    services.AddSingleton(typeof(IGetReservation), typeof(GetReservation));
                    services.AddSingleton(typeof(IIgnoreTransaction), typeof(IgnoreTransaction));
                    services.AddSingleton<CreateSessionCommandHandler>();
                    services.AddSingleton<GetReservationQueryHandler>();
                    services.AddSingleton<IgnoreTransactionCommandHandler>();
                    services.AddSingleton<SessionCloseCommandHandler>();

                    services.AddSingleton(typeof(IDuplicatePnrRepoCommand), typeof(DuplicatePnrRepoCommandHandler));
                    services.AddSingleton(typeof(IRemoveDuplicatePnrRepoCommand), typeof(RemoveDuplicatePnrRepoCommandHandler));
                    services.AddSingleton(typeof(IPnrRepoService), typeof(PnrRepoService));
                    services.AddSingleton(s => new SessionCreatePortTypeClient(SessionCreatePortTypeClient.EndpointConfiguration.SessionCreatePortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new SessionClosePortTypeClient(SessionClosePortTypeClient.EndpointConfiguration.SessionClosePortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s=>new QueueAccessPortTypeClient(QueueAccessPortTypeClient.EndpointConfiguration.QueueAccessPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new GetReservationPortTypeClient(GetReservationPortTypeClient.EndpointConfiguration.GetReservationPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));
                    services.AddSingleton(s => new IgnoreTransactionPortTypeClient(IgnoreTransactionPortTypeClient.EndpointConfiguration.IgnoreTransactionPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))));

                })
                .Build();

            return host.RunAsync();
        }
    }
}
