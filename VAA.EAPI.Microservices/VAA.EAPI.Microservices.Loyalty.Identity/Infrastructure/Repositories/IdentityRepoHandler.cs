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
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.IdentityEntities;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories
{
    public class IdentityRepoHandler : IRequestHandler<IdentityRepoCommand, bool>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<IdentityRepoHandler> _logger;

        public IdentityRepoHandler(IMediator mediator, ILogger<IdentityRepoHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<bool> Handle(IdentityRepoCommand request, CancellationToken cancellationToken)
        {

            MemberEnrolmentGraphRequest graphAzureB2Crequest = IdentityRequestMapper.MapIdentityrequest(request, request.IsAccountEnabled);

            string jsonContent = IdentityJsonRequest(graphAzureB2Crequest);

            if (!await _mediator.Send(new IdentityServiceCommand("/users", jsonContent), cancellationToken))
            {
                _logger.LogError("B2C enrolment failed. ErrorCode: {ErrorCode}", ErrorCodes.B2CBusinessError);
                return false;
            }
            return true;

        }

        private static string IdentityJsonRequest(MemberEnrolmentGraphRequest graphAzureB2Crequest)
        {
            var graphRequestWithCustomAttributes = new Dictionary<string, object>
            {
                { ConfigReader.Get(Environment.AccountEnabled),graphAzureB2Crequest.AccountEnabled },
                { ConfigReader.Get(Environment.Surname) ,graphAzureB2Crequest.Surname},
                { ConfigReader.Get(Environment.DisplayName),graphAzureB2Crequest.DisplayName },
                { ConfigReader.Get(Environment.GivenName),graphAzureB2Crequest.GivenName },
                { ConfigReader.Get(Environment.PasswordProfile),graphAzureB2Crequest.PasswordProfile },
                { ConfigReader.Get(Environment.UserPrincipalName),graphAzureB2Crequest.UserPrincipalName },
                { ConfigReader.Get(Environment.MailNickName),graphAzureB2Crequest.MailNickname },
                { ConfigReader.Get(Environment.Identities),graphAzureB2Crequest.Identities },
                { $"{ConfigReader.Get(Environment.ExtensionPrefix)}{ConfigReader.Get(Environment.CustomMemberTypeAttribute)}", graphAzureB2Crequest.MemberType },
                { $"{ConfigReader.Get(Environment.ExtensionPrefix)}{ConfigReader.Get(Environment.CustomMemberStatusAttribute)}", graphAzureB2Crequest.MemberStatus },
                { $"{ConfigReader.Get(Environment.ExtensionPrefix)}{ConfigReader.Get(Environment.CustomSecurityQuestion1Attribute)}", graphAzureB2Crequest.SecurityQuestion1 },
                { $"{ConfigReader.Get(Environment.ExtensionPrefix)}{ConfigReader.Get(Environment.CustomSecurityQuestion2Attribute)}", graphAzureB2Crequest.SecurityQuestion2 },
                { $"{ConfigReader.Get(Environment.ExtensionPrefix)}{ConfigReader.Get(Environment.CustomSecurityAnswer1Attribute)}", graphAzureB2Crequest.SecurityAnswer1 },
                { $"{ConfigReader.Get(Environment.ExtensionPrefix)}{ConfigReader.Get(Environment.CustomSecurityAnswer2Attribute)}", graphAzureB2Crequest.SecurityAnswer2 },
                { $"{ConfigReader.Get(Environment.ExtensionPrefix)}{ConfigReader.Get(Environment.CustomCustomerIdAttribute)}", graphAzureB2Crequest.CustomerId },
            };
            return Json.Serialize(graphRequestWithCustomAttributes, false);
        }
    }
}
