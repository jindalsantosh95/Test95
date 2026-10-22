using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Command
{
    public class TierRuleSetEventCommandHandler : IRequestHandler<TierRuleSetEventCommand, Result<bool>>
    {
        private readonly IMediator _mediator;
        private readonly IInRuleRepository _inRuleRepository;
        private readonly ILogger<TierRuleSetEventCommandHandler> _logger;
        public TierRuleSetEventCommandHandler(IMediator mediator, IInRuleRepository inRuleRepository, ILogger<TierRuleSetEventCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _inRuleRepository = inRuleRepository ?? throw new ArgumentNullException(nameof(inRuleRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<bool>> Handle(TierRuleSetEventCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var tierRuleSetRequest = Json.Deserialize<TierRuleSetEntityRequest>(Convert.ToString(request.Data));
                var ruleEngineRequest = TierRuleSetRequestMapper.TierRuleSetToInRuleMapper(tierRuleSetRequest);
                //Inrule Call
                var inRuleResponse = await _inRuleRepository.ExecuteInRule(ruleEngineRequest);
                TierRuleEntityState resEntity = Json.Deserialize<TierRuleEntityState>(Convert.ToString(inRuleResponse.ResponseEntity));
                var eventData = ActionEventDataMapper.MapActionEventData(tierRuleSetRequest, resEntity);
                var cloudEvent = ActionEventMapper.MapActionEvent(eventData);
                _logger.LogDebug($"Store event in event store - Start");
                await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject });
                _logger.LogDebug($"Store event in event store - End");
                try
                {
                    _logger.LogDebug($"Publish event - Start");
                    await _mediator.Publish(cloudEvent);
                    _logger.LogDebug($"Publish event - End");
                }
                catch
                {
                    _logger.LogDebug($"Deleting the event from event store");
                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                    throw;
                }
                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"InRule Call failed - {exception}");
                return false;
            }
        }
    }
}
