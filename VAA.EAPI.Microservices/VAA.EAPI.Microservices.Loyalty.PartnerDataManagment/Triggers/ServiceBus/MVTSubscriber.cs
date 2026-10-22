using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Helper;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.MVT;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Triggers.ServiceBus
{
    public class MVTSubscriber
    {
        private readonly IMVTMessageEventHandler _mVTMessageEventHandler;
        private readonly ILogger<MVTSubscriber> _logger;
        private readonly IMVTStorageEventHandler _flightMovementEventHandler;
        private readonly ITableStorageService<FlightMovementDetails> _tableStorage;

        public MVTSubscriber(IMVTMessageEventHandler mVTMessageEventHandler, ILogger<MVTSubscriber> logger, IMVTStorageEventHandler flightMovementEventHandler, ITableStorageService<FlightMovementDetails> tableStorage)
        {
            _mVTMessageEventHandler = mVTMessageEventHandler ?? throw new ArgumentNullException(nameof(mVTMessageEventHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _flightMovementEventHandler = flightMovementEventHandler ?? throw new ArgumentNullException(nameof(flightMovementEventHandler));
            _tableStorage = tableStorage ?? throw new ArgumentNullException(nameof(tableStorage));
            _tableStorage.ConnectionString = ConfigReader.Get(Constants.Environment.MicroServiceDBKey);
            _tableStorage.TableName = Constants.Application.MVTEventStoreTable;
        }

        [Function("MVTSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.PdmEventTopicName, Constants.Application.MVTMessageSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            if (MVTADMessageParser.TryParse(eventRequestPayload, out var adMessage))
            {
                await _flightMovementEventHandler.HandleAsync(adMessage);

                return;
            }
            if (!MVTAAMessageParser.TryParse(eventRequestPayload, out var message))
            {
                _logger.LogError("MVT message failed validation. Payload: {Payload}", eventRequestPayload);
                return;
            }

            var flightMovement = await _tableStorage.RetrieveAsync(Constants.Application.MVTMessageEventType, $"{message.FlightCarrier}{message.FlightNumber}_{message.DepartureDate}");
            if (flightMovement == null)
            {
                _logger.LogError(
                    "No matching AD record found for flight {FlightCarrier}{FlightNumber} on {DepartureDate}",
                    message.FlightCarrier,
                    message.FlightNumber,
                    message.DepartureDate);
                return;
            }

            flightMovement.ArrivalAirport = message.ArrivalAirport;
            message.DepartureAirport = flightMovement.DepartureAirport;

            await _tableStorage.InsertOrMergeAsync(flightMovement);

            if (flightMovement.DepartureAirport.Is(flightMovement.ArrivalAirport))
            {
                _logger.LogError(
                    "Flight movement validation failed for Flight {FlightCarrier}{FlightNumber} on {DepartureDate}. DepartureAirport '{DepartureAirport}' and ArrivalAirport '{ArrivalAirport}' cannot be the same.",
                    flightMovement.FlightCarrier,
                    flightMovement.FlightNumber,
                    flightMovement.DepartureDate,
                    flightMovement.DepartureAirport,
                    flightMovement.ArrivalAirport);
                return;
            }

            var handled = await _mVTMessageEventHandler.Handle(message);

            if (!handled)
            {
                throw new InvalidOperationException("Failed to process MVT message.");
            }
        }
    }
}


