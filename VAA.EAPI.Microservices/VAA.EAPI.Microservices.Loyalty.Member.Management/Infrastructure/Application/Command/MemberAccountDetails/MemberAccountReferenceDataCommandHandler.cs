using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MemberAccountDetails;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Helper.Cache.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Command.MemberAccountDetails
{
    public class MemberAccountReferenceDataCommandHandler : IRequestHandler<MemberAccountReferenceDataCommand, Result<Response<MemberAccountReferenceData>>>
    {
        private readonly ICachedMemberAccountReferenceData _cachedMemberAccountReferenceData;
        private readonly ILogger<MemberAccountReferenceDataCommandHandler> _logger;

        public MemberAccountReferenceDataCommandHandler(ICachedMemberAccountReferenceData cachedMemberAccountReferenceData, ILogger<MemberAccountReferenceDataCommandHandler> logger)
        {
            _cachedMemberAccountReferenceData = cachedMemberAccountReferenceData ?? throw new ArgumentNullException(nameof(cachedMemberAccountReferenceData));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<MemberAccountReferenceData>>> Handle(MemberAccountReferenceDataCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Member Account Reference Data command handler process started");
            try
            {
                var memberAccountReferenceData = await _cachedMemberAccountReferenceData.GetCachedMemberAccountReferenceData();
                return memberAccountReferenceData;
            }
            finally
            {
                _logger.LogInformation("Member Account Reference Data command handler process ended");
            }
        }
    }
}
