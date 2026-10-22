using MediatR;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Mappers;
using VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Proxy.AllianzDecisionResponse;

namespace VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Services.OrderCreate
{
    public class ServiceDecisionCommandHandler : IRequestHandler<ServiceDecisionCommand, Unit>
    {
        private readonly ILogger<ServiceDecisionCommandHandler> _logger;
        private readonly IHttpConnector _apiConnector;

        public ServiceDecisionCommandHandler(ILogger<ServiceDecisionCommandHandler> logger, IHttpConnector apiConnector)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
        }

        public async Task<Unit> Handle(ServiceDecisionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var anzDecisionRequest = AllianzDecisionRequestMapper.AllianzDecisionRequest(request.Request);
                var quoteResponse = await _apiConnector.SendAsync<AllianzDecisionResponse>(ConfigReader.Try(Insurance.Constants.Environment.AllianzDecisionUri), headers: null, null, verb: HttpMethod.Post, content: Json.Serialize(anzDecisionRequest, false));
                // Logging for testing purpose as this is fire and forget
                _logger.LogInformation($"Decision RS: {Json.Serialize(quoteResponse)}");
                
                return Unit.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation id - {AsyncContext.Current.CorrelationId} Request Processing Terminated for ServiceDecisionCommandHandler");
                throw;
            }
        }
    }
}
