using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Domain;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Interface;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Query
{
    public class PartnerProfileQueryHandler : IPartnerProfileQueryHandler
    {
        private readonly IScvRefDBRepository _scvRefDBRepository;

        public PartnerProfileQueryHandler(IScvRefDBRepository scvRefDBRepository)
        {
            _scvRefDBRepository = scvRefDBRepository ?? throw new ArgumentNullException(nameof(scvRefDBRepository));
        }

        public async Task<Result<Response<IEnumerable<PartnerProfileDetails>>>> Handle(PartnerProfileQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return Result.Success(new Response<IEnumerable<PartnerProfileDetails>>
                {
                    Status = Status.OK,
                    Value = await _scvRefDBRepository.GetPartnerCodeDetails()
                });
            }
            catch (Exception ex)
            {
                return Result.Failure<Response<IEnumerable<PartnerProfileDetails>>>(ex.Message.ToString());
            }
        }
    }
}
