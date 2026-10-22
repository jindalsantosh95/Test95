using MediatR;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Book.Domain.Payment.Requests;
using VAA.EAPI.Web.Book.Models.Book.Payment;

namespace VAA.EAPI.Web.Book.Application.Command
{
    public class PaymentRefIdRequestHandler : IRequestHandler<PaymentRefIdRequest, PaymentResponse>
    {
        private readonly IMediator _mediator;

        public PaymentRefIdRequestHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        public async Task<PaymentResponse> Handle(PaymentRefIdRequest request, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new PaymentReferenceIdRequest { BasketId = request.BasketId, AuthToken = request.AuthToken, ExpiryUrl = request.ExpiryUrl, Link = request.Link, ReturnUrl = request.ReturnUrl, DeviceInfo = request.DeviceInfo, BrowserInfo = request.BrowserInfo, IpAddress = request.IpAddress, PayWithPointsValue = request.PayWithPointsValue }, cancellationToken);
        }
    }
}
