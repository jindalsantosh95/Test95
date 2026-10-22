using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Email.Interface;
using VAA.EAPI.Microservices.CRM.Email.Mapper;

namespace VAA.EAPI.Microservices.CRM.Email.Application.Command.ACM
{
    public class VACEnrolEmailCommandHandler : IRequestHandler<VACEnrolEmailCommand, bool>
    {
        private readonly IACMClient _client;
        private readonly ILogger<EmailCommandHandler> _logger;

        public VACEnrolEmailCommandHandler(ILogger<EmailCommandHandler> logger, IACMClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(VACEnrolEmailCommand request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            try
            {
                request.B2CPasswordResetLink ??= ConfigReader.Get(Constants.Environment.B2CPasswordResetLink);
                request.VACEnrolEmailTemplate ??= ConfigReader.Get(Constants.Environment.VACEnrolEmailTemplate);

                var acmRequest = VACEnrolEmailCommandRequestMapper.MapRequest(request);

                var retry = ConfigReader.Get<int>(Constants.Environment.RetryCounter);
                bool publishFails = false;

                for (int i = 0; i < retry; i++)
                {
                    if (await _client.SendEmail(acmRequest))
                    {
                        publishFails = true;
                        break;
                    }

                    await Task.Delay(ConfigReader.Get<TimeSpan>(Constants.Environment.DelayTime), cancellationToken);
                }

                if (publishFails)
                {
                    _logger.LogDebug("Communication is sent successfully");
                    return true;
                }

                else
                {
                    _logger.LogError("Sending Communication is failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Sending Communication is failed - {ex}");
                return false;
            }
        }
    }
}
