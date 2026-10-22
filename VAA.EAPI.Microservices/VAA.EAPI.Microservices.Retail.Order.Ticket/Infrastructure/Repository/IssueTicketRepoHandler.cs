using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Repository;
using VAA.EAPI.Infrastructure.PSS.Repository.Command;
using VAA.EAPI.Infrastructure.PSS.Repository.Query;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.IssueTicket;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Infrastructure.Mapper;


namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Infrastructure.Repository
{
    public class IssueTicketRepoCommandHandler : IRequestHandler<IssueTicketRepoCommand, IssueTicketResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<IssueTicketRepoCommandHandler> _logger;

        public IssueTicketRepoCommandHandler(IMediator mediator, ILogger<IssueTicketRepoCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<IssueTicketResponse> Handle(IssueTicketRepoCommand request, CancellationToken cancellationToken)
        {
            string sessionId = await _mediator.Send(new PNRSetupContextRepoQuery { RecordLocator = request.IssueTicketRequest.BookingReferences.RLOC }, cancellationToken);

            var payerAddress = request.IssueTicketRequest.paymentDetails.First().CardMethod.BillingAddress;

            string fraudRemarks = await _mediator.Send(new VerifyFraudCheckQuery { Country = payerAddress.Country, State = payerAddress.Region, Pos = request.IssueTicketRequest.pos.CityCode, SessionId = sessionId }, cancellationToken);

            if (string.Equals(fraudRemarks, Constants.Application.CreditCardFraudCheckMessage, StringComparison.OrdinalIgnoreCase))
            {
                var paymentDetails = request.IssueTicketRequest.paymentDetails.First();
                await _mediator.Send(new AddRemarksCommand { Remarks = FraudCheckRemarks.PnrRemarks(paymentDetails.CardMethod.CardNumber, paymentDetails.CardMethod.CardType.Value.ToString()), SessionId = sessionId }, cancellationToken);
            }

            var response = await _mediator.Send(new TicketingPerformRevenueQuery(sessionId, request.IssueTicketRequest.paymentDetails,
                                          request.IssueTicketRequest.CustomerDetails), cancellationToken);
            return BookingResponseMapper.ResponseMapper(response);

        }
    }
}
