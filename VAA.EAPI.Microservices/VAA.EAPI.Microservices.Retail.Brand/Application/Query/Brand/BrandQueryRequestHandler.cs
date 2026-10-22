using MediatR;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Brand.Infrastructure.ATPCO.Request;

namespace VAA.EAPI.Microservices.Retail.Brand.Application.Query.Brand
{
    public class BrandQueryRequestHandler : IAtpcoCommand
    {
        private readonly IMediator _mediator;

        public BrandQueryRequestHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        public async Task<Models.Brand> Send(BrandQueryRequest request, CancellationToken cancellationToken)
        {
            return (Models.Brand)(await _mediator.Send((ATPCORequest)request, cancellationToken), request);
        }
    }
}
