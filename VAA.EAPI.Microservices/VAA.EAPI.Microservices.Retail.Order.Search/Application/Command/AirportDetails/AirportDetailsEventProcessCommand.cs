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
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using VAA.EAPI.Microservices.Retail.Order.Search.Mapper;
namespace VAA.EAPI.Microservices.Retail.Order.Search.Application.Command.AirportDetails
{
    public class AirportDetailsEventProcessCommand : CloudEvent, IRequest<Result<Response<string>>>
    {
        public AirportDetail Data { get; set; }
    }

    public class AirportDetailsEventProcessCommandhandler : IRequestHandler<AirportDetailsEventProcessCommand, Result<Response<string>>>
    {
        private readonly ILogger<AirportDetailsEventProcessCommandhandler> _logger;
        private readonly TableStorageService<AirportLocation> _tableStorageService;

        public AirportDetailsEventProcessCommandhandler(ILogger<AirportDetailsEventProcessCommandhandler> logger, ITableStorageService<AirportLocation> tableStorageService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tableStorageService = (TableStorageService<AirportLocation>)tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.ReferenceDataMicroServiceDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.ReferenceDataTableName);
        }

        public async Task<Result<Response<string>>> Handle(AirportDetailsEventProcessCommand request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var data = request.Data;
            try
            {
                switch (data.OpType)
                {
                    case Constants.Application.DeleteType:
                        await _tableStorageService.DeleteAsync(AirportLocationMapper.MaptoAirportLocation(data));
                        break;
                    case Constants.Application.MergeType:
                        await _tableStorageService.InsertOrMergeAsync(AirportLocationMapper.MaptoAirportLocation(data));
                        break;
                }
                return Result.Success(new Response<string>
                {
                    Value = $"Flight Information  Event Subscribe Command handler success for airport code {data.IataAirportCode}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Airport details storage processing failed for {EventId} and {AirportCode}; exception type {ExceptionType}", request?.Id, data?.IataAirportCode, ex.GetType().Name);
                return Result.Failure<Response<string>>(ex.Message);
            }
        }
    }
}
