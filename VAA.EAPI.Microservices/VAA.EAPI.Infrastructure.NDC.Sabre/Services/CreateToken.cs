using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreateToken;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services
{
    public class CreateToken(TokenCreatePortTypeClient tokenTypeClient, ILogger<CreateToken> logger) : ICreateToken
    {
        private readonly TokenCreatePortTypeClient _tokenTypeClient = tokenTypeClient ?? throw new ArgumentNullException(nameof(tokenTypeClient));
        private readonly ILogger<CreateToken> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<TokenCreateRQResponse> CreateTokenAsync(TokenCreateRQRequest tokenCreate)
        {
            try
            {
                var response = await _tokenTypeClient.TokenCreateRQAsync(tokenCreate.MessageHeader, tokenCreate.Security, tokenCreate.TokenCreateRQ);

                return response;
            }
            catch (FaultException ex)
            {
                _logger.LogError(ex, $"Error while creating a token due to : {ex.Message}");
                throw SabreFaultExceptionChecker.Check(ex);
            }
        }
    }
}
