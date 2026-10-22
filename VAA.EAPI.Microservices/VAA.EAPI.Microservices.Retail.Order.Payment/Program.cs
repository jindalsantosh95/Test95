using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.HealthChecks;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.IFG;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Services.CjsCheckout;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Query.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Constants;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.CjsCheckout;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Air4TokenGenerator;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.MmbQuery;

namespace VAA.EAPI.Microservices.Retail.Order.Payment
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
                    .AddTypeActivatedCheck<ServiceBusHealthCheck>(name: "SORServiceBusCheck:AzureServiceBus:ServiceBusConnectivity",
                                        failureStatus: HealthStatus.Unhealthy,
                                        tags: new[] { nameof(ServiceBusHealthCheck), "Service Bus Api" },
                                        args: new object[]
                                        {
                                            ConfigReader.Get(Constants.Environment.ServiceBusConnectionString)
                                        })
                    .AddTypeActivatedCheck<CosmosHealthCheck>(name: "OrderCosmosCheck:AzureCosmosDb:CosmosConnectivity",
                                        failureStatus: HealthStatus.Unhealthy,
                                        tags: new[] { nameof(CosmosHealthCheck), "Cosmos Check" },
                                        args: new object[]
                                        {
                                            ConfigReader.Get(Constants.Environment.PaymentDBEndpoint),
                                            ConfigReader.Get(Constants.Environment.PaymentDatabaseName),
                                            ConfigReader.Get(Constants.Environment.PaymentCollection)
                                        });
                services.AddSingleton<ISplitPnrPaymentCommand, SplitPnrPaymentCommandHandler>();
                services.AddSingleton<PaymentDocRepoQueryHandler>();
                services.AddSingleton<CosmosRepoCommandHandler>();
                services.AddSingleton<ICosmosClientFactory, CosmosClientFactory>();
                services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                services.AddSingleton(typeof(IAzureServiceBusTopicClient), typeof(AzureServiceBusTopicClient));
                services.AddSingleton(new ServiceBusClient(ConfigReader.Get(Environment.ServiceBusConnectionString), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }));
                services.AddSingleton<IHttpConnector, HttpConnector>();
                services.AddSingleton<IAir4TokenGenerator, Air4TokenGenerator>();
                services.AddSingleton<ICjsCheckoutPaymentAppGateway, CjsCheckoutPaymentAppGateway>();
                services.AddSingleton<ICjsCheckoutPaymentApp, CjsCheckoutPaymentApp>();
                services.AddSingleton<IMmbQueryLoginGateway, MmbQueryLoginGateway>();
                services.AddSingleton<IVredToken, VredToken>();
                services.AddSingleton<ISynchronyAccessTokenGenerator, SynchronyAccessTokenGenerator>();
                services.AddSingleton<ISynchronyPayloadCryptoService, SynchronyPayloadCryptoService>();
                services.AddSingleton<IMessageBusFactory, AzureServiceBusFactory>();
                services.AddSingleton<ICachedChannel, CachedChannel>();
                services.AddSingleton<ICreatePaymentDocumentCommand, CreatePaymentDocumentCommandHandler>();
                services.AddSingleton<EventStoreRequestDeleteCommandHandler>();
                services.AddSingleton<EventStoreRequestInsertCommandHandler>();
                services.AddSingleton<ServiceBusTopicMessageEventCommandHandler>();
                services.AddSingleton<ServiceResponseInsertCommandHandler>();
                services.AddSingleton<ICreatePaymentDocumentQuery, CreatePaymentDocumentQueryHandler>();
                services.AddSingleton<ICreatePaymentDocumentEvent, CreatePaymentDocumentEventHandler>();
                services.AddSingleton<ServiceResponseQueryHandler>();
                services.AddSingleton<CreatePaymentDocRepoCommandHandler>();
                services.AddSingleton<IAzureBlobService>((serviceProvoder) =>
                {
                    // lets preload the blobs
                    var countryBlobUri = AzureBlobService.CreateBlobUri(ConfigReader.Get(Constants.Environment.CpdBlobStorageUri), ConfigReader.Get(Constants.Environment.CpdContainerName), ConfigReader.Get(Constants.Environment.CpdCountryBlob), ConfigReader.Get(Constants.Environment.CpdBlobStorageSaSKey));
                    var azureBlobService = new AzureBlobService();

                    Task.WaitAll(azureBlobService.Download(countryBlobUri, ConfigReader.Get(Constants.Environment.CpdCountryBlob), CancellationToken.None));

                    return azureBlobService;
                });
                services.AddMediatR(config =>
                {
                    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                    config.RegisterServicesFromAssemblyContaining(typeof(EventRequestInsertCommand));
                });
            })
            .Build();

            return host.RunAsync();
        }
    }
}
