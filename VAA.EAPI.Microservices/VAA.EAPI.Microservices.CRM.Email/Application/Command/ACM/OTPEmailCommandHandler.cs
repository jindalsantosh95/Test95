using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Email;
using VAA.EAPI.Infrastructure.Email.Interface;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.CRM.Email.Mapper;

namespace VAA.EAPI.Microservices.CRM.Email.Application.Command.ACM
{
    public class OTPEmailCommandHandler : IRequestHandler<OTPEmailCommand, bool>
    {
        private readonly ILogger<OTPEmailCommandHandler> _logger;
        private readonly IACMClient _client;

        public OTPEmailCommandHandler(ILogger<OTPEmailCommandHandler> logger, IACMClient client)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<bool> Handle(OTPEmailCommand request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var acmRequest = OTPEmailToACMRequestMapper.MapRequest(Json.Deserialize<SendEmailRequest>(Convert.ToString(request.Data)));

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
            if (!publishFails) { _logger.LogError("error occurred while sending OTP email failed"); }
            return publishFails;
        }
    }
}
