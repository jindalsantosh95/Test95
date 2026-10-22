using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreateSession;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services
{
    public class CreateSession : ICreateSession
    {
        readonly SessionCreatePortTypeClient _sessionTypeClient;
        private readonly ILogger<CreateSession> _logger;

        public CreateSession(SessionCreatePortTypeClient sessionTypeClient, ILogger<CreateSession> logger)
        {
            _sessionTypeClient = sessionTypeClient ?? throw new ArgumentNullException(nameof(sessionTypeClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SessionCreateRQResponse> CreateSessionAsync(SessionCreateRQRequest sessionCreate)
        {
            try
            {
                var response = await _sessionTypeClient.SessionCreateRQAsync(sessionCreate.MessageHeader, sessionCreate.Security, sessionCreate.SessionCreateRQ);

                return response;
            }
            catch (FaultException ex)
            {
                _logger.LogError(ex, $"Error while creating a session due to : {ex.Message}");
                throw SabreFaultExceptionChecker.Check(ex);
            }            
        }
    }
}
