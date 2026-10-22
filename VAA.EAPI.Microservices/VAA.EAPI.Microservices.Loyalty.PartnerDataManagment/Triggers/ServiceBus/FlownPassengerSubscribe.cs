
using Microsoft.Azure.Functions.Worker;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.FlownPassenger.Interface;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Domain.Entities.Common;
namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Triggers.ServiceBus
{
    public class FlownPassengerSubscribe
    {


        private readonly IFlownPassenger _flownPassenger;
        private readonly ILogger<FlownPassengerSubscribe> _logger;

        public FlownPassengerSubscribe(IFlownPassenger flownPassenger, ILogger<FlownPassengerSubscribe> logger)
        {
            _flownPassenger = flownPassenger;
            _logger = logger;
        }

        [Function("FlownPassengerSubscribe")]
        public async Task Run([ServiceBusTrigger(Constants.Application.FlownPassengerTopicName, Constants.Application.FlownPassengerDataSubscriber, Connection = Constants.Environment.FlownPassengerServiceBusConnectionString)] string eventRequestPayload, CancellationToken cancellationToken)
        {
            _logger.LogInformation("FlownPassengerSubscribe function triggered. PayloadLength: {PayloadLength}", eventRequestPayload?.Length ?? 0);

            try
            {
                var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);

                VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(cloudEvent);

                var result = await _flownPassenger.Handle(cloudEvent, cancellationToken);

                if (result.Status != Status.OK)
                {
                    throw new InvalidOperationException(result.ErrorMessage ?? "Flown passenger data could not be processed.");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing flown passenger data. PayloadLength: {PayloadLength}", eventRequestPayload?.Length ?? 0);
                throw;
            }
        }
    }
}
