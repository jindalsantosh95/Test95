using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.Enrolment
{
    public class LoginValidationQueryCommandHandler : IRequestHandler<LoginValidationQuery, string>
    {
        private readonly ILogger<LoginValidationQueryCommandHandler> _logger;
        private readonly IMediator _mediator;

        public LoginValidationQueryCommandHandler(ILogger<LoginValidationQueryCommandHandler> logger, IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> Handle(LoginValidationQuery request, CancellationToken cancellationToken)
        {
            var status = await _mediator.Send(new MemberLoginRepoQuery(request), cancellationToken);

            if (!string.IsNullOrEmpty(status) && status == Constants.Application.ActivePendingUser)
            {
                return Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.Memberstatus, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath));
            }
            return string.Empty;
        }
    }

}
