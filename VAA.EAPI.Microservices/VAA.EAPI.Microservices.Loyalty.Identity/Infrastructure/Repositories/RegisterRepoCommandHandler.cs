using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MediatR;

using Microsoft.Extensions.Logging;

using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Identity.Constants;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.IdentityEntities;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;

using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories
{
    public class RegisterRepoCommandHandler : IRequestHandler<RegisterRepoCommand, B2CUserResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RegisterRepoCommandHandler> _logger;

        public RegisterRepoCommandHandler(IMediator mediator, ILogger<RegisterRepoCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<B2CUserResponse> Handle(RegisterRepoCommand request, CancellationToken cancellationToken)
        {
            var userResponse = await _mediator.Send(new RegisterServiceCommand("/users", IdentityJsonRequest(IdentityRequestMapper.MapRegisterRequest(request.MemberRegisterRequest, request.AccountEnabled))), cancellationToken);

            if (userResponse.IsB2COperationSuccess)
                return userResponse;

            _logger.LogError("B2C enrolment failed. ErrorCode: {ErrorCode}", ErrorCodes.B2CBusinessError);

            return userResponse;
        }

        private static string IdentityJsonRequest(MemberEnrolmentGraphRequest memberEnrolmentGraphRequest)
        {
            return Json.Serialize(new Dictionary<string, object>
            {
                { ConfigReader.Get(Environment.AccountEnabled),memberEnrolmentGraphRequest.AccountEnabled },
                { ConfigReader.Get(Environment.Surname) ,memberEnrolmentGraphRequest.Surname},
                { ConfigReader.Get(Environment.DisplayName),memberEnrolmentGraphRequest.DisplayName },
                { ConfigReader.Get(Environment.GivenName),memberEnrolmentGraphRequest.GivenName },
                { ConfigReader.Get(Environment.PasswordProfile),memberEnrolmentGraphRequest.PasswordProfile },
                { ConfigReader.Get(Environment.PasswordPolicies),memberEnrolmentGraphRequest.PasswordPolicies },
                { ConfigReader.Get(Environment.UserPrincipalName),memberEnrolmentGraphRequest.UserPrincipalName },
                { ConfigReader.Get(Environment.Identities),memberEnrolmentGraphRequest.Identities },
                { $"{ConfigReader.Get(Environment.ExtensionPrefix)}{ConfigReader.Get(Environment.CustomMemberTypeAttribute)}",memberEnrolmentGraphRequest.MemberType },
                { $"{ConfigReader.Get(Environment.ExtensionPrefix)}{ConfigReader.Get(Environment.CustomMemberStatusAttribute)}",memberEnrolmentGraphRequest.MemberStatus },
                { $"{ConfigReader.Get(Environment.ExtensionPrefix)}{ConfigReader.Get(Environment.CustomCustomerIdAttribute)}", memberEnrolmentGraphRequest.CustomerId },
            }, false);
        }
    }
}
