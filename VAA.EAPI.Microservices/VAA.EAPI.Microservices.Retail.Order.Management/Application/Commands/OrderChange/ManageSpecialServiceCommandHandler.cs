using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.OrderChange
{
    public class ManageSpecialServiceCommandHandler : IRequestHandler<ManageSpecialServiceCommand, Result<Response<string>>>
    {
        private readonly ILogger<ManageSpecialServiceCommandHandler> _logger;
        private readonly IMediator _mediator;

        public ManageSpecialServiceCommandHandler(ILogger<ManageSpecialServiceCommandHandler> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Result<Response<string>>> Handle(ManageSpecialServiceCommand request, CancellationToken cancellationToken)
        {
            var id = string.Empty;

            try
            {
                var cloudEvent = ManageSpecialServiceCommandToEventMapper.CommandToEventMapper(request);

                id = cloudEvent.Id; 

                await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                try
                {
                    await _mediator.Publish(cloudEvent, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Error while publishing event:{ex}");
                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = id, Type = OrderEventType.ManageSpecialServiceInitiate.ToString(), Data = request, Subject = Constants.Application.ManageSpecialServiceEventSubject }, cancellationToken);
                    throw;
                }

                var manageCustomerResponse = ManageSpecialServiceCommandResponseMapper.ManageSpecialServiceResponseMapper(cloudEvent.Id);

                return Result.Success(manageCustomerResponse);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in manage SpecialService command handler for CorrelationId: {AsyncContext.Current.CorrelationId}");
                return Result.Failure<Response<string>>(ex.Message);
            }
        }

    }
}
