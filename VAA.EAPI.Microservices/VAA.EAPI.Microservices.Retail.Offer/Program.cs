using MediatR;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.Graphql;
using VAA.EAPI.Infrastructure.HealthChecks;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.PSS.Proxy;
using VAA.EAPI.Infrastructure.PSS.Proxy.CreditCardV2;
using VAA.EAPI.Infrastructure.PSS.Proxy.PassengerV4Proxy;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Infrastructure.PSS.Proxy.RemarksV3;
using VAA.EAPI.Infrastructure.PSS.Proxy.SeatAvailability;
using VAA.EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy;
using VAA.EAPI.Infrastructure.PSS.Repository;
using VAA.EAPI.Infrastructure.PSS.Repository.Command;
using VAA.EAPI.Infrastructure.PSS.Repository.Query;
using VAA.EAPI.Infrastructure.PSS.Services;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.WcfServiceMessaging;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.FlightAvailability;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.ReshopForCancellation;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.ReshopForNameChange;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Services;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Delta.FlexiDateSearch;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Delta.FlightAvailability;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta.FlightAvailability;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Ita;
using VAA.EAPI.Microservices.Retail.Offer.Interfaces;
using VAA.EAPI.Microservices.Retail.Offer.Services;

namespace VAA.EAPI.Microservices.Retail.Offer
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
                        .AddTypeActivatedCheck<RedisHealthCheck>(name: "RedisCacheCheck",
                                        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                                        tags: new[] { nameof(RedisHealthCheck)},
                                        args: new object[]
                                        {
                                            ConfigReader.Get(Constants.Environment.RedisConnectionString)
                                        });
                    services.AddStackExchangeRedisCache(options => options.Configuration = ConfigReader.Get(Constants.Environment.RedisConnectionString));
                    services.AddSingleton(typeof(IBookingProcessV3), typeof(BookingProcessV3));
                    services.AddSingleton(typeof(IAir4PassengerV4), typeof(Air4PassengerV4));
                    services.AddSingleton(typeof(ISeatAvailabilityV8), typeof(SeatAvailabilityV8));
                    services.AddSingleton(typeof(ICreditCardV2), typeof(CreditCardV2));
                    services.AddSingleton(typeof(IAir4RemarksV3), typeof(Air4RemarksV3));
                    services.AddSingleton(typeof(IPNRCommonV6), typeof(PNRCommonV6));
                    services.AddSingleton(typeof(ITicketingProcessV3), typeof(TicketingProcessV3));
                    services.AddSingleton(s => new BookingProcessClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.BookingProcessV3EndPoint)))));
                    services.AddSingleton(s => new PassengerClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.Air4PassengerV4EndPoint)))));
                    services.AddSingleton(s => new SeatClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.Air4SeatV8EndPoint)))));
                    services.AddSingleton(s => new CreditCardClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.CreditCardV2EndPoint)))));
                    services.AddSingleton(s => new Remark1Client(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.RemarksV3EndPoint)))));
                    services.AddSingleton(s => new PnrCommonClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.Air4PnrCommonEndPoint)))));
                    services.AddSingleton(s => new TicketingProcessClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.TicketingProcessV3EndPoint)))));
                    services.AddSingleton(typeof(IItaService), typeof(ItaService));
                    services.AddSingleton<IRewardSeatCalendarService, RewardSeatCalendarService>();
                    services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));
                    services.AddSingleton<ICabinDetails, CabinDetails>();
                    services.AddSingleton<ICachedPriceClass, CachedPriceClass>();
                    services.AddSingleton<ICachedAmenities, CachedAmenities>();
                    services.AddSingleton<ICachedCabin, CachedCabin>();
                    services.AddSingleton<ICachedPieceAllowance, CachedPieceAllowance>();
                    services.AddSingleton<ICachedCabinBaggage, CachedCabinBaggage>();
                    services.AddSingleton<ICachedAirportDetails, CachedAirportDetails>();
                    services.AddSingleton(typeof(IDistributedCacheService), typeof(DistributedCacheService));
                    services.AddSingleton<IFareFamily, FareFamily>();
                    services.AddSingleton(typeof(IRetrieveAmenities), typeof(RetrieveAmenities));
                    services.AddSingleton(typeof(IHttpConnector), typeof(HttpConnector));
                    services.AddSingleton(typeof(IAir4TokenGenerator), typeof(Air4TokenGenerator));
                    services.AddSingleton<ICosmosClientFactory, CosmosClientFactory>();
                    services.AddSingleton(s => new EAPI.Infrastructure.PSS.Proxy.SchedulesAvailability.SchedulesAvailabilityClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.Air4SchedulesAvailabilityEndPoint)))));
                    services.AddSingleton(s => new EAPI.Infrastructure.PSS.Proxy.HostContext.HostContextClient(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.Air4HostContextEndPoint)))));
                    services.AddSingleton(typeof(IAir4HostContextV2), typeof(Air4HostContextV2));
                    services.AddSingleton<IAir4SchedulesAvailabilityV2, Air4SchedulesAvailabilityV2>();
                    services.AddSingleton(typeof(IDeltaOfferService), typeof(DeltaOfferService));
                    services.AddSingleton(typeof(IReshopNameChange), typeof(ReshopNameChangeQueryHandler));
                    // Due to using generics, GraphQL handlers need to be registered as services for mediatr to pick them up.
                    services.AddSingleton(typeof(IGraphQLService), typeof(GraphQLService));
                    services.AddSingleton(typeof(IDeltaGqlService), typeof(DeltaGqlService));
                    services.AddSingleton<IAzureBlobService>((serviceProvoder) =>
                    {
                        var cabinBlobUri = AzureBlobService.CreateBlobUri(ConfigReader.Get(Constants.Environment.ReferenceDataBlobStorageEndPoint), ConfigReader.Get(Constants.Environment.ReferenceDataBlobContainerName), ConfigReader.Get(Constants.Environment.CabinDataBlobName), ConfigReader.Get(Constants.Environment.ReferenceDataBlobSasKey));
                        var cabinBaggageBlobUri = AzureBlobService.CreateBlobUri(ConfigReader.Get(Constants.Environment.ReferenceDataBlobStorageEndPoint), ConfigReader.Get(Constants.Environment.ReferenceDataBlobContainerName), ConfigReader.Get(Constants.Environment.CabinBagAllowanceBlobName), ConfigReader.Get(Constants.Environment.ReferenceDataBlobSasKey));
                        var pieceAllowanceBlobUri = AzureBlobService.CreateBlobUri(ConfigReader.Get(Constants.Environment.ReferenceDataBlobStorageEndPoint), ConfigReader.Get(Constants.Environment.ReferenceDataBlobContainerName), ConfigReader.Get(Constants.Environment.PieceAllowanceBlobName), ConfigReader.Get(Constants.Environment.ReferenceDataBlobSasKey));
                        var priceClassBlobUri = AzureBlobService.CreateBlobUri(ConfigReader.Get(Constants.Environment.ReferenceDataBlobStorageEndPoint), ConfigReader.Get(Constants.Environment.ReferenceDataBlobContainerName), ConfigReader.Get(Constants.Environment.PriceClassBlobName), ConfigReader.Get(Constants.Environment.ReferenceDataBlobSasKey));
                        var fareFamilyBlobUri = AzureBlobService.CreateBlobUri(ConfigReader.Get(Constants.Environment.ReferenceDataBlobStorageEndPoint), ConfigReader.Get(Constants.Environment.ReferenceDataBlobContainerName), ConfigReader.Get(Constants.Environment.ItaFareFamilyBlobName), ConfigReader.Get(Constants.Environment.ReferenceDataBlobSasKey));

                        var azureBlobService = new AzureBlobService();

                        Task.WaitAll(
                            azureBlobService.Download(cabinBlobUri, ConfigReader.Get(Constants.Environment.CabinDataBlobName), CancellationToken.None),
                            azureBlobService.Download(cabinBaggageBlobUri, ConfigReader.Get(Constants.Environment.CabinBagAllowanceBlobName), CancellationToken.None),
                            azureBlobService.Download(pieceAllowanceBlobUri, ConfigReader.Get(Constants.Environment.PieceAllowanceBlobName), CancellationToken.None),
                            azureBlobService.Download(priceClassBlobUri, ConfigReader.Get(Constants.Environment.PriceClassBlobName), CancellationToken.None),
                            azureBlobService.Download(fareFamilyBlobUri, ConfigReader.Get(Constants.Environment.ItaFareFamilyBlobName), CancellationToken.None));

                        return azureBlobService;
                    });
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(PNRRetrieveHistoryRepoQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(FlightAvailabilityQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(FlightAvailabilityRepoQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(FlightAvailabilityServiceQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(FlightSchedulesAvailabilityQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(OrderReshopBundleQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(ReshopForCancellationQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(RetrieveAmenitiesQuery));
                        config.RegisterServicesFromAssemblyContaining(typeof(FlexiDateSearchQueryHandler));
                        config.RegisterServicesFromAssemblyContaining(typeof(CreateAndSwingCityCommand));
                    });
                })
                .Build();

            return host.RunAsync();
        }
    }
}
