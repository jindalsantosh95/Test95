using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Command
{
    public class ExecuteTierRuleSetCommandHandler : IExecuteTierRuleSetCommand
    {
        private readonly IInRuleRepository _inRuleRepository;
        private readonly ILogger<ExecuteTierRuleSetCommandHandler> _logger;

        public ExecuteTierRuleSetCommandHandler(
            IInRuleRepository inRuleRepository,
            ILogger<ExecuteTierRuleSetCommandHandler> logger)
        {
            _inRuleRepository = inRuleRepository ?? throw new ArgumentNullException(nameof(inRuleRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Response<ExecuteTierRuleSetInRuleResponse>> Handle(
            TierRuleSetEntityRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var tierRuleSetRequest = request
                    ?? throw new ArgumentNullException(nameof(request));
                var ruleEngineRequest = TierRuleSetRequestMapper.TierRuleSetToInRuleMapper(tierRuleSetRequest);
                var inRuleResponse = await _inRuleRepository.ExecuteInRule(ruleEngineRequest);
                var tierRuleEntityState = Json.Deserialize<TierRuleEntityState>(Convert.ToString(inRuleResponse.ResponseEntity));

                return new Response<ExecuteTierRuleSetInRuleResponse>
                {
                    Status = Status.OK,
                    Value = new ExecuteTierRuleSetInRuleResponse
                    {
                        HasRuntimeErrors = inRuleResponse.HasRuntimeErrors,
                        ResponseEntity = ActionEventDataMapper.MapActionEventData(tierRuleSetRequest, tierRuleEntityState)
                    }
                };
            }            
            catch (Exception ex)
            {
                var errorId = GetErrorId();
                _logger.LogError(ex, "Wings member assessment failed. Error ID {ErrorId}.", errorId);

                return new Response<ExecuteTierRuleSetInRuleResponse>
                {
                    Status = Status.Error,
                    Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(
                        Constants.ErrorCodes.TierRulesEvaluationError,
                        errorId,
                        VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                };
            }
        }

        private static string GetErrorId()
        {
            var correlationId = AsyncContext.Current?.CorrelationId;

            return correlationId is { } value && value != Guid.Empty
                ? value.ToString()
                : Guid.NewGuid().ToString();
        }
    }
}
