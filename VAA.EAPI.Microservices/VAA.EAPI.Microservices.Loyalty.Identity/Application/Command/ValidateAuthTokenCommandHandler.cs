using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Constants;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.Interface;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Loyalty.Identity.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command
{
    public class ValidateAuthTokenCommandHandler : IRequestHandler<ValidateAuthTokenCommand, Response<string>>
    {
        private readonly IAuthTokenValidator _authTokenValidator;
        private readonly ILogger<ValidateAuthTokenCommand> _logger;
        private readonly IVerifyMemberStatusRepository _verifyMemberStatusRepository;


        public ValidateAuthTokenCommandHandler(IAuthTokenValidator authTokenValidator, ILogger<ValidateAuthTokenCommand> logger, IVerifyMemberStatusRepository verifyMemberStatusRepository)
        {
            _authTokenValidator = authTokenValidator ?? throw new ArgumentNullException(nameof(authTokenValidator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _verifyMemberStatusRepository = verifyMemberStatusRepository ?? throw new ArgumentNullException(nameof(verifyMemberStatusRepository));
        }

        public async Task<Response<string>> Handle(ValidateAuthTokenCommand request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            try
            {
                IEnumerable<Claim> tokenResponse = await _authTokenValidator.Verify(request.Token);

                if (tokenResponse.Any())
                {
                    TokenClaimResponse response = AuthTokenResponseMapper.ResponseMapper(tokenResponse);

                    if (response.EmailAddress.IsNullOrWhiteSpace())
                    {
                        response.EmailAddress = tokenResponse.FirstOrDefault(claim => claim.Type.Is("email"))?.Value;
                        MemberInfo customerResponse;

                        if (response.EmailAddress.IsNullOrWhiteSpace())
                        {
                            response.Oid = tokenResponse.FirstOrDefault(claim => claim.Type.Is("oid"))?.Value;
                            customerResponse = await _verifyMemberStatusRepository.VerifyMemberStatus(string.Empty, response.Oid, cancellationToken);
                            response.EmailAddress = customerResponse?.MemberEmail;
                        }
                        else
                        {
                            customerResponse = await _verifyMemberStatusRepository.VerifyMemberStatus(response.EmailAddress, string.Empty, cancellationToken);
                        }

                        if (customerResponse != null)
                        {
                            if (response.MemberStatus.IsNullOrWhiteSpace()) response.MemberStatus = customerResponse.MemberStatus;
                            if (response.FcNumber.IsNullOrWhiteSpace()) response.FcNumber = customerResponse.MemberNumber;
                        }
                    }

                    return new Response<string>()
                    {
                        Status = Status.OK,
                        Value = Common.Serialisation.Json.Serialize(response),
                        Errors = null
                    };
                }

                return GetErrorResponse(ErrorCodes.TokenInvalidOrExpiredErrorCode);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token Decryption failed due to an unexpected technical error.");
                return GetErrorResponse(ErrorCodes.TokenDecryptionErrorCode);
            }
        }
        private static Response<string> GetErrorResponse(string errorCode)
        {

            List<ErrorDetails> errorList = new()
            {
                ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault()
            };

            return new Response<string>()
            {
                Status = Status.BusinessException,
                Errors = errorList
            };

        }
    }
}
