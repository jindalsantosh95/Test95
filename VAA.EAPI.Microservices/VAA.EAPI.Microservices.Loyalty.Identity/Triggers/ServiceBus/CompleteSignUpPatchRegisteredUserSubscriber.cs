using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Triggers.ServiceBus
{
    public class CompleteSignUpPatchRegisteredUserSubscriber(
        IPatchRegisteredUserHandler patchRegisteredUserHandler,
        ILogger<CompleteSignUpPatchRegisteredUserSubscriber> logger)
    {
        private readonly IPatchRegisteredUserHandler _patchRegisteredUserHandler = patchRegisteredUserHandler ?? throw new ArgumentNullException(nameof(patchRegisteredUserHandler));
        private readonly ILogger<CompleteSignUpPatchRegisteredUserSubscriber> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [Function("CompleteSignUpPatchRegisteredUserSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.IdentityEventHandlerTopicName, Constants.Application.CompleteSignUpPatchRegisteredUserSubscriber, Connection = "ServiceBusConnectionString")] string eventRequestPayload, CancellationToken cancellationToken)
        {
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);
            VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(cloudEvent);

            if (cloudEvent?.Data == null)
                throw new InvalidOperationException("Patch registered user event data is missing.");

            var dataToken = cloudEvent.Data as JToken;
            var request = dataToken != null ? Json.DeserializeObject<PatchRegisteredUserRequest>(dataToken) : null;

            if (request == null)
                throw new InvalidOperationException("Patch registered user event data could not be deserialized.");
            _logger.LogInformation("CompleteSignUp patch registered user event received with event id {EventId}", cloudEvent.Id);

            await _patchRegisteredUserHandler.Handle(request, cancellationToken);
        }
    }
}
