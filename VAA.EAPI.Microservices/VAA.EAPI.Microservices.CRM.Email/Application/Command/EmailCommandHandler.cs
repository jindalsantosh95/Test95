using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Email;
using VAA.EAPI.Infrastructure.Email.Interface;

namespace VAA.EAPI.Microservices.CRM.Email.Application.Command
{
    public class EmailCommandHandler : IRequestHandler<EmailEventProcessCommand, bool>
    {
        private readonly ILogger<EmailCommandHandler> _logger;
        private readonly ICcpRepository _client;

        public EmailCommandHandler(ILogger<EmailCommandHandler> logger, ICcpRepository repository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<bool> Handle(EmailEventProcessCommand request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            try
            {
                if (await _client.SendEmail(Json.Deserialize<SendEmailRequest>(Convert.ToString(request.Data))))
                {
                    _logger.LogInformation(" Email is sent successfully");
                    return true;
                }
                else
                {
                    _logger.LogInformation(" Sending email failed");
                    return false;
                }
            }
            catch (Exception exception)
            {
                _logger.LogInformation($"Sending email failed - {exception}");
            }

            return false;
        }
    }
}
