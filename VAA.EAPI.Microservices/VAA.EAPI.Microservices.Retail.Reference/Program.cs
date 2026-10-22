using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Retail.Reference.Application;
using VAA.EAPI.Microservices.Retail.Reference.Application.Command;
using VAA.EAPI.Microservices.Retail.Reference.Application.Interface;
using VAA.EAPI.Microservices.Retail.Reference.Application.Query;
using VAA.EAPI.Microservices.Retail.Reference.Domain.Interface;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Interface;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Models;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Repository;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Services;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Services;
using Environment = VAA.EAPI.Microservices.Retail.Reference.Constants.Environment;

namespace VAA.EAPI.Microservices.Retail.Reference
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
                    services.AddDbContext<ScvReferenceDbContext>(options => options.UseSqlServer(ConfigReader.Get(Environment.SCVCommonRefDbConnString)));
                    services.AddScoped<IScvRefDbService, ScvRefDbService>();
                    services.AddScoped<IScvRefDBRepository, ScvRefDBRepository>();
                    services.AddStackExchangeRedisCache(options => options.Configuration = ConfigReader.Get(Environment.RedisConnectionString));
                    services.AddSingleton(typeof(IDistributedCacheService), typeof(DistributedCacheService));
                    services.AddSingleton(new BlobServiceClient(new Uri(ConfigReader.Get(Environment.BlobStorageUri)), new DefaultAzureCredential()));
                    services.AddSingleton<ICacheService, CacheService>();
                    services.AddSingleton(typeof(IHttpConnector), typeof(HttpConnector));
                    services.AddSingleton<IAzureBlobService, AzureBlobService>();
                    services.AddScoped<IDistanceAndCountryCodeQueryHandler, DistanceAndCountryCodeQueryHandler>();
                    services.AddScoped<ICurrencyConversionCommand, CurrencyConversionCommandHandler>();
                    services.AddScoped<ICurrencyConversionService, CurrencyConversionService>();
                    services.AddScoped<ICurrencyExchangeRateQuery, CurrencyExchangeRateQueryHandler>();
                    services.AddSingleton<ReferenceDataCommandHandler>();
                    services.AddScoped<IMobileFeaturesQueryHandler, MobileFeaturesQueryHandler>();
                    services.AddMediatR(config =>
                    {
                        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                    });
                })
                .Build();

            return host.RunAsync();
        }
    }
}
