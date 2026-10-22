using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository
{
    public class VirginPointsRepoHandler : IRequestHandler<VirginPointsRepoQuery, VRedResponse>
    {
        private readonly IMediator _mediator;

        public VirginPointsRepoHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<VRedResponse> Handle(VirginPointsRepoQuery request, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new VirginPointsServiceQuery(request.MemberID));
        }

    }
}
