using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Reference.Domain;
using VAA.EAPI.Microservices.Retail.Reference.Domain.Interface;

namespace VAA.EAPI.Microservices.Retail.Reference.Application.Query
{
    public class TierThresholdQueryHandler : IRequestHandler<TierThresholdQuery, IEnumerable<TierThresholdValues>>
    {
        private readonly IScvRefDBRepository _scvRefDBRepository;

        public TierThresholdQueryHandler(IScvRefDBRepository scvRefDBRepository)
        {
            _scvRefDBRepository = scvRefDBRepository ?? throw new ArgumentNullException(nameof(scvRefDBRepository));
        }

        public async Task<IEnumerable<TierThresholdValues>> Handle(TierThresholdQuery request, CancellationToken cancellationToken)
        {
            return await _scvRefDBRepository.GetTierThresholdDetails();
        }
    }
}
