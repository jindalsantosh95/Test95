using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Application.OrderChangeNotif;
using VAA.EAPI.NDC.Order.Domain;
using VAA.EAPI.NDC.Order.Triggers.Http;
using Xunit;
using TestFunctionContext = VAA.EAPI.Core.Functions.TestFunctionContext;
using TestHttpRequestData = VAA.EAPI.Core.Functions.TestHttpRequestData;

namespace VAA.EAPI.NDC.Order.Tests.OrderChangeNotif
{
    public class OrderChangeNotifControllerTests
    {
        [Fact]
        public async Task OrderChangeNotif_ForwardsDeserializedEnsRequestToCommandHandler()
        {
            var command = new RecordingOrderChangeNotifCommand();
            var controller = new OrderChangeNotifController(
                new Mediator(new EmptyServiceProvider()),
                NullLogger<OrderChangeNotifController>.Instance,
                command,
                new NoOpTtlExpiryCommand());
            var payload = Microservices.Framework.Tests.TestHelpers.TestDataProvider
                .GetXmlTestData<string>("NDCOrder\\OrderChangeNotif\\OrderChangeNotifReq");
            var request = new TestHttpRequestData(
                new TestFunctionContext(),
                new Uri("https://localhost/api/OrderChangeNotif"),
                new MemoryStream(Encoding.UTF8.GetBytes($"<Envelope><Body>{payload}</Body></Envelope>")),
                "POST");

            var response = await controller.OrderChangeNotif(request);

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            Assert.NotNull(command.Request);
            Assert.Equal("SIVOZP", command.Request.Locator);
            Assert.Equal(1, command.CallCount);
        }

        private sealed class RecordingOrderChangeNotifCommand : IOrderChangeNotifCommand
        {
            public OrderChangeNotifRQ? Request { get; private set; }
            public int CallCount { get; private set; }

            public Task<Result<Response<string>>> Handle(OrderChangeNotifRQ request, CancellationToken cancellationToken)
            {
                Request = request;
                CallCount++;
                return Task.FromResult(Result.Success(new Response<string> { Status = Status.Accepted }));
            }
        }

        private sealed class NoOpTtlExpiryCommand : ITtlExpiryCommand
        {
            public Task Handle(OcnTtlExpiryRQ request, string requestType, CancellationToken cancellationToken) => Task.CompletedTask;
        }

        private sealed class EmptyServiceProvider : IServiceProvider
        {
            public object? GetService(Type serviceType) => null;
        }

    }
}
