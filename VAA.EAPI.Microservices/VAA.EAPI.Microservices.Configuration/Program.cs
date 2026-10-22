using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.HealthChecks;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Configuration.Application;
using VAA.EAPI.Microservices.Configuration.Application.QueriesAndHandlers;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Services;

namespace VAA.EAPI.Microservices.Configuration
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
                            .AddTypeActivatedCheck<TableStorageHealthCheck>(name: "ConfigurationStorageHealthCheck",
                                       failureStatus: HealthStatus.Unhealthy,
                                       tags: new[] { nameof(TableStorageHealthCheck) },
                                       args: new object[]
                                       {
                                            ConfigReader.Get(Constants.Environment.CurrencyDBKey),
                                            new List<string>()
                                            {
                                                ConfigReader.Get(Constants.Environment.PccCurrencyDBTableName),
                                                ConfigReader.Get(Constants.Environment.UpsellOffersDBTableName),
                                                ConfigReader.Get(Constants.Environment.BspMarketCurrencyDBTableName),
                                                ConfigReader.Get(Constants.Environment.BspMarketPaymentDBTableName),
                                                ConfigReader.Get(Constants.Environment.AirportListDBTableName),
                                                ConfigReader.Get(Constants.Environment.FareFamilyListDBTableName),
                                                ConfigReader.Get(Constants.Environment.PricingLocationDBTableName),
                                                ConfigReader.Get(Constants.Environment.ReshopFareFamilyDBTableName),
                                                ConfigReader.Get(Constants.Environment.CurrencyDBTableName),
                                                ConfigReader.Get(Constants.Environment.TtlRulesDBTableName),
                                                ConfigReader.Get(Constants.Environment.TtlEnabledCityDBTableName),
                                                ConfigReader.Get(Constants.Environment.CityCodeDBTableName)
                                            }
                                       }
                            );
                    services.AddSingleton<ICorporateData, CorporateDataService>();
                    services.AddSingleton<ICorporateDetails, CorporateQueryHandler>();
                    services.AddSingleton<ICurrencyDetails, CurrencyDetails>();
                    services.AddSingleton<IFareFamilyListService, FareFamilyListService>();
                    services.AddSingleton<IReshopFareFamilyService, ReshopFareFamilyService>();
                    services.AddSingleton<IUpsellOffersService, UpsellOffersService>();
                    services.AddSingleton<ILocationMappingService, LocationMappingService>();
                    services.AddSingleton<IAirportListService, AirportListService>();
                    services.AddSingleton<IBspMarketCurrencyDetails, BspMarketCurrencyDetails>();
                    services.AddSingleton<IitaFareTypeCodeData, ItaFareTypeCodeDetails>();
                    services.AddSingleton<IitaFareTypeCodeDetails, ItaFareTypeCodeQueryHandler>();
                    services.AddSingleton<ITtlRules, TtlQueryHandler>();
                    services.AddSingleton<ITtlRulesData, TtlRulesDataService>();
                    services.AddSingleton<ITtlEnabledCity, TtlEnabledCityQueryHandler>();
                    services.AddSingleton<ITtlEnabledCityData, TtlEnabledCityDataService>();
                    services.AddSingleton<IPccCurrencyDetails, PccCurrencyDetails>();
                    services.AddSingleton<IBspMarketPaymentDetails, BspMarketPaymentDetails>();
                    services.AddSingleton<ICityCodeQuery, CityCodeQueryHandler>();
                    services.AddSingleton<ICityCodeDetails, CityCodeDetails>();
                    services.AddSingleton<IAzureBlobService, AzureBlobService>();
                    services.AddSingleton(typeof(ITableStorageService<CurrencyDbEntity>), typeof(TableStorageService<CurrencyDbEntity>));
                    services.AddSingleton(typeof(ITableStorageService<ReshopFareFamilyDBEntity>), typeof(TableStorageService<ReshopFareFamilyDBEntity>));
                    services.AddSingleton(typeof(ITableStorageService<UpsellOffersDbEntity>), typeof(TableStorageService<UpsellOffersDbEntity>));
                    services.AddSingleton(typeof(ITableStorageService<LocationMapDbEntity>), typeof(TableStorageService<LocationMapDbEntity>));
                    services.AddSingleton(typeof(ITableStorageService<AirportListDBEntity>), typeof(TableStorageService<AirportListDBEntity>));
                    services.AddSingleton(typeof(ITableStorageService<FareFamilyListDbEntity>), typeof(TableStorageService<FareFamilyListDbEntity>));
                    services.AddSingleton(typeof(ITableStorageService<BspMarketCurrencyDbEntity>), typeof(TableStorageService<BspMarketCurrencyDbEntity>));
                    services.AddSingleton(typeof(ITableStorageService<BspMarketPaymentDbEntity>), typeof(TableStorageService<BspMarketPaymentDbEntity>));
                    services.AddSingleton(typeof(ITableStorageService<ItaFareTypeCodeDbEntity>), typeof(TableStorageService<ItaFareTypeCodeDbEntity>));
                    services.AddSingleton(typeof(ITableStorageService<PccCurrencyDbEntity>), typeof(TableStorageService<PccCurrencyDbEntity>));
                    services.AddSingleton(typeof(ITableStorageService<TtlRulesDbEntity>), typeof(TableStorageService<TtlRulesDbEntity>));
                    services.AddSingleton(typeof(ITableStorageService<TtlEnabledCityDbEntity>), typeof(TableStorageService<TtlEnabledCityDbEntity>));
                    services.AddSingleton(typeof(ITableStorageService<CityCodeDbEntity>), typeof(TableStorageService<CityCodeDbEntity>));
                    services.AddSingleton<ICacheService, CacheService>();
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                        config.RegisterServicesFromAssemblyContaining(typeof(CurrencyDBQueryHandler));
                        config.RegisterServicesFromAssemblyContaining(typeof(ReshopFareFamilyDBQueryHandler));
                        config.RegisterServicesFromAssemblyContaining(typeof(UpsellOffersDBQueryHandler));
                        config.RegisterServicesFromAssemblyContaining(typeof(PricingLocationDbQueryHandler));
                        config.RegisterServicesFromAssemblyContaining(typeof(FareFamilyQueryHandler)); 
                        config.RegisterServicesFromAssemblyContaining(typeof(TtlQueryHandler));
                        config.RegisterServicesFromAssemblyContaining(typeof(TtlEnabledCityQueryHandler));
                    });
                })
                .Build();

            return host.RunAsync();
        }
    }
}