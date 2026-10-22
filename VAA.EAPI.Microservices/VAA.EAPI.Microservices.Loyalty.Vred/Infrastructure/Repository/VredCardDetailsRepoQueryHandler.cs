using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredCardDetails;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository
{
    public class VredCardDetailsRepoQueryHandler : IRequestHandler<VredCardDetailsRepoQuery, VredCardDetailsResponse>
    {
        private readonly IMediator _mediator;

        public VredCardDetailsRepoQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<VredCardDetailsResponse> Handle(VredCardDetailsRepoQuery request, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new VredCardDetailsServiceQuery(request.MemberNumber), cancellationToken);
        }
    }
}

