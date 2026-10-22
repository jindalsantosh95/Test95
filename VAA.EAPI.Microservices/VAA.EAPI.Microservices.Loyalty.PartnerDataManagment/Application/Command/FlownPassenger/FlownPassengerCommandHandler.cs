using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.FlownPassenger.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.FlownPassenger;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.FlownDB.Interface;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.FlownPassenger
{
    public class FlownPassengerCommandHandler : IFlownPassenger
    {

         private readonly IFlownDbRepository _flownDbRepository;
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler;
        private readonly ILogger<FlownPassengerCommandHandler> _logger;

        public FlownPassengerCommandHandler(
            IFlownDbRepository flownDbRepository,
            ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler,
            ILogger<FlownPassengerCommandHandler> logger)
        {

            _flownDbRepository = flownDbRepository ?? throw new ArgumentNullException(nameof(flownDbRepository));
            _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Response<string>> Handle(CloudEvent cloudEvent, CancellationToken cancellationToken)
        {
            try
            {
                if (cloudEvent.Data is not string compressedData || string.IsNullOrWhiteSpace(compressedData))
                {
                    return new Response<string>
                    {
                        Status = Status.Error,
                        Value = "false",
                        ErrorMessage = "Invalid request: CloudEvent.Data must be a non-empty compressed string payload."
                    };
                }
                var eventData = compressedData.DecompressToString();
                var request = Json.Deserialize<IEnumerable<FlownPassengerRequest>>(eventData);
                if (request is null)
                {
                    return new Response<string>
                    {
                        Status = Status.Error,
                        Value = "false",
                        ErrorMessage = "Invalid request: payload could not be deserialized into flown passenger details."
                    };
                }

                var successfullyInsertedPassengers = await _flownDbRepository.InsertFlownPassengerDetailsAsync(request, cancellationToken);
                await PublishAutoAccrualMessagesAsync(successfullyInsertedPassengers, cancellationToken);

                return new Response<string>
                {
                    Status = Status.OK,
                    Value = "true", 
                };
            }
            catch (Exception ex)
            {
                // Bubble up failures so the Service Bus trigger can retry / dead-letter as configured.
                throw new InvalidOperationException("Error processing flown passenger data.", ex);
            }
        }

        private async Task PublishAutoAccrualMessagesAsync(IEnumerable<FlownPassengerRequest> flownPassengers, CancellationToken cancellationToken)
        {
            var autoAccrualPayloads = flownPassengers
                .Select(CreateAutoAccrualPayload)
                .Where(payload => payload is not null)
                .Select(payload => payload!)
                .GroupBy(payload => new
                {
                    FlightCarrier = payload.FlightCarrier.Trim().ToUpperInvariant(),
                    FlightNumber = payload.FlightNumber.Trim().ToUpperInvariant(),
                    DepartureAirport = payload.DepartureAirport.Trim().ToUpperInvariant(),
                    ArrivalAirport = payload.ArrivalAirport.Trim().ToUpperInvariant(),
                    payload.DepartureDate
                })
                .Select(group => group.First());

            foreach (var autoAccrualPayload in autoAccrualPayloads)
            {
                var autoAccrualMessage = new AutoAccrualMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    Source = Constants.Application.PdmEventTopicName,
                    Type = Constants.Application.AutoAccrualMessage,
                    Subject = Constants.Application.AutoAccrualMessage,
                    Data = autoAccrualPayload
                };

                _logger.LogInformation(
                    "Publishing AutoAccrual message after flown passenger insert. MessageId: {MessageId}, Payload: {Payload}",
                    autoAccrualMessage.Id,
                    Json.Serialize(autoAccrualMessage));

                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand
                {
                    Message = autoAccrualMessage,
                    ServiceBusConnectionString = ConfigReader.Get<string>(Constants.Environment.ServiceBusConnectionString),
                    ServiceBusTopicName = Constants.Application.PdmEventTopicName
                }, cancellationToken);
            }
        }

        private static AutoAccrualPayload? CreateAutoAccrualPayload(FlownPassengerRequest flownPassenger)
        {
            var eventFlight = flownPassenger.EventFlight;
            var flightCarrier = FirstNonEmpty(eventFlight?.OperatingCarrier, eventFlight?.FlightCarrier);
            var flightNumber = FirstNonEmpty(eventFlight?.OperatingFlightNumber, eventFlight?.FlightNumber);

            if (eventFlight is null ||
                eventFlight.DepartureDate == DateTime.MinValue ||
                string.IsNullOrWhiteSpace(flightCarrier) ||
                string.IsNullOrWhiteSpace(flightNumber) ||
                string.IsNullOrWhiteSpace(eventFlight.DepartureAirport) ||
                string.IsNullOrWhiteSpace(eventFlight.ArrivalAirport))
            {
                return null;
            }

            return new AutoAccrualPayload
            {
                FlightCarrier = flightCarrier,
                FlightNumber = flightNumber,
                DepartureAirport = eventFlight.DepartureAirport,
                ArrivalAirport = eventFlight.ArrivalAirport,
                DepartureDate = eventFlight.DepartureDate.ToString("yyyy-MM-dd")
            };
        }

        private static string? FirstNonEmpty(string? primaryValue, string? fallbackValue) =>
            !string.IsNullOrWhiteSpace(primaryValue) ? primaryValue : fallbackValue;
    }
}
