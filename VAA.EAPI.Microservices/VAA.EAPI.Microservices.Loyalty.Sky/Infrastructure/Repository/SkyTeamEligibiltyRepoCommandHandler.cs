using MediatR;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Sky.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Sky.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Sky.Infrastructure.Repository
{
    public class SkyTeamEligibiltyRepoCommandHandler : IRequestHandler<SkyTeamEligibiltyRepoCommand, CheckEligibilityResponse>
    {
        private readonly ILogger<SkyTeamEligibiltyRepoCommandHandler> _logger;
        private readonly ISkyTeamRetro _skyTeamRetro;

        public SkyTeamEligibiltyRepoCommandHandler(ILogger<SkyTeamEligibiltyRepoCommandHandler> logger, ISkyTeamRetro skyTeamRetro)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _skyTeamRetro = skyTeamRetro ?? throw new ArgumentNullException(nameof(skyTeamRetro));
        }

        public async Task<CheckEligibilityResponse> Handle(SkyTeamEligibiltyRepoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var skyEligibilityCheckResponse = await _skyTeamRetro.SkyTeamRetroEligibiltycheck(SkyTeamEligibilityCheckMapper.SkyCheckEligibilityRequest(request.TicketInfo));

                return SkyTeamEligibilityCheckMapper.SkyCheckEligibilitytResponse(skyEligibilityCheckResponse.OnlineRetroCreditResponse);

            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "SkyTeamEligibiltyCheck - Exception");
                throw;
            }
        }
    }
}
