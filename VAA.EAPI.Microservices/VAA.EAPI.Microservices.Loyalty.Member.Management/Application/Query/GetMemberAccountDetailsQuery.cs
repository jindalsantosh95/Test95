using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayBonus;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MemberAccountDetails;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.MemberAccountDetails;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class GetMemberAccountDetailsQuery : IRequest<Result<Response<MemberAccountResponse>>>
    {
        public string CustomerId { get; set; }
    }

    public class GetMemberAccountDetailsQueryHandler : IRequestHandler<GetMemberAccountDetailsQuery, Result<Response<MemberAccountResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetMemberAccountDetailsQueryHandler> _logger;

        public GetMemberAccountDetailsQueryHandler(IMediator mediator, ILogger<GetMemberAccountDetailsQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<MemberAccountResponse>>> Handle(GetMemberAccountDetailsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                Product productDetail = await _mediator.Send(new GetProductRefrenceDetailsQuery { ProductName = Constants.Application.AwardType });
                var documentDataResponse = await _mediator.Send(request: new MemberAccountRequestQuery { CustomerId = request.CustomerId, ProductId = productDetail?.ProductId });
                var memberReferenceData = await _mediator.Send(new MemberAccountReferenceDataQuery { });

                if (documentDataResponse.IsSuccess && memberReferenceData.IsSuccess)
                {
                    MemberAccountResponse documentData = documentDataResponse.Value.Value;
                    MemberAccountReferenceData referenceData = memberReferenceData.Value.Value;
                    var result = MemberAccountResponseMapping.ResponseMapping(documentData, referenceData);

                    return new Response<MemberAccountResponse>
                    {
                        Status = Status.OK,
                        Value = result
                    };
                }
                else
                {
                    return Result.Failure<Response<MemberAccountResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCustomerId), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in Get Member Account Details Response Query Handler with Customer id:{request.CustomerId}");
                return Result.Failure<Response<MemberAccountResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCustomerId), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
        }
    }
}
