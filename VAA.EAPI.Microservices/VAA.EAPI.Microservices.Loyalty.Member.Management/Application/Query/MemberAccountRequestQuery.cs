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
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class MemberAccountRequestQuery : IRequest<Result<Response<MemberAccountResponse>>>
    {
        public string CustomerId { get; set; }
        public string ProductId { get; set; }
    }

    public class MemberAccountRequestQueryHandler : IRequestHandler<MemberAccountRequestQuery, Result<Response<MemberAccountResponse>>>
    {
        private readonly IMemberAccountDetailsService _memberAccountService;
        private readonly ILogger<MemberAccountRequestQueryHandler> _logger;

        public MemberAccountRequestQueryHandler(IMemberAccountDetailsService memberAccountService, ILogger<MemberAccountRequestQueryHandler> logger)
        {
            _memberAccountService = memberAccountService ?? throw new ArgumentNullException(nameof(memberAccountService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<MemberAccountResponse>>> Handle(MemberAccountRequestQuery request, CancellationToken cancellationToken)
        {
            try
            {

                var documentDataResponse = await _memberAccountService.GetMemberAccountDetails(request);
                if (documentDataResponse.IsSuccess)
                {
                    return documentDataResponse;
                }
                else
                {
                    return Result.Failure<Response<MemberAccountResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCustomerId), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in Get Document Data Query Handler with Customer id:{request.CustomerId}");
                return Result.Failure<Response<MemberAccountResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCustomerId), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
        }
    }
}
