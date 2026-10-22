using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MemberAccountDetails;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Validations;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class MemberAccountDetailsQuery : IRequest<Result<Response<MemberAccountResponse>>>
    {
        public string CustomerId { get; set; }
    }
    public class MemberAccountDetailsQueryHandler : IRequestHandler<MemberAccountDetailsQuery, Result<Response<MemberAccountResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<MemberAccountDetailsQueryHandler> _logger;
        public MemberAccountDetailsQueryHandler(IMediator mediator, ILogger<MemberAccountDetailsQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public virtual async Task<Result<Response<MemberAccountResponse>>> Handle(MemberAccountDetailsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var memberAccountDetailRequestValidator = new MemberAccountDetailQueryValidator().Validate(request);
                if (memberAccountDetailRequestValidator.IsValid)
                {
                    _logger.LogInformation("Retrieving Member account Details");
                    var memberAccountDetailsResponse = await _mediator.Send(new GetMemberAccountDetailsQuery { CustomerId = request.CustomerId });
                    return memberAccountDetailsResponse;
                }
                else
                {
                    return Result.Failure<Response<MemberAccountResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MandatoryInputFieldsMissing), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in Get Member Account Details Response Query Handler with Customer id:{request.CustomerId}");
                return Result.Failure<Response<MemberAccountResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            finally
            {
                _logger.LogInformation($"Get Member Account Details Response Query Handler with Customer id:{request.CustomerId}");
            }
        }
    }
}
