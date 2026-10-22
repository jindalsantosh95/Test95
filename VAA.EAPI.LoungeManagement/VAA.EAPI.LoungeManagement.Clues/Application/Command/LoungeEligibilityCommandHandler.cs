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
    public class LoungeEligibilityCommandHandler : IRequestHandler<LoungeEligibilityCommand, Response<LoungeEligibilityResponse>>
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<LoungeEligibilityCommandHandler> _logger;

        public LoungeEligibilityCommandHandler(IOAuthTokenProvider tokenProvider, IHttpConnector httpConnector, ILogger<LoungeEligibilityCommandHandler> logger)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Response<LoungeEligibilityResponse>> Handle(LoungeEligibilityCommand request, CancellationToken cancellationToken)
        {
                return new Response<LoungeEligibilityResponse>()
                {
                    Status = Status.OK,
                    Value = await _httpConnector.SendAsync<LoungeEligibilityResponse>($"{ConfigReader.Get(Constants.Environment.LoungeEligibilityApiUrl)}{ConfigReader.Get(Constants.Application.Eligibility)}", MediaTypeHelper.GetHeaders(), await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Constants.Environment.LoungeAppId)), HttpMethod.Post, Json.Serialize(request.LoungeEligibilityRequest)),
                    Errors = null
                };           
        }
    }
}
