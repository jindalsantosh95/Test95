using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Reference.Domain;
using VAA.EAPI.Microservices.Retail.Reference.Domain.Interface;

namespace VAA.EAPI.Microservices.Retail.Reference.Application.Query
{
    public class PartnerProfileQueryHandler : IRequestHandler<PartnerProfileQuery, IEnumerable<PartnerProfileDetails>>
    {
        private readonly IScvRefDBRepository _scvRefDBRepository;

        public PartnerProfileQueryHandler(IScvRefDBRepository scvRefDBRepository)
        {
            _scvRefDBRepository = scvRefDBRepository ?? throw new ArgumentNullException(nameof(scvRefDBRepository));
        }

        public async Task<IEnumerable<PartnerProfileDetails>> Handle(PartnerProfileQuery request, CancellationToken cancellationToken)
        {
            return await _scvRefDBRepository.GetPartnerCodeDetails();
        }
    }
}
