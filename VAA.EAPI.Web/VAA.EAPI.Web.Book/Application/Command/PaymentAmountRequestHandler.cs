using MediatR;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Book.Domain.Payment.Requests;
using VAA.EAPI.Web.Book.Models.Book.Payment;

namespace VAA.EAPI.Web.Book.Application.Command
{
    public class PaymentAmountRequestHandler : IRequestHandler<PaymentAmountRequest, PaymentResponse>
    {
        private readonly IMediator _mediator;

        public PaymentAmountRequestHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        public async Task<PaymentResponse> Handle(PaymentAmountRequest request, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new PaymentAmountRequestIdRequest { BasketId = request.BasketId, AuthToken = request.AuthToken, AmountToPay = request.AmountToPay, Link = request.Link, IpAddress = request.IpAddress, BrowserInfo = request.BrowserInfo }, cancellationToken);
        }
    }
}
