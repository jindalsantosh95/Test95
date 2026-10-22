using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Constants;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories
{
    public class UpdateUserRepoCommandHandler : IRequestHandler<UpdateUserRepoCommand, B2CUserResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateUserRepoCommandHandler> _logger;

        public UpdateUserRepoCommandHandler(IMediator mediator, ILogger<UpdateUserRepoCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<B2CUserResponse> Handle(UpdateUserRepoCommand request, CancellationToken cancellationToken)
        {
            var userResponse = await _mediator.Send(new PatchUserServiceCommand("/users/", IdentityJsonRequest(), request.ObjectID), cancellationToken);

            if (userResponse.IsB2COperationSuccess)
                return userResponse;

            _logger.LogError("B2C enrolment failed. ErrorCode: {ErrorCode}", ErrorCodes.B2CBusinessError);

            return userResponse;
        }

        private static string IdentityJsonRequest()
        {
            return Json.Serialize(new Dictionary<string, object>
            {
                { ConfigReader.Get(Environment.AccountEnabled),true },

            }, false);
        }
    }
}
