using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredStatements;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository
{
    public class VredStatementsRepoHandler : IRequestHandler<VredStatementsRepoQuery, VredStatementResponse>
    {
        private readonly IMediator _mediator;

        public VredStatementsRepoHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
        public async Task<VredStatementResponse> Handle(VredStatementsRepoQuery request, CancellationToken cancellationToken)
        {
            VredStatementResponse vredStatementResponse = await _mediator.Send(new VredStatementsServiceQuery(request.MemberNumber), cancellationToken);

            vredStatementResponse.Statements = vredStatementResponse?.Statements?.OrderByDescending(item => item.ActivityDate).ToList();

            return vredStatementResponse;
        }
    }
}
