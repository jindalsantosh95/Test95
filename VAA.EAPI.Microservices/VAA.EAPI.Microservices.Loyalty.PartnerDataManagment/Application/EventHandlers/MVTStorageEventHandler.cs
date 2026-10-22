using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.MVT;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.MVT;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.EventHandlers
{
    public class MVTStorageEventHandler : IMVTStorageEventHandler
    {
        private readonly ITableStorageService<FlightMovementDetails> _tableStorage;

        public MVTStorageEventHandler(ITableStorageService<FlightMovementDetails> tableStorage)
        {
            _tableStorage = tableStorage;
            _tableStorage.ConnectionString = ConfigReader.Get(Constants.Environment.MicroServiceDBKey);
            _tableStorage.TableName = Constants.Application.MVTEventStoreTable;
        }

        public async Task<string> HandleAsync(MVTMessageRequest message)
        {
            try
            {
                if (message == null)
                {
                    throw new ArgumentNullException(nameof(message));
                }

                var entity = new FlightMovementDetails
                {
                    PartitionKey = Constants.Application.MVTMessageEventType,
                    RowKey = $"{message.FlightCarrier}{message.FlightNumber}_{message.DepartureDate}",
                    FlightCarrier = message.FlightCarrier,
                    FlightNumber = message.FlightNumber,
                    DepartureDate = message.DepartureDate,
                    DepartureAirport = message.DepartureAirport,
                    ArrivalAirport = message.ArrivalAirport
                };

                await _tableStorage.InsertOrMergeAsync(entity);

                return "Flight movement data processed successfully.";
            }

            catch (Exception ex)
            {
                throw new Exception($"Error processing flight movement data: {ex.Message}", ex);
            }
        }
    }
}