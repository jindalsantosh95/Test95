using MediatR;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Book.Domain.Payment.Requests;
using VAA.EAPI.Web.Book.Models.Book.Payment;

namespace VAA.EAPI.Web.Book.Application.Command
{
    internal class PaymentStatusRequestHandler : IRequestHandler<PaymentStatusRequest, PaymentStatusResponse>
    {
        private readonly IMediator _mediator;

        public PaymentStatusRequestHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        public async Task<PaymentStatusResponse> Handle(PaymentStatusRequest request, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new PaymentStatusGraphQLRequest { BasketId = request.BasketId, AuthToken = request.AuthToken, PaymentReferenceId = request.PaymentReferenceId, HppPaymentStatus = request.HppPaymentStatus }, cancellationToken);
        }
    }
}
