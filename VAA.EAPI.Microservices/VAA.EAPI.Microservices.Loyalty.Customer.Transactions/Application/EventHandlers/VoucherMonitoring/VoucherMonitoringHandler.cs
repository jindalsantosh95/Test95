using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.VoucherMonitoring;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Services.VoucherMonitoring;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.VoucherMonitoring
{
    public sealed class VoucherMonitoringHandler : IVoucherMonitoringHandler
    {
        private readonly IVoucherMonitoringService _service;
        private readonly ILogger<VoucherMonitoringHandler> _logger;
        private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler;


        public VoucherMonitoringHandler(IVoucherMonitoringService service, ILogger<VoucherMonitoringHandler> logger, EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
        }

        public async Task<Result<Response<VoucherMonitoringResponse>>> Handle(VoucherMonitoringEvent request, CancellationToken cancellationToken)
        {
            try
            {
                if (request?.Data is null)
                    throw new ArgumentNullException(nameof(request));

                VoucherMonitoringRequest eventData = Json.DeserializeObject<VoucherMonitoringRequest>((JToken)request.Data);

                var validationResult = new VoucherMonitoringRequestValidator().Validate(eventData);

                if (!validationResult.IsValid)
                    throw new RequestValidationException("Voucher monitoring request contains invalid data or missing mandatory fields.", validationResult.Errors);

                var result = await _service.GetVoucherTransactionMonitoringStatus(eventData, cancellationToken);

                //create event to store in event store
                var cloudEvent = new CloudEvent
                {
                    Id = request.Id,
                    Type = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.VoucherMonitoringStatus.ToString())),
                    Source = ConfigReader.Get(Constants.EnvironmentSettings.ServiceBusTopicName),
                    Subject = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.VoucherMonitoringStatus.ToString())),
                    Time = DateTime.UtcNow.ToString(Constants.Application.CloudEventTimeFormat),
                    Specversion = Constants.Application.TransactionDocSchemaVersion,
                    Data = result
                };

                await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                //publish event to be implemented in future

                return Result.Success(new Response<VoucherMonitoringResponse> { Status = Status.OK, Value = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred in VoucherMonitoringHandler.");
                return Result.Failure<Response<VoucherMonitoringResponse>>("Voucher monitoring processing failed.");
            }
        }
    }
}
