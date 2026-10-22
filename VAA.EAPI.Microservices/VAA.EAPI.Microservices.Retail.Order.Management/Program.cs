using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.PSS.Proxy.PassengerV4Proxy;
using VAA.EAPI.Infrastructure.PSS.Proxy.RemarksV3;
using VAA.EAPI.Infrastructure.PSS.Repository;
using VAA.EAPI.Infrastructure.PSS.Services;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.WcfServiceMessaging;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.SegmentV4Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.SpecialServiceRequestV4Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.TicketingProcessV3Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper.Caching;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories.CommonRefDB;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.CommonRefDB;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.CommonRefDB.Interface;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Management.Application.Services;
using VAA.EAPI.Infrastructure.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using VAA.EAPI.Infrastructure.Extentions;

namespace VAA.EAPI.Microservices.Retail.Order.Management
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
                    services.AddHealthChecks()
                        .AddTypeActivatedCheck<ServiceBusHealthCheck>(name: "SeatServiceBusCheck:AzureServiceBus:ServiceBusConnectivity",
                                            failureStatus: HealthStatus.Unhealthy,
                                            tags: new[] { nameof(ServiceBusHealthCheck), "Service Bus Api" },
                                            args: new object[]
                                            {
                                                ConfigReader.Get(Constants.Environment.SeatServiceBusConnectionString)
                                            })
                        .AddTypeActivatedCheck<ServiceBusHealthCheck>(name: "OrderServiceBusCheck:AzureServiceBus:ServiceBusConnectivity",
                                            failureStatus: HealthStatus.Unhealthy,
                                            tags: new[] { nameof(ServiceBusHealthCheck), "Service Bus Api" },
                                            args: new object[]
                                            {
                                                ConfigReader.Get(Constants.Environment.ServiceBusConnectionString)
                                            });
                    services.AddDbContext<CommonRefDbContext>(options => options.UseSqlServer(ConfigReader.Get(Constants.Environment.OneTicketCommonRefDBConnString)));
                    services.AddScoped<ICommonRefDBServices, CommonRefDBServices>();
                    services.AddScoped<ICommonRefDBRepository, CommonRefDBRepository>();
                    services.AddScoped<ICachedAirport, CachedAirport>();
                    services.AddScoped<ICachedCarrier, CachedCarrier>();
                    services.AddSingleton(typeof(IAzureServiceBusTopicClient), typeof(AzureServiceBusTopicClient));
                    services.AddSingleton(new ServiceBusClient(ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }));
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton(typeof(IAir4TicketingProcessV3), typeof(Air4TicketingProcessV3));
                    services.AddSingleton(typeof(IAir4PnrCommonV6), typeof(Air4PnrCommonV6));
                    services.AddSingleton(s => new PnrCommonClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.Air4PnrCommonEndPoint)))));
                    services.AddSingleton(s => new TicketingProcessClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.Air4TicketingProV3EndPoint)))));
                    services.AddSingleton(s => new SpecialServiceRequestClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SpecialServiceRequestV4EndPoint)))));
                    services.AddSingleton(s => new Remark1Client(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.RemarksV3EndPoint)))));
                    services.AddSingleton<IAir4RemarksV3, Air4RemarksV3>();
                    services.AddSingleton<ISpecialRemarksService, SpecialRemarksService>();
                    services.AddSingleton(typeof(ISpecialServiceRequestV4), typeof(SpecialServiceRequestV4));
                    services.AddSingleton(typeof(IAir4SegmentV4), typeof(Air4SegmentV4));
                    services.AddSingleton(s => new SegmentClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SegmentV4EndPoint)))));
                    services.AddSingleton<ICachedChannel, CachedChannel>();
                    services.AddSingleton<IOfferSearchItems, OfferSearchItems>();
                    services.AddSingleton<IHttpConnector, HttpConnector>();
                    services.AddSingleton(s => new PassengerClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.DeltaPassengerV4EndPoint)))));
                    services.AddSingleton(typeof(IAir4PassengerV4), typeof(Air4PassengerV4));
                    services.AddSingleton(typeof(IPNRCommonV6), typeof(PNRCommonV6));
                    services.AddSingleton(s => new EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy.PnrCommonClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.Air4PnrCommonEndPoint)))));
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(EventRequestInsertCommand));
                        config.RegisterServicesFromAssemblyContaining(typeof(PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(PNRSetupContextRepoQuery));
                    });
                })
                .Build();

            return host.RunAsync();
        }
    }
}
