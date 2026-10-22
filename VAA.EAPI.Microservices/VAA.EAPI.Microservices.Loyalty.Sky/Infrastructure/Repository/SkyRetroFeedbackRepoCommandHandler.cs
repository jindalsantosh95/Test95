using MediatR;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Sky.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Sky.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Sky.Infrastructure.Repository
{
    public class SkyRetroFeedbackRepoCommandHandler : IRequestHandler<SkyRetroFeedbackRepoCommand, SkyOnlineRetroFeedbackResponse>
    {
        private readonly ILogger<SkyRetroFeedbackRepoCommandHandler> _logger;
        private readonly ISkyTeamRetro _skyTeamRetro;

        public SkyRetroFeedbackRepoCommandHandler(ILogger<SkyRetroFeedbackRepoCommandHandler> logger, ISkyTeamRetro skyTeamRetro)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _skyTeamRetro = skyTeamRetro ?? throw new ArgumentNullException(nameof(skyTeamRetro));
        }

        public async Task<SkyOnlineRetroFeedbackResponse> Handle(SkyRetroFeedbackRepoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var skyEligibilityCheckResponse = await _skyTeamRetro.SkyTeamRetroFeedback(SkyRetroFeedbackMapper.SkyFeedbackRequest(request.FeedbackRequest));

                return SkyRetroFeedbackMapper.SkyOnlineRetroFeedbackResponse(skyEligibilityCheckResponse.OnlineFeedbackResponse);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "SkyRetroFeedback - Exception");
                throw;
            }
        }
    }
}
