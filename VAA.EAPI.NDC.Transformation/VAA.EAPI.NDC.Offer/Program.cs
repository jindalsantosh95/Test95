using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.HealthChecks;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Offer.Application.Query.AirShopping;
using VAA.EAPI.NDC.Offer.Application.Query.OfferPrice;
using VAA.EAPI.NDC.Offer.Application.Query.OrderQuote;
using VAA.EAPI.NDC.Offer.Application.Query.OrderReshop;
using VAA.EAPI.NDC.Offer.Application.Query.OrderReshopCancel;
using VAA.EAPI.NDC.Offer.Application.Query.OrderReshopNameCorrection;
using VAA.EAPI.NDC.Offer.Application.Query.SeatAvailability;
using VAA.EAPI.NDC.Offer.Application.Query.ServiceList;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OfferPriceRS;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRS;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.ServiceListRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Interface;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.ConfigMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.SeatMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using VAA.EAPI.NDC.Offer.Validations.AirShopping;
using VAA.EAPI.NDC.Offer.Validations.OfferPrice;
using VAA.EAPI.NDC.Offer.Validations.SeatAvailability;
using VAA.EAPI.NDC.Offer.Validations.ServiceListValidations;
using AirShoppingRS = VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.AirShoppingRS;


namespace VAA.EAPI.NDC.Offer
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
                            .AddTypeActivatedCheck<HttpHealthCheck>(name: "OfferMSHealthCheck",
                                       failureStatus: HealthStatus.Unhealthy,
                                       tags: new[] { nameof(HttpHealthCheck) },
                                       args: new object[]
                                       {
                                            ConfigReader.Get(Constants.Environment.OfferMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.OfferMSSubscriptionKey)] }
                                            }
                        }).AddTypeActivatedCheck<HttpHealthCheck>(name: "ConfigMSHealthCheck",
                                       failureStatus: HealthStatus.Unhealthy,
                                       tags: new[] { nameof(HttpHealthCheck) },
                                       args: new object[]
                                       {
                                            ConfigReader.Get(Constants.Environment.ConfigMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.ConfigurationMSOcpKey)] }
                                            }
                        }).AddTypeActivatedCheck<HttpHealthCheck>(name: "ValidationMSHealthCheck",
                                       failureStatus: HealthStatus.Unhealthy,
                                       tags: new[] { nameof(HttpHealthCheck) },
                                       args: new object[]
                                       {
                                            ConfigReader.Get(Constants.Environment.ValidationMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.ValidationMSSubscriptionKey)] }
                                            }
                        }).AddTypeActivatedCheck<HttpHealthCheck>(name: "BrandMSHealthCheck",
                                       failureStatus: HealthStatus.Unhealthy,
                                       tags: new[] { nameof(HttpHealthCheck) },
                                       args: new object[]
                                       {
                                            ConfigReader.Get(Constants.Environment.BrandMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.BrandMSSubscriptionKey)] }
                                            }
                        }).AddTypeActivatedCheck<HttpHealthCheck>(name: "SeatMSHealthCheck",
                                       failureStatus: HealthStatus.Unhealthy,
                                       tags: new[] { nameof(HttpHealthCheck) },
                                       args: new object[]
                                       {
                                            ConfigReader.Get(Constants.Environment.SeatMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.SeatMSSubscriptionKey)] }
                                            }
                        }).AddTypeActivatedCheck<HttpHealthCheck>(name: "FlightServicesMSHealthCheck",
                                       failureStatus: HealthStatus.Unhealthy,
                                       tags: new[] { nameof(HttpHealthCheck) },
                                       args: new object[]
                                       {
                                            ConfigReader.Get(Constants.Environment.FlightServicesMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.FlightServicesMSSubscriptionKey)] }
                                            }
                        }).AddTypeActivatedCheck<HttpHealthCheck>(name: "OrderDataMSHealthCheck",
                                       failureStatus: HealthStatus.Unhealthy,
                                       tags: new[] { nameof(HttpHealthCheck) },
                                       args: new object[]
                                       {
                                            ConfigReader.Get(Constants.Environment.OrderDataMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.OrderDataMSSubscriptionKey)] }
                                            }
                        }).AddTypeActivatedCheck<HttpHealthCheck>(name: "Air4OrderSearchMSHealthCheck",
                                       failureStatus: HealthStatus.Unhealthy,
                                       tags: new[] { nameof(HttpHealthCheck) },
                                       args: new object[]
                                       {
                                            ConfigReader.Get(Constants.Environment.Air4OrderSearchMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.Air4OrderSearchMSSubscriptionKey)] }
                                            }
                        }).AddTypeActivatedCheck<HttpHealthCheck>(name: "OrderSearchMSHealthCheck",
                                       failureStatus: HealthStatus.Unhealthy,
                                       tags: new[] { nameof(HttpHealthCheck) },
                                       args: new object[]
                                       {
                                            ConfigReader.Get(Constants.Environment.OrderSearchMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.OrderSearchMSSubscriptionKey)] }
                                            }
                        }).AddTypeActivatedCheck<HttpHealthCheck>(name: "ReferenceMSHealthCheck",
                                       failureStatus: HealthStatus.Unhealthy,
                                       tags: new[] { nameof(HttpHealthCheck) },
                                       args: new object[]
                                       {
                                            ConfigReader.Get(Constants.Environment.ReferenceMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.ReferenceMSOcpKey)] }
                                            }
                        });
                            services.AddStackExchangeRedisCache(options => options.Configuration = ConfigReader.Get(Common.Constants.Environment.RedisCacheConnectionString));
                            services.AddSingleton(typeof(IHttpConnector), typeof(HttpConnector));
                            services.AddSingleton(typeof(IBrandMSPTCCommand), typeof(BrandMSCommandHandlerPTC));
                            services.AddSingleton(typeof(IBrandMSCommand), typeof(BrandMSCommandHandler));
                            services.AddSingleton<IMessageBusFactory, AzureServiceBusFactory>();
                            services.AddSingleton<IitaFareTypeCode, ItaFareTypeCodeQueryHandler>();
                            services.AddSingleton<ITtlRules, TtlRulesQueryHandler>();
                            services.AddSingleton<ITtlEnabledCity, TtlEnabledCityQueryHandler>();
                            services.AddSingleton<ICorporateQuery, CorporateQueryHandler>();
                            services.AddTransient<IGenericDataMapper, GenericDataMapper>();
                            services.AddTransient<IAirShoppingRequestValidations<AirShoppingRS.IATA_AirShoppingRS>, AirShoppingRequestValidations<AirShoppingRS.IATA_AirShoppingRS>>();
                            services.AddTransient<IOfferPriceValidations<IATA_OfferPriceRS>, OfferPriceValidations<IATA_OfferPriceRS>>();
                            services.AddTransient<IServiceListRequestValidations<IATA_ServiceListRS>, ServiceListRequestValidations<IATA_ServiceListRS>>();
                            services.AddTransient<IServiceListResponseValidation, ServiceListResponseValidation>();
                            services.AddTransient<ISeatAvailabilityValidations<IATA_SeatAvailabilityRS>, SeatAvailabilityValidations<IATA_SeatAvailabilityRS>>();
                            services.AddSingleton<ITrackAnalyticsEventCommandHandler, TrackAnalyticsEventCommandHandler>();
                            services.AddSingleton<IServiceBusTopicMessageEventCommandHandler, ServiceBusTopicMessageEventCommandHandler>();
                            services.AddMediatR(config =>
                            {
                                config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                                config.RegisterServicesFromAssemblyContaining(typeof(AirShoppingQuery));
                                config.RegisterServicesFromAssemblyContaining(typeof(OfferPriceQuery));
                                config.RegisterServicesFromAssemblyContaining(typeof(SeatAvailabilityQuery));
                                config.RegisterServicesFromAssemblyContaining(typeof(ServiceListQuery));
                                config.RegisterServicesFromAssemblyContaining(typeof(SeatMSQuery));
                                config.RegisterServicesFromAssemblyContaining(typeof(ValidationMSCommand));
                                config.RegisterServicesFromAssemblyContaining(typeof(TrackAnalyticsEventCommand));
                                config.RegisterServicesFromAssemblyContaining(typeof(ServiceBusTopicMessageEventCommand));
                                config.RegisterServicesFromAssemblyContaining(typeof(OrderReshopQuery));
                                config.RegisterServicesFromAssemblyContaining(typeof(OrderReshopCancelQuery));
                                config.RegisterServicesFromAssemblyContaining(typeof(OrderReshopNameCorrectionQuery));
                                config.RegisterServicesFromAssemblyContaining(typeof(OrderQuoteQuery));
                                config.RegisterServicesFromAssemblyContaining(typeof(ReshopFareFamilyQuery));
                                config.RegisterServicesFromAssemblyContaining(typeof(PricingLocationDbQuery));
                            });
                            services.AddHttpContextAccessor();
                        })
                        .Build();

            return host.RunAsync();
        }
    }
}