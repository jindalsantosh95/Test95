using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Constants;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services
{
    public class VerifyMemberServiceHandler : IRequestHandler<VerifyMemberServiceCommand, string>
    {
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<VerifyMemberServiceHandler> _logger;

        public VerifyMemberServiceHandler(IHttpConnector httpConnector, ILogger<VerifyMemberServiceHandler> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> Handle(VerifyMemberServiceCommand verifyB2CServiceQuery, CancellationToken cancellationToken)
        {
            string loginApiUrl = $"{ConfigReader.Get(Environment.AuthenticationUrl)}{ConfigReader.Get(Environment.B2CTenantId)}{verifyB2CServiceQuery.Api}";

            var httpResponse = await _httpConnector.PostAsync(loginApiUrl, verifyB2CServiceQuery.FormUrlEncodedContent);

            var responseMessage = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Azure B2C member verification failed. StatusCode: {StatusCode}", (int)httpResponse.StatusCode);
                return Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.InvalidMemberErrorCode, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath));
            }

            return Json.Deserialize<B2CAuthenticationResponse>(responseMessage)?.Id_token;
        }
    }
}
