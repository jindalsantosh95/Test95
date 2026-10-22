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
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Command.MemberAccountDetails;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class MemberAccountReferenceDataQuery : IRequest<Result<Response<MemberAccountReferenceData>>>
    {
    }
    public class MemberAccountReferenceDataQueryHandler : IRequestHandler<MemberAccountReferenceDataQuery, Result<Response<MemberAccountReferenceData>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<MemberAccountReferenceDataQueryHandler> _logger;
        public MemberAccountReferenceDataQueryHandler(IMediator mediator, ILogger<MemberAccountReferenceDataQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<MemberAccountReferenceData>>> Handle(MemberAccountReferenceDataQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Member Account Reference Data Query Handler - Start");
                var memberAccountReferenceData = await _mediator.Send(new MemberAccountReferenceDataCommand { });
                if (memberAccountReferenceData.IsSuccess)
                {
                    return memberAccountReferenceData;
                }
                else
                {
                    return Result.Failure<Response<MemberAccountReferenceData>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCustomerId), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in Getting Member Account Reference Data");
                return Result.Failure<Response<MemberAccountReferenceData>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCustomerId), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
            finally
            {
                _logger.LogInformation($"Member Account Reference Data executed");
            }

        }
    }
}
