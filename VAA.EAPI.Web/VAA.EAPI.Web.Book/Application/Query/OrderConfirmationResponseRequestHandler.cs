using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Book.Domain.Book.Requests;
using VAA.EAPI.Web.Book.Models.Book.Confirmation;

namespace VAA.EAPI.Web.Book.Application.Query
{
    public class OrderConfirmationResponseRequestHandler : IRequestHandler<OrderConfirmationResponseRequest, ConfirmationResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderConfirmationResponseRequestHandler> _logger;

        public OrderConfirmationResponseRequestHandler(IMediator mediator, ILogger<OrderConfirmationResponseRequestHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<ConfirmationResponse> Handle(OrderConfirmationResponseRequest request, CancellationToken cancellationToken)
        {            
            return await _mediator.Send(new OrderResponseRequest() { BasketId = request.BasketId, AuthToken = request.AuthToken }, cancellationToken);
        }
    }
}
