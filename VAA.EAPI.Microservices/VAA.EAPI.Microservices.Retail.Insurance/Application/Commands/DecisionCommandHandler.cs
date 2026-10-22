using MediatR;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Insurance.Domain.Decision;
using VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Services.OrderCreate;

namespace VAA.EAPI.Microservices.Retail.Insurance.Application.Commands
{
    public class DecisionCommandHandler : IRequestHandler<DecisionCommand, Unit>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CreateQuoteCommandHandler> _logger;
        private readonly IHttpConnector _httpConnector;

        public DecisionCommandHandler(IMediator mediator, ILogger<CreateQuoteCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<Unit> Handle(DecisionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var requestBody = Json.Deserialize<DecisionRequest>(request.Data.ToString());
                var quoteResponse = await _mediator.Send(new ServiceDecisionCommand { Request = requestBody }, cancellationToken);
                return Unit.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Exception - {ex}");
                throw;
            }
        }
    }
}
