
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.FlownPassenger;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.FlownPassenger.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Triggers.ServiceBus;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Tests
{
    public class FlownPassengerSubscribeTests
    {
        private readonly IServiceProvider _serviceProvider;

        public FlownPassengerSubscribeTests()
        {
            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IFlownPassenger, FakeFlownPassenger>()
                .AddSingleton<FlownPassengerSubscribe>()
                .BuildServiceProvider();
        }

        [Fact]
        public async Task Run_ValidPayload_InvokesHandleSuccessfully()
        {
            var subscriber = _serviceProvider.GetRequiredService<FlownPassengerSubscribe>();

            var payload = Json.Serialize(new CloudEvent
            {
                Id = "test-id",
                Type = "test-type",
                Source = "test-source",
                Specversion = "1.0",
                Data = new { Test = "data" }
            });

            var cancellationToken = CancellationToken.None;

            await subscriber.Run(payload, cancellationToken);

            var flownPassenger = _serviceProvider.GetRequiredService<IFlownPassenger>() as FakeFlownPassenger;
            Assert.True(flownPassenger.WasHandleCalled);
        }

        [Fact]
        public async Task Run_InvalidPayload_ThrowsException()
        {
            var subscriber = _serviceProvider.GetRequiredService<FlownPassengerSubscribe>();

            var invalidPayload = "invalid-payload";
            var cancellationToken = CancellationToken.None;

            var ex = await Assert.ThrowsAsync<JsonReaderException>(() => subscriber.Run(invalidPayload, cancellationToken));

            Assert.Contains("Unexpected character encountered", ex.Message);
        }

        private class FakeFlownPassenger : IFlownPassenger
        {
            public bool WasHandleCalled { get; private set; }

            public Task<Response<string>> Handle(CloudEvent cloudEvent, CancellationToken cancellationToken)
            {
                WasHandleCalled = cloudEvent.Id == "test-id";
                return Task.FromResult(new Response<string> { Status = Status.OK });
            }
        }
    }

}
