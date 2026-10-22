using MediatR;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.FlightInformation.Application.Command.Airportdetails;
using VAA.EAPI.Microservices.FlightInformation.Infrasturcture.Entities;
using VAA.EAPI.Microservices.FlightInformation.Mapper;

namespace VAA.EAPI.Microservices.FlightInformation.Application.Command.AirportDetails
{
    public class AirportDetailsMergeCommandHandler : IRequestHandler<AirportDetailsMergeCommand, List<Domain.Airportdetails.Model.Airportdetail>>
    {
        private readonly TableStorageService<AirportDatabaseEntity> _tableStorageService;
        private readonly ILogger<AirportDetailsMergeCommandHandler> _logger;
        public AirportDetailsMergeCommandHandler(ILogger<AirportDetailsMergeCommandHandler> logger, ITableStorageService<AirportDatabaseEntity> tableStorageService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tableStorageService = (TableStorageService<AirportDatabaseEntity>)tableStorageService ?? throw new ArgumentNullException(nameof(_logger));
            _tableStorageService.ConnectionString = Environment.GetEnvironmentVariable(Constants.Environment.MicroServiceDBKey);
            _tableStorageService.TableName = Environment.GetEnvironmentVariable(Constants.Environment.AirportDataTableName);
        }
        public async Task<List<Domain.Airportdetails.Model.Airportdetail>> Handle(AirportDetailsMergeCommand request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            List<Task> tableStorageTaskList = new List<Task>();
            List<AirportDatabaseEntity> airportDetailsToBeMerged = await MergeDetailsToStorageTable(request.AirportDetails);
            return AirportDetailsToDBEnitytMapper.MaptoAirportDetailsList(airportDetailsToBeMerged);
        }
        private async Task<List<AirportDatabaseEntity>> MergeDetailsToStorageTable(List<Domain.Airportdetails.Model.Airportdetail> airportDetails)
        {
            IEnumerable<AirportDatabaseEntity> airportDbEntitiesInStorageTable = await _tableStorageService.RetrieveItemsAsync(default(TableContinuationToken));
            IEnumerable<AirportDatabaseEntity> listOfDBEntitiesToBeDeleted = airportDbEntitiesInStorageTable.Where(x => !airportDetails.Any(y => y.AirportID.Equals(x.RowKey) && y.IataAirportCode.Equals(x.PartitionKey)));
            IEnumerable<AirportDatabaseEntity> listOfDBEntitiesToBeInserted = AirportDetailListToEntityList.MapToDBEntityList(airportDetails.Where(x => !airportDbEntitiesInStorageTable.Any(y => y.RowKey.Equals(x.AirportID) && y.PartitionKey.Equals(x.IataAirportCode))));
            IEnumerable<AirportDatabaseEntity> listOfDBEntitiesToBeUpdated = AirportDetailListToEntityList.MapToDBEntityList(airportDetails.FindAll(x => airportDbEntitiesInStorageTable.Any(y => y.RowKey.Equals(x.AirportID) && y.PartitionKey.Equals(x.IataAirportCode) && !IsDbEntitySameWithStorage(x, y))));
            List<AirportDatabaseEntity> listOfDBEntitiesToBeOperated = new List<AirportDatabaseEntity>();

            listOfDBEntitiesToBeOperated.AddRange(await DeleteFromAzureStorge(listOfDBEntitiesToBeDeleted));
            listOfDBEntitiesToBeOperated.AddRange(await InsertOrMergeToAzureStorage(listOfDBEntitiesToBeInserted));
            listOfDBEntitiesToBeOperated.AddRange(await InsertOrMergeToAzureStorage(listOfDBEntitiesToBeUpdated));

            return listOfDBEntitiesToBeOperated;
        }
        private bool IsDbEntitySameWithStorage(Domain.Airportdetails.Model.Airportdetail source, AirportDatabaseEntity dbEntity)
        {
            if (source != null && dbEntity != null)
            {
                return source.CityIataCode.Equals(dbEntity.CityIataCode)
                       && source.CountryName.Equals(dbEntity.CountryName)
                       && source.AirportName.Equals(dbEntity.AirportName)
                       && source.CountryCode.Equals(dbEntity.CountryCode)
                       && source.EffectiveEndDate.Equals(dbEntity.EffectiveEndDate)
                       && source.EffectiveStartDate.Equals(dbEntity.EffectiveStartDate);
            }
            return false;
        }

        private async Task<List<AirportDatabaseEntity>> DeleteFromAzureStorge(IEnumerable<AirportDatabaseEntity> dbEntities)
        {
            var deletedList = new List<AirportDatabaseEntity>();
            foreach (var entity in dbEntities)
            {
                await _tableStorageService.DeleteAsync(entity);
                entity.OpType = Constants.Application.DeleteType;
                deletedList.Add(entity);
            }
            return deletedList;
        }
        private async Task<List<AirportDatabaseEntity>> InsertOrMergeToAzureStorage(IEnumerable<AirportDatabaseEntity> dbEntities)
        {
            var mergedList = new List<AirportDatabaseEntity>();
            foreach (var entity in dbEntities)
            {
                await _tableStorageService.InsertOrMergeAsync(entity);
                entity.OpType = Constants.Application.MergeType;
                mergedList.Add(entity);
            }
            return mergedList;
        }
    }
}
