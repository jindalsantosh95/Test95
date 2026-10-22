using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Email.Interface;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.CRM.Email.Mapper;

namespace VAA.EAPI.Microservices.CRM.Email.Application.Command.ACM
{
    public class AdobeMemberInviteEmailCommandHandler : IRequestHandler<AdobeMemberInviteEmailCommand, Response<string>>
    {
        private readonly IACMClient _client;

        public AdobeMemberInviteEmailCommandHandler(IACMClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<Response<string>> Handle(AdobeMemberInviteEmailCommand request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var acmRequest = AdobeMemberInviteEmailCommandRequestMapper.MapRequest(request);

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

            if (!publishFails)
            {
                return new Response<string>()
                {
                    Status = Status.Error,
                    Value = "Failed to Invite Member",
                    Errors = null
                };
            }

            return new Response<string>()
            {
                Status = Status.OK,
                Value = "Invited Member Successfully",
                Errors = null
            };

        }
    }
}
