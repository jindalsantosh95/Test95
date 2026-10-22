using MediatR;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;


namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.OfferMSReshopNameCorrection
{
    public class OfferMSReshopNameCorrectionCommand : IRequest<OfferResponse>
    {
        public OrderReshopRequest OrderReshopRequest { get; set; }
    }
}

