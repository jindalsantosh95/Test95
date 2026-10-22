using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Automation.Core.Azure.Functions.Configuration;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Utility.HttpMocker.Application;
using VAA.EAPI.Automation.Utility.HttpMocker.Infrastructure.Azure;
using VAA.EAPI.Automation.Utility.HttpMocker.Infrastructure.Middleware;

namespace VAA.EAPI.Automation.Utility.HttpMocker
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            var builder = FunctionsApplication.CreateBuilder(args)!;

            builder.Configuration.AddEnvironmentConfiguration();

            builder.ConfigureFunctionsWebApplication();

            builder.Services.AddApplicationInsightsTelemetryWorkerService()
                            .ConfigureFunctionsApplicationInsights()
                            .AddLogging()
                            .AddSingleton<IHttpClientFactory, Core.Http.HttpClientFactory>();

            var mode = ConfigReader.Try<MockStorageService.SourceType>("HttpMocker.MockStorageService.Source");

            if (mode == MockStorageService.SourceType.LocalStorage)
                builder.Services.AddSingleton<IMockStorageService>((provider) => new MockStorageService(@".\http", provider.GetRequiredService<ILogger<MockStorageService>>()));
            else
            {
                var container = ConfigReader.Try("HttpMocker.AzureBlobContainerService.Container");

                if (container.IsNullOrWhiteSpace())
                    throw new ArgumentNullException("HttpMocker.AzureBlobContainerService.Container");

                var connectionString = ConfigReader.Try("HttpMocker.AzureBlobContainerService.ConnectionString");
                var uri = ConfigReader.Try("HttpMocker.AzureBlobContainerService.Uri");

                if (connectionString.IsNullOrWhiteSpace() && uri.IsNullOrWhiteSpace())
                    throw new ArgumentNullException("HttpMocker.AzureBlobContainerService.ConnectionString/HttpMocker.AzureBlobContainerService.Uri");

                var sas = ConfigReader.Try("HttpMocker.AzureBlobContainerService.Sas");

                if (!connectionString.IsNullOrWhiteSpace())
                    builder.Services.AddSingleton<IAzureBlobContainerService>((s) => new AzureBlobContainerService(connectionString, container));
                else
                    builder.Services.AddSingleton<IAzureBlobContainerService>((s) => new AzureBlobContainerService(uri.Parse<Uri>(), container, sas));

                builder.Services.AddSingleton<IMockStorageService, MockStorageService>();
            }

            builder.UseMiddleware<MockResponseMiddleware>();

            return builder.Build().RunAsync();
        }
    }
}
