using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.LoungeManagement.Clues.Helper;
using VAA.EAPI.LoungeManagement.Clues.Infrastructure.Domain.Entities.LoungeEligibility;

namespace VAA.EAPI.LoungeManagement.Clues.Application.Command
{
    public class LoungeEligibilityOverrideCommandHandler : IRequestHandler<LoungeEligibilityOverrideCommand, Response<LoungeEligibilityOverrideResponse>>
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<LoungeEligibilityOverrideCommandHandler> _logger;
        public LoungeEligibilityOverrideCommandHandler(IOAuthTokenProvider tokenProvider, IHttpConnector httpConnector, ILogger<LoungeEligibilityOverrideCommandHandler> logger)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Response<LoungeEligibilityOverrideResponse>> Handle(LoungeEligibilityOverrideCommand request, CancellationToken cancellationToken)
        {
            try
            {
                return new Response<LoungeEligibilityOverrideResponse>()
                {
                    Status = Status.OK,
                    Value = await _httpConnector.SendAsync<LoungeEligibilityOverrideResponse>($"{ConfigReader.Get(Constants.Environment.LoungeEligibilityApiUrl)}{ConfigReader.Get(Constants.Application.EligibilityOverride)}", MediaTypeHelper.GetHeaders(), await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Constants.Environment.LoungeAppId)), HttpMethod.Post, Json.Serialize(request.LoungeEligibilityOverrideRequest)),
                    Errors = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Eligibility-SkyTeamEligibilityOverrideServiceFacade - failed due to Exception");

                throw;
            }

        }
    }
}
