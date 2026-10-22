using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Offer.Domain;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.Reshop
{
    public class OrderReshopSliceBySliceQueryHandler : IRequestHandler<OrderReshopSlicebySliceQuery, Result<Response<OfferResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderReshopSliceBySliceQueryHandler> _logger;

        public OrderReshopSliceBySliceQueryHandler(IMediator mediator, ILogger<OrderReshopSliceBySliceQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<Result<Response<OfferResponse>>> Handle(OrderReshopSlicebySliceQuery requestData, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();

        }
    }
}
