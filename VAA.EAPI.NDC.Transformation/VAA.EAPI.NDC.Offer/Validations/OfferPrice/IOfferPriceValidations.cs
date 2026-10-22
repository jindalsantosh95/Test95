using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OfferPriceRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.UpsellOffers;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;

namespace VAA.EAPI.NDC.Offer.Validations.OfferPrice
{
    public interface IOfferPriceValidations<T>
    {
        Response<T> ValidateOfferPriceVaRequest(IATA_OfferPriceRQ offerPriceRQ, IHostEnvironment hostingEnvironment);
        Response<T> ValidateDbOffer(OfferResponse airlineOffers, string offerId, IHostEnvironment hostingEnvironment);
        Response<T> ValidateAgentAndPax(OfferResponse offerDB, IATA_OfferPriceRQ offerPricerRequest, IHostEnvironment hostingEnvironment, ValidationRS validationRS, IGenericDataMapper genericDataMapper);
        Response<T> ValidateOfferPriceResponse(OfferResponse offer, IHostEnvironment hostingEnvironment);
        Response<T> ValidateAgency(ValidationRS validationRS, IHostEnvironment hostingEnvironment);
        Response<T> ValidateUpsellConfigResponse(IEnumerable<UpsellOffersData> upsellConfigResponse, IHostEnvironment hostingEnvironment);
        Response<T> CreateTechnicalErrorResponse(IHostEnvironment hostingEnvironment);
        Response<T> ValidateAgentInRequest(OfferResponse offerDocument, DistributionChainLinkType[] distributionChains, IHostEnvironment hostEnvironment);

    }
}
