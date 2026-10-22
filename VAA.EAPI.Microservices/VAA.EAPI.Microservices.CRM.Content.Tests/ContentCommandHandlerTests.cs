using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Xunit.Caching;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Core.Xunit.ServiceBus;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.CRM.Content.Application;
using VAA.EAPI.Microservices.CRM.Content.Interfaces;
using VAA.EAPI.Microservices.CRM.Content.Models;
using VAA.EAPI.Microservices.CRM.Content.Triggers.ServiceBus;
using Xunit;

namespace VAA.EAPI.Microservices.CRM.Content.Tests
{
    public class ContentCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public ContentCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {

            ConfigReader.Set("AemGraphEndpointUri", "https://test1-vaahome.webdev.vholsinternal.co.uk/graphql/execute.json/vaa-home/");
            ConfigReader.Set("AemImagePrefix", "https://stg-vaabrowse.virginatlantic.com/");
            ConfigReader.Set("ContentServiceBusConnection", "https://tempuri.org/reference");

            var azureServiceBusFactory = new MessageBusFactory();

            _serviceProvider ??= new ServiceCollection().AddLogging()
                                                        .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                        .AddSingleton<IHttpConnector, HttpConnector>()
                                                        .AddSingleton<IDistributedCache>(DistributedCache.Instance)
                                                        .AddSingleton<IDistributedCacheService, DistributedCacheService>()
                                                        .AddSingleton<IAzureBlobService, Core.Xunit.Azure.AzureBlobService>()
                                                        .AddSingleton<IContentRequestHandler, ContentRequestHandler>()
                                                        .AddSingleton<ContentRequestSubscriber>()
                                                        .AddSingleton<IMessageBus>(azureServiceBusFactory.GetClient("testConnectionString", "testSource"))
                                                        .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };

        }

        [Fact]
        public async Task TestVaa_App_Help_Screen()
        {
            var cache = _serviceProvider.GetService<IDistributedCacheService>();
            var blob = _serviceProvider.GetService<IAzureBlobService>();
            var distributedCacheService = _serviceProvider.GetService<IDistributedCacheService>();
            var uat = _serviceProvider.GetService<IContentRequestHandler>();

            // Act
            var result = await uat.Handle("vaa-app-help-screen", CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Count());
            Assert.Equal("11223344", result.SelectToken("items").First().SelectToken("goldTierUkPhoneNumber").ToString());
            Assert.Equal("55667788", result.SelectToken("items").First().SelectToken("goldTierUsPhoneNumber").ToString());
            Assert.Equal("12345678", result.SelectToken("items").First().SelectToken("redTierUsPhoneNumber").ToString());
        }

        [Fact]
        public async Task Test_ContentRequest_ServiceBus()
        {
            // Arrange
            var controller = _serviceProvider.GetService<ContentRequestSubscriber>()!;
            var messageBus = (MessageBus)_serviceProvider.GetService<IMessageBus>()!;

            CloudEventTopic? message = default;
            messageBus.OnMessage += (m) => message = (CloudEventTopic)m; // capture message sent to service bus for validation

            var cloudEvent = new CloudEvent
            {
                Data = Json.Serialize(new ContentRequestEvent()
                {
                    ContentFragmentId = "vaa-app-help-screen",
                    ReplySubject = "test-reply-subject",
                    ReplyTopic = "test-reply-topic"
                })
            };

            await controller.Run(Json.Serialize(cloudEvent), CancellationToken.None);

            await Task.Delay(100); // allow some time for the message to be published to the bus    

            Assert.NotNull(message);
            Assert.Equal("test-reply-topic", message!.Topic);
            Assert.Equal("test-reply-subject", message.Subject);

            Assert.NotNull(message.Data);

            var data = (List<string>)message.Data!;
            Assert.Single(data);

            var result = JObject.Parse(data.First());

            Assert.Equal("11223344", result.SelectToken("goldTierUkPhoneNumber").ToString());
            Assert.Equal("55667788", result.SelectToken("goldTierUsPhoneNumber").ToString());
            Assert.Equal("12345678", result.SelectToken("redTierUsPhoneNumber").ToString());
        }
    }
}
