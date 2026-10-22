using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Infrastructure.PSS.Repository;

namespace VAA.EAPI.Microservices.Retail.Order.Create.Infrastructure.Repository
{
    public class OrderCreateRepoHandler : IRequestHandler<OrderCreateRepoCommand, BookingReference>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderCreateRepoHandler> _logger;

        public OrderCreateRepoHandler(IMediator mediator, ILogger<OrderCreateRepoHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<BookingReference> Handle(OrderCreateRepoCommand request, CancellationToken cancellationToken)
        {
            string pnr = await _mediator.Send(new CreatePnrRepoQuery(request.OrderCreateRequest.pos, request.OrderCreateRequest.parties, request.OrderCreateRequest.selectedOffers, request.OrderCreateRequest.paymentDetails, request.OrderCreateRequest.dataList.OriginDestinationAssociations, request.OrderCreateRequest.dataList.CustomerDetails, request.OrderCreateRequest.dataList.SegmentDetails, request.OrderCreateRequest.dataList.BaggageAllowances, request.OrderCreateRequest.dataList.PriceClass), cancellationToken);

            if (request.OrderCreateRequest.paymentDetails.IsNullOrEmpty())
            {
                return new BookingReference
                {
                    RLOC = pnr
                };
            }
            else
            {
                string sessionId = await _mediator.Send(new PNRSetupContextRepoQuery { RecordLocator = pnr }, cancellationToken);
                return (await _mediator.Send(new TicketingPerformRevenueQuery(sessionId, request.OrderCreateRequest.paymentDetails, request.OrderCreateRequest.dataList.CustomerDetails), cancellationToken)).BookingReference;
            }
        }
    }
}
