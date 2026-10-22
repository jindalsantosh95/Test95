using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Book.Domain.RefData.Requests;
using VAA.EAPI.Web.Book.Models.RefData;

namespace VAA.EAPI.Web.Book.Application.Query
{
    public class RetrieveCountriesRequestHandler : IRequestHandler<RetrieveCountriesRequest, IEnumerable<Country>>
    {
        private readonly IMediator _mediator;

        public RetrieveCountriesRequestHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
        public async Task<IEnumerable<Country>> Handle(RetrieveCountriesRequest request, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new CountriesRetrieveRequest(), cancellationToken);
        }
    }
}
