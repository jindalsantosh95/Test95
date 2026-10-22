using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreateSession;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.SessionClose;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services
{
    public class SessionClose : ISessionClose
    {
        private readonly SessionClosePortTypeClient _sessionClosePortTypeClient;
        private readonly ILogger<SessionClose> _logger;

        public SessionClose(SessionClosePortTypeClient sessionClosePortTypeClient, ILogger<SessionClose> logger)
        {
            _sessionClosePortTypeClient = sessionClosePortTypeClient ?? throw new ArgumentNullException(nameof(sessionClosePortTypeClient)); ;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SessionCloseRQResponse> SessionCloseAsync(SessionCloseRQRequest sessionCloseRQ)
        {
            try
            {
                return await _sessionClosePortTypeClient.SessionCloseRQAsync(sessionCloseRQ.MessageHeader, sessionCloseRQ.Security, sessionCloseRQ.SessionCloseRQ);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, $"Error while Closing the session : {ex.Message}");
                return null;
            }
        }
    }
}

