using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.EarnCalculator;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Helper;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Command
{
    public class EarnAirlineAccrualCalculatorCommandHandler : IRequestHandler<EarnAirlineAccrualCalculatorCommand, Response<EarnAirlineAccrualCalculatorResponse>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<EarnAirlineAccrualCalculatorCommandHandler> _logger;
        private readonly IInRuleRepository _inRuleRepository;

        public EarnAirlineAccrualCalculatorCommandHandler(IMediator mediator, IInRuleRepository inRuleRepository,ILogger<EarnAirlineAccrualCalculatorCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _inRuleRepository = inRuleRepository ?? throw new ArgumentNullException(nameof(inRuleRepository));
        }

        public async Task<Response<EarnAirlineAccrualCalculatorResponse>> Handle(EarnAirlineAccrualCalculatorCommand request, CancellationToken cancellationToken)
        {
            EarnAirlineAccrualCalculatorResponse earnAirlineAccrualCalculatorResponse = new EarnAirlineAccrualCalculatorResponse();

            try
            {
                var inRuleEntityStates = InRuleHelper.GetEntityStates(request.EarnAirlineAccrualCalculatorRequest);

                earnAirlineAccrualCalculatorResponse.SegmentResponses = await Task.WhenAll(inRuleEntityStates.Select(async entityState =>
                {
                    var inRuleResponse = await _inRuleRepository.ExecuteInRule(
                        InRuleHelper.GetRuleEngineRequest(entityState));

                    return (SegmentResponse)Json.Deserialize<LoyaltyRuleEntityStateResponse>(Convert.ToString(inRuleResponse.ResponseEntity));
                }));

                return new Response<EarnAirlineAccrualCalculatorResponse>
                {
                    Value = earnAirlineAccrualCalculatorResponse,
                    Status = Status.OK
                };

            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in Calculating Points. {ex.Message}");
                return new Response<EarnAirlineAccrualCalculatorResponse>() { Status = Status.BusinessException, Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.PointsCalculationError), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath) };             

            }
        }
    }
}
