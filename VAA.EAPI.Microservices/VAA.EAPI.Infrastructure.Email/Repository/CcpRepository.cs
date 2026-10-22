using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Email;
using VAA.EAPI.Infrastructure.Email.Entities;
using VAA.EAPI.Infrastructure.Email.Interface;
using VAA.EAPI.Infrastructure.Email.Mapper;
using VAA.EAPI.Infrastructure.Email.Policies;

namespace VAA.EAPI.Infrastructure.Email.Repository
{
    public class CcpRepository : ICcpRepository
    {
        private readonly ICcpClient _client;
        private readonly ILogger<CcpRepository> _logger;

        public CcpRepository(ILogger<CcpRepository> logger, ICcpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> SendEmail(SendEmailRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (EmailRecipientPolicy.ShouldSuppress(request.To))
            {
                _logger.LogInformation("Email delivery suppressed by recipient policy");
                return true;
            }

            var ccpRequest = SendEmailToCCPRequestMapper.MapRequest(request);
            _logger.LogInformation($"Process started to send emails");

            KeyValuePair<bool, string> result = await _client.SendEmail(ccpRequest);
            CcpResponse ccpResponse = Json.Deserialize<CcpResponse>(result.Value);

            if (result.Key)
            {
                _logger.LogInformation($"CCP API Response,  Key : {result.Key}");

                if (ccpResponse.Success)
                {
                    return true;
                }
                else
                {
                    if (ccpResponse.ErrorCode != null && ccpResponse.ErrorMessage != null)
                    {
                        _logger.LogInformation($"{ccpResponse.ErrorCode} {ccpResponse.ErrorMessage}");
                        return false;
                    }
                    else
                    {
                        _logger.LogInformation("ccpResponse-There was an unexpected error");
                        return false;
                    }
                }
            }
            _logger.LogInformation($"CCP API Response- Empty response");

            return false;
        }
    }
}
