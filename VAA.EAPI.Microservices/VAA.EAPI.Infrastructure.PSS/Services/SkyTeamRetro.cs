using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Services
{
    public class SkyTeamRetro : ISkyTeamRetro
    {
        private readonly OnlineRetroCreditPtClient _onlineRetroCreditPtClient;
        private readonly ILogger<SkyTeamRetro> _logger;

        public SkyTeamRetro(OnlineRetroCreditPtClient onlineRetroCreditPtClient, ILogger<SkyTeamRetro> logger)
        {
            _onlineRetroCreditPtClient = onlineRetroCreditPtClient ?? throw new ArgumentNullException(nameof(onlineRetroCreditPtClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OnlineRetroCreditingResponse> SkyTeamRetroEligibiltycheck(OnlineRetroCreditRequestType eligibiltyCheckRequest)
        {
            try
            {
                return await _onlineRetroCreditPtClient.OnlineRetroCreditingAsync(eligibiltyCheckRequest);
            }

            catch (Exception exception)
            {
                _logger.LogError(exception, "Fault Exception from SkyTeamRetro service");
                throw;
            }
        }

        public async Task<OnlineRetroCreditFeedbackResponse> SkyTeamRetroFeedback(OnlineFeedbackRequestType skyFeedbackRequest)
        {
            try
            {
                return await _onlineRetroCreditPtClient.OnlineRetroCreditFeedbackAsync(skyFeedbackRequest);
            }

            catch (Exception exception)
            {
                _logger.LogError(exception, "Fault Exception from SkyTeamRetro service");
                throw;
            }
        }
    }
}
