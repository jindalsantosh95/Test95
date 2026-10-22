using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands
{
    public class VirginPointsQueryHandler : IRequestHandler<VirginPointsQuery, VRedResponse>
    {
        private readonly IMediator _mediator;
        public VirginPointsQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<VRedResponse> Handle(VirginPointsQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.MemberID))
                throw new ArgumentNullException(nameof(request.MemberID));

            return await _mediator.Send(new VirginPointsRepoQuery(request.MemberID));
        }
    }


}
