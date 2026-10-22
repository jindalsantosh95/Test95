using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Net;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Caching;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Insights.Application.Providers;
using VAA.EAPI.Microservices.Insights.Interfaces;
using VAA.EAPI.Microservices.Insights.Models;
using VAA.EAPI.Microservices.Insights.Triggers.Http;

namespace VAA.EAPI.Microservices.Insights.Tests
{
    public sealed class ADOControllerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public ADOControllerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _serviceProvider ??= new ServiceCollection().AddLogging()
                                                        .Configure<WorkerOptions>(Program.ConfigureWorkerSerialization)
                                                        .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                        .AddSingleton<IHttpConnector, HttpConnector>()
                                                        .AddSingleton<IDistributedCache, DistributedCache>()
                                                        .AddSingleton<IDistributedCacheService, DistributedCacheService>()
                                                        .AddScoped<IOAuthTokenProvider, VAA.EAPI.Core.Xunit.Azure.OAuthTokenProvider>()
                                                        .AddScoped<IADODeploymentCompiler, ADODeploymentCompiler>()
                                                        .AddScoped<IADOPipelineProcessor, ADOPipelineProcessor>()
                                                        .AddSingleton<IMessageBusFactory, VAA.EAPI.Core.Xunit.ServiceBus.MessageBusFactory>()
                                                        .AddSingleton<IMessageBus, VAA.EAPI.Core.Xunit.ServiceBus.MessageBus>()
                                                        .AddScoped<ADOController>()
                                                        .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new VAA.EAPI.Core.Functions.TestFunctionContext() };
            AsyncContext.Current.Context.InstanceServices = _serviceProvider;
            ConfigReader.Set(Common.Constants.Environment.AADAuthority, "aad-auth");
            ConfigReader.Set(Common.Constants.Environment.AADTenant, "aad-tenant");
            ConfigReader.Set(Common.Constants.Environment.EapiMSSecret, "eapi-ms-secret");
        }


        [Fact]
        public async Task Test_ResetAndCompileAll()
        {
            ConfigReader.Set(VAA.EAPI.Microservices.Insights.Constants.Environment.DevOpsBaseUrl, "https://dev.azure.com/virginatlantic");
            ConfigReader.Set(VAA.EAPI.Microservices.Insights.Constants.Environment.DevOpsProject, "EAPI");
            ConfigReader.Set(VAA.EAPI.Microservices.Insights.Constants.Environment.DevOpsPAT, "test-pat");
            ConfigReader.Set(VAA.EAPI.Microservices.Insights.Constants.Environment.ADOPipelineServiceBus, "test-service-bus");

            var controller = _serviceProvider.GetRequiredService<ADOController>();

            var pipelineProcessingTasks = new ConcurrentBag<Task>();
            var messageBus = (VAA.EAPI.Core.Xunit.ServiceBus.MessageBus)_serviceProvider
                .GetRequiredService<IMessageBusFactory>()
                .GetClient("test-service-bus", VAA.EAPI.Microservices.Insights.Constants.Application.ADOPipelineTopic);

            messageBus.OnMessage += cloudEvent =>
            {
                var eventData = JObject.Parse(cloudEvent.Data?.ToString() ?? throw new InvalidOperationException("ADO compile event data is required."));
                var pipelineId = eventData["pipelineId"]!.Value<int>();
                var pipelineName = eventData["pipelineName"]!.Value<string>()!;

                pipelineProcessingTasks.Add(_serviceProvider.GetRequiredService<IADOPipelineProcessor>().HandleAsync(pipelineId, pipelineName));
            };

            var resetResponse = await controller.Reset(CreateRequest("http://localhost/api/deploymentstatus/reset", "POST"));
            Assert.Equal(HttpStatusCode.Accepted, resetResponse.StatusCode);

            var resetCollection = await WaitForDeploymentStatusAsync(controller, collection => collection is null);
            Assert.Null(resetCollection);

            var compileResponse = await controller.CompileDeploymentStatus(CreateRequest("http://localhost/api/deploymentstatus/compile", "POST"));
            Assert.Equal(HttpStatusCode.Accepted, compileResponse.StatusCode);

            await WaitForPipelineProcessingToStartAsync(pipelineProcessingTasks);
            await Task.WhenAll(pipelineProcessingTasks);

            var compiledCollection = await WaitForDeploymentStatusAsync(
                controller,
                collection => collection?.DeploymentStatuses.Any() == true);

            Assert.NotNull(compiledCollection);
            Assert.NotEmpty(compiledCollection.DeploymentStatuses);
            Assert.Single(compiledCollection.DeploymentStatuses);
            Assert.Equal("Insights - Synthetic Pipeline", compiledCollection.DeploymentStatuses.First().PipelineName);  // As per Mock File
            Assert.Equal(1001, compiledCollection.DeploymentStatuses.First().PipelineId);  // As per Mock File

        }

        [Fact]
        public async Task CompileDeploymentData_RemovesStoredPipelineStatusesThatAreNotInCurrentPipelineList()
        {
            ConfigReader.Set(VAA.EAPI.Microservices.Insights.Constants.Environment.DevOpsBaseUrl, "https://dev.azure.com/virginatlantic");
            ConfigReader.Set(VAA.EAPI.Microservices.Insights.Constants.Environment.DevOpsProject, "EAPI");
            ConfigReader.Set(VAA.EAPI.Microservices.Insights.Constants.Environment.DevOpsPAT, "test-pat");
            ConfigReader.Set(VAA.EAPI.Microservices.Insights.Constants.Environment.ADOPipelineServiceBus, "test-service-bus");

            var cachedCollection = new ADODeploymentCollection
            {
                LastUpdated = DateTime.UtcNow,
                DeploymentStatuses =
                [
                    new ADODeploymentStatus
                    {
                        PipelineId = 1001,
                        PipelineName = "Insights - Synthetic Pipeline",
                        LastUpdated = DateTime.UtcNow,
                    },
                    new ADODeploymentStatus
                    {
                        PipelineId = 9999,
                        PipelineName = "Deleted Pipeline",
                        LastUpdated = DateTime.UtcNow,
                    },
                ],
            };

            var cache = _serviceProvider.GetRequiredService<IDistributedCacheService>();
            await cache.Set(
                VAA.EAPI.Microservices.Insights.Constants.Application.DeploymentStatus,
                cachedCollection,
                false,
                TimeSpan.Zero,
                TimeSpan.FromDays(30));

            await _serviceProvider.GetRequiredService<IADODeploymentCompiler>().CompileDeploymentDataAsync();

            var compiledCollection = await cache.Get<ADODeploymentCollection>(VAA.EAPI.Microservices.Insights.Constants.Application.DeploymentStatus);

            Assert.NotNull(compiledCollection);
            Assert.Collection(
                compiledCollection.DeploymentStatuses,
                status =>
                {
                    Assert.Equal(1001, status.PipelineId);
                    Assert.Equal("Insights - Synthetic Pipeline", status.PipelineName);
                });
        }

        [Fact]
        public async Task DeploymentStatusCacheSave_StoresCollectionAndUpdatesTimestamp()
        {
            var cache = _serviceProvider.GetRequiredService<IDistributedCacheService>();
            var collection = new ADODeploymentCollection
            {
                LastUpdated = DateTime.MinValue,
                DeploymentStatuses =
                [
                    new ADODeploymentStatus
                    {
                        PipelineId = 1001,
                        PipelineName = "Insights - Synthetic Pipeline",
                        LastUpdated = DateTime.UtcNow,
                    },
                ],
            };

            var startedAt = DateTime.UtcNow;

            await ADODeploymentStatusCache.SaveAsync(cache, collection);

            var storedCollection = await cache.Get<ADODeploymentCollection>(VAA.EAPI.Microservices.Insights.Constants.Application.DeploymentStatus);

            Assert.NotNull(storedCollection);
            Assert.True(storedCollection.LastUpdated >= startedAt);
            Assert.Collection(
                storedCollection.DeploymentStatuses,
                status =>
                {
                    Assert.Equal(1001, status.PipelineId);
                    Assert.Equal("Insights - Synthetic Pipeline", status.PipelineName);
                });
        }

        [Fact]
        public async Task DeploymentStatusCacheGet_ReturnsStoredCollection()
        {
            var cache = _serviceProvider.GetRequiredService<IDistributedCacheService>();
            var collection = new ADODeploymentCollection
            {
                LastUpdated = DateTime.UtcNow,
                DeploymentStatuses =
                [
                    new ADODeploymentStatus
                    {
                        PipelineId = 1001,
                        PipelineName = "Insights - Synthetic Pipeline",
                        LastUpdated = DateTime.UtcNow,
                    },
                ],
            };

            await cache.Set(
                VAA.EAPI.Microservices.Insights.Constants.Application.DeploymentStatus,
                collection,
                false,
                TimeSpan.Zero,
                TimeSpan.FromDays(30));

            var storedCollection = await ADODeploymentStatusCache.GetAsync(cache);

            Assert.NotNull(storedCollection);
            Assert.Collection(
                storedCollection.DeploymentStatuses,
                status =>
                {
                    Assert.Equal(1001, status.PipelineId);
                    Assert.Equal("Insights - Synthetic Pipeline", status.PipelineName);
                });
        }

        [Fact]
        public async Task DeploymentStatusCacheReset_RemovesStoredCollection()
        {
            var cache = _serviceProvider.GetRequiredService<IDistributedCacheService>();
            var collection = new ADODeploymentCollection
            {
                LastUpdated = DateTime.UtcNow,
                DeploymentStatuses =
                [
                    new ADODeploymentStatus
                    {
                        PipelineId = 1001,
                        PipelineName = "Insights - Synthetic Pipeline",
                        LastUpdated = DateTime.UtcNow,
                    },
                ],
            };

            await ADODeploymentStatusCache.SaveAsync(cache, collection);

            await ADODeploymentStatusCache.ResetAsync(cache);

            var storedCollection = await ADODeploymentStatusCache.GetAsync(cache);

            Assert.Null(storedCollection);
        }

        [Fact]
        public async Task DeploymentStatusCacheUpdate_SerialisesReadModifyWriteOperations()
        {
            var cache = _serviceProvider.GetRequiredService<IDistributedCacheService>();
            await ADODeploymentStatusCache.ResetAsync(cache);

            var firstMutationStarted = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            var allowFirstMutationToComplete = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            var secondMutationStarted = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

            var firstUpdate = Task.Run(
                () => ADODeploymentStatusCache.UpdateAsync(
                    cache,
                    deploymentCollection =>
                    {
                        firstMutationStarted.SetResult(null);
                        allowFirstMutationToComplete.Task.GetAwaiter().GetResult();

                        deploymentCollection.DeploymentStatuses =
                        [
                            .. deploymentCollection.DeploymentStatuses,
                            new ADODeploymentStatus
                            {
                                PipelineId = 1001,
                                PipelineName = "First Pipeline",
                                LastUpdated = DateTime.UtcNow,
                            },
                        ];

                        return true;
                    }),
                TestContext.Current.CancellationToken);

            await firstMutationStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            var secondUpdate = Task.Run(
                () => ADODeploymentStatusCache.UpdateAsync(
                    cache,
                    deploymentCollection =>
                    {
                        secondMutationStarted.SetResult(null);

                        deploymentCollection.DeploymentStatuses =
                        [
                            .. deploymentCollection.DeploymentStatuses,
                            new ADODeploymentStatus
                            {
                                PipelineId = 1002,
                                PipelineName = "Second Pipeline",
                                LastUpdated = DateTime.UtcNow,
                            },
                        ];

                        return true;
                    }),
                TestContext.Current.CancellationToken);

            await Task.Delay(100, TestContext.Current.CancellationToken);
            Assert.False(secondMutationStarted.Task.IsCompleted);

            allowFirstMutationToComplete.SetResult(null);
            await Task.WhenAll(firstUpdate, secondUpdate);

            var storedCollection = await ADODeploymentStatusCache.GetAsync(cache);

            Assert.NotNull(storedCollection);
            Assert.Collection(
                storedCollection.DeploymentStatuses.OrderBy(status => status.PipelineId),
                status =>
                {
                    Assert.Equal(1001, status.PipelineId);
                    Assert.Equal("First Pipeline", status.PipelineName);
                },
                status =>
                {
                    Assert.Equal(1002, status.PipelineId);
                    Assert.Equal("Second Pipeline", status.PipelineName);
                });
        }

        private static HttpRequestData CreateRequest(string uri, string method, string? body = null)
        {
            Stream? requestBody = body is null
                ? null
                : new MemoryStream(System.Text.Encoding.UTF8.GetBytes(body));

            return new TestHttpRequestData(AsyncContext.Current.Context, new Uri(uri), requestBody, method, null, null, null);
        }

        private static async Task<ADODeploymentCollection?> ReadDeploymentStatusAsync(HttpResponseData response)
        {
            response.Body.Position = 0;
            using var reader = new StreamReader(response.Body);
            var responseBody = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);

            return string.IsNullOrWhiteSpace(responseBody)
                ? null
                : Json.Deserialize<ADODeploymentCollection>(responseBody);
        }

        private static async Task WaitForPipelineProcessingToStartAsync(ConcurrentBag<Task> pipelineProcessingTasks)
        {
            var deadline = DateTime.UtcNow.AddSeconds(5);

            while (pipelineProcessingTasks.IsEmpty && DateTime.UtcNow < deadline)
            {
                await Task.Delay(50, TestContext.Current.CancellationToken);
            }

            Assert.False(pipelineProcessingTasks.IsEmpty);
        }

        private static async Task<ADODeploymentCollection?> WaitForDeploymentStatusAsync(
            ADOController controller,
            Func<ADODeploymentCollection?, bool> condition)
        {
            var deadline = DateTime.UtcNow.AddSeconds(5);
            ADODeploymentCollection? collection = null;

            do
            {
                var response = await controller.GetDeploymentStatus(CreateRequest("http://localhost/api/deploymentstatus", "GET"));
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                collection = await ReadDeploymentStatusAsync(response);
                if (condition(collection))
                {
                    return collection;
                }

                await Task.Delay(50, TestContext.Current.CancellationToken);
            }
            while (DateTime.UtcNow < deadline);

            Assert.True(condition(collection));
            return collection;
        }
    }
}
