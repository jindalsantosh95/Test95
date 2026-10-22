using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.AirportDetails;
using VAA.EAPI.Microservices.Retail.Order.Management.Mapper.AirportDetail;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.AirportDetails
{
    public class AirportDetailsEventProcessCommand : CloudEvent, IRequest<Result<Response<string>>>
    {
        public AirportDetail Data { get; set; }
    }
    public class AirportDetailsEventProcessCommandhandler : IRequestHandler<AirportDetailsEventProcessCommand, Result<Response<string>>>
    {

        private readonly ILogger<AirportDetailsEventProcessCommandhandler> _logger;
        private readonly TableStorageService<AirportLocation> _tableStorageService;
        public AirportDetailsEventProcessCommandhandler(
             ILogger<AirportDetailsEventProcessCommandhandler> logger, ITableStorageService<AirportLocation> tableStorageService)
        {

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tableStorageService = (TableStorageService<AirportLocation>)tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.ReferenceDataMicroServiceDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.ReferenceDataTableName);

        }
        public async Task<Result<Response<string>>> Handle(AirportDetailsEventProcessCommand request, CancellationToken cancellationToken)
        {
            var data = request?.Data;
            try
            {

                _logger.LogDebug("Flight Information  Event Subscribe command handler, airport code :" + data?.IataAirportCode);

                if (data?.OpType == Constants.Application.DeleteType)
                {
                    _logger.LogDebug("DELETE FOR AIRPORT: " + data.IataAirportCode);
                    await _tableStorageService.DeleteAsync(AirportLocationMapper.MaptoAirportLocation(data));

                }
                if (data?.OpType == Constants.Application.MergeType)
                {
                    _logger.LogDebug("MERGE FOR AIRPORT: " + data.IataAirportCode);
                    await _tableStorageService.InsertOrMergeAsync(AirportLocationMapper.MaptoAirportLocation(data));
                }
                return Result.Success(new Response<string>
                {
                    Value = $"Flight Information  Event Subscribe Command handler success for airport code {data?.IataAirportCode}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occured in AirportDetailsEventProcess Subscribe Command Handler for airport code : {data?.IataAirportCode}");
                return Result.Failure<Response<string>>(ex.Message);
            }
        }
    }
}
