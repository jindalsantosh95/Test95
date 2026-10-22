using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.MillionMiler;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class MillionMilerEmailTriggerCommandHandler : IRequestHandler<MillionMilerEmailTriggerCommand, bool>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<MillionMilerEmailTriggerCommandHandler> _logger;

        public MillionMilerEmailTriggerCommandHandler(IMediator mediator, ILogger<MillionMilerEmailTriggerCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(MillionMilerEmailTriggerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var eventData = MillionMilerEmailTriggeringEventMapper.CreateEventSchemaData(request);
                var cloudEvent = MillionMilerEmailTriggeringEventGridMapper.EventDataMapper(eventData);

                await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent, Subject = cloudEvent.Subject });

                try
                {
                    await _mediator.Publish(cloudEvent, CancellationToken.None);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"MillionMilerEmailTriggerCommandHandler {request.Id} failed due to Exception");
                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"MillionMilerEmailTriggerCommandHandler {request.Id} failed due to Exception");
                return false;
            }
            finally
            {
                _logger.LogInformation($"MillionMilerEmailTriggerCommandHandler {request.Id} - End");
            }
        }
    }
}
