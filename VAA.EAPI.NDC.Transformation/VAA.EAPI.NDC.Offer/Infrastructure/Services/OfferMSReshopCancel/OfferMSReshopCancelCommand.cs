using MediatR;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMSReshopCancel;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.OfferMSReshopCancel
{
    public class OfferMSReshopCancelCommand : IRequest<CancelReshopResponse>
    {
        public OrderReshopRequest OrderReshopRequest { get; set; }
    }
}
