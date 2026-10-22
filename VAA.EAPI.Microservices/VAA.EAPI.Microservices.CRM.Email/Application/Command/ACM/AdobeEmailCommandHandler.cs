using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Email.Interface;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.CRM.Email.Mapper;

namespace VAA.EAPI.Microservices.CRM.Email.Application.Command.ACM
{
    public class AdobeEmailCommandHandler : IRequestHandler<AdobeEmailCommand, Response<bool>>
    {

        private readonly ILogger<AdobeEmailCommandHandler> _logger;
        private readonly IACMClient _client;

        public AdobeEmailCommandHandler(ILogger<AdobeEmailCommandHandler> logger, IACMClient client)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<Response<bool>> Handle(AdobeEmailCommand request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var acmRequest = MyAccountUpdatesEmailRequestMapper.MapRequest(request);

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

            if (!publishFails) { _logger.LogError($"error occurred while sending  email for {request.To}"); }

            return new Response<bool>()
            {
                Status = Status.OK,
                Value = publishFails,
                Errors = null
            };

        }
    }
}
