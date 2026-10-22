using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Mapper.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Commands
{
    public class ManageCustomerCommandHandler : IRequestHandler<ManageCustomerCommand, Result<Response<string>>>
    {
        private readonly ILogger<ManageCustomerCommandHandler> _logger;
        private readonly IMediator _mediator;
        private readonly string _correlationId;

        public ManageCustomerCommandHandler(ILogger<ManageCustomerCommandHandler> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Result<Response<string>>> Handle(ManageCustomerCommand manageCustomerRequest, CancellationToken cancellationToken)
        {
            var eventId = string.Empty;

            try
            {
                if (manageCustomerRequest == null) throw new ArgumentNullException(nameof(manageCustomerRequest));

                var cloudEvent = ManageCustomerCommandToEventMapper.CommandToEventMapper(manageCustomerRequest);
                eventId = cloudEvent.Id;
                await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                try
                {
                    await _mediator.Publish(cloudEvent, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Manage customer event processing failed for {EventId}", cloudEvent.Id);

                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = OrderEventType.ManageCustomerInitiate.ToString(), Data = manageCustomerRequest, Subject = Constants.Application.ManageCustomerEventSubject }, cancellationToken);
                    return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
                }

                var manageCustomerResponse = ManageCustomerEventCommandResponseMapper.ManageCustomerResponseMapper(cloudEvent.Id);

                return manageCustomerResponse != null ? Result.Success(manageCustomerResponse) : Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manage customer command handling failed for {EventId}", eventId);

                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, _correlationId, EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath);

                return Result.Failure<Response<string>>(Json.Serialize(errorResponse));
            }
        }
    }
}

