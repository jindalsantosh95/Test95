using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Email.Interface;
using VAA.EAPI.Microservices.CRM.Email.Domain;
using VAA.EAPI.Microservices.CRM.Email.Mapper;

namespace VAA.EAPI.Microservices.CRM.Email.Application.Command.StoreAppointment
{
    public class StoreAppointmentEmailEventProcessCommandHandler : IRequestHandler<StoreAppointmentEmailEventProcessCommand, bool>
    {
        private readonly ILogger<StoreAppointmentEmailEventProcessCommandHandler> _logger;
        private readonly IACMClient _client;

        public StoreAppointmentEmailEventProcessCommandHandler(ILogger<StoreAppointmentEmailEventProcessCommandHandler> logger, IACMClient client)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<bool> Handle(StoreAppointmentEmailEventProcessCommand request, CancellationToken cancellationToken)
        {
            var payload = Json.Deserialize<StoreAppointmentCrmEmailEventData>(Convert.ToString(request.Data));
            if (payload == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(payload.EmailAddress) || string.IsNullOrWhiteSpace(payload.TemplateId) || string.IsNullOrWhiteSpace(payload.EmailDataXml))
            {
                return false;
            }

            var acmRequest = StoreAppointmentEmailToACMRequestMapper.MapRequest(payload);
            var retry = ConfigReader.Get<int>(Constants.Environment.RetryCounter);
            var publishSuccess = false;

            for (int i = 0; i < retry; i++)
            {
                if (await _client.SendEmail(acmRequest))
                {
                    publishSuccess = true;
                    break;
                }

                await Task.Delay(ConfigReader.Get<TimeSpan>(Constants.Environment.DelayTime), cancellationToken);
            }

            if (!publishSuccess)
            {
                _logger.LogError("StoreAppointment email publish failed for recipient: {Recipient}", payload.EmailAddress);
            }

            return publishSuccess;
        }
    }
}
