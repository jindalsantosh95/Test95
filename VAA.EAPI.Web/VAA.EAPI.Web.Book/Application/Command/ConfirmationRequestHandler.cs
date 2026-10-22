using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Book.Domain.Book.Requests;
using VAA.EAPI.Web.Book.Models.Book.Confirmation;
using VAA.EAPI.Web.Common.Entities;

namespace VAA.EAPI.Web.Book.Application.Command
{
    public class ConfirmationRequestHandler : IRequestHandler<ConfirmationRequest, Response<ConfirmationResponse>>
    {
        private readonly IMediator _mediator;

        public ConfirmationRequestHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        public async Task<Response<ConfirmationResponse>> Handle(ConfirmationRequest request, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new OrderConfirmationRequest { Request = new Request { BasketId = request.BasketId, PaymentStatus = request.PaymentStatus, DeviceInfo = Constants.Application.DefalutDeviceInfo, BrowserInfo = request.BrowserInfo, IpAddress = request.IpAddress, CcCheckConsent = request.FraudCheckConsentTaken, MetaPartner = request.MetaPartner?.Select(kvp => kvp) }, AuthToken = request.AuthToken }, cancellationToken);
        }
    }
}
