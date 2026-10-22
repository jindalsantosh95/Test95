using MediatR;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.OrderQuote;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.OrderQuote
{
    public class OrderQuoteRepriceQuery : RepriceRequest, IRequest<OfferResponse>
    {
    }
}
