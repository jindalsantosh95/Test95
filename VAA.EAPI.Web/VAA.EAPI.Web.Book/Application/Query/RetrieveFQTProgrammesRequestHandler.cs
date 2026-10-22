using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Book.Domain.RefData.Requests;
using VAA.EAPI.Web.Book.Models.RefData;

namespace VAA.EAPI.Web.Book.Application.Query
{
    public class RetrieveFQTProgrammesRequestHandler : IRequestHandler<RetrieveFQTProgrammesRequest, IEnumerable<FQTProgramme>>
    {
        private readonly IMediator _mediator;

        public RetrieveFQTProgrammesRequestHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<IEnumerable<FQTProgramme>> Handle(RetrieveFQTProgrammesRequest request, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new FQTProgrammesRetrieveRequest(), cancellationToken);
        }
    }
}
