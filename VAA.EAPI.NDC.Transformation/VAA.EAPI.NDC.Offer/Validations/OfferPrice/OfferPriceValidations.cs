using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OfferPriceRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.UpsellOffers;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;

namespace VAA.EAPI.NDC.Offer.Validations.OfferPrice
{
    public class OfferPriceValidations<T> : IOfferPriceValidations<T>
    {
        public Response<T> ValidateAgency(ValidationRS validationRS, IHostEnvironment hostingEnvironment)
        {
            var errorIds = RequestValidations.ValidateAgencyDetails(validationRS);

            return ErrorHelper<T>.GetErrors(errorIds, hostingEnvironment);
        }

        public Response<T> ValidateAgentInRequest(OfferResponse offerDocument, DistributionChainLinkType[] distributionChains, IHostEnvironment hostEnvironment)
        {
            var errorIds = ValidateOfferPrice.ValidateAgencyInRequest(offerDocument, distributionChains);

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public Response<T> ValidateAgentAndPax(OfferResponse offerDB, IATA_OfferPriceRQ offerPricerRequest, IHostEnvironment hostingEnvironment, ValidationRS validationRS, IGenericDataMapper genericDataMapper)
        {
            var errorIds = new List<ErrorDetail>();

            if (offerDB != null)
            {
                errorIds.AddRange(ValidateOfferPrice.ValidateAgentV2(offerDB, validationRS));
                errorIds.AddRange(ValidateOfferPrice.ValidatePax(offerDB, offerPricerRequest, genericDataMapper));
                errorIds.AddRange(ValidateOfferPrice.ValidateDuplicatePaxID(offerPricerRequest));
                errorIds.AddRange(ValidateOfferPrice.ValidateOfferItemRefId(offerDB, offerPricerRequest));
            }

            return ErrorHelper<T>.GetErrors(errorIds, hostingEnvironment);
        }

        public Response<T> ValidateDbOffer(OfferResponse airlineOffers, string offerId, IHostEnvironment hostingEnvironment)
        {
            var errorIds = ValidateOfferPrice.ValidateOfferFromDb(airlineOffers, offerId);

            return ErrorHelper<T>.GetErrors(errorIds, hostingEnvironment);
        }

        public Response<T> ValidateOfferPriceResponse(OfferResponse offer, IHostEnvironment hostingEnvironment)
        {
            var errorIds = ValidateOfferPrice.ValidateOfferPriceResponse(offer);

            return ErrorHelper<T>.GetErrors(errorIds, hostingEnvironment);
        }

        public Response<T> ValidateOfferPriceVaRequest(IATA_OfferPriceRQ offerPriceRQ, IHostEnvironment hostingEnvironment)
        {
            var pricedOffer = (SelectedOfferListType)offerPriceRQ?.Request?.PricedOffer?.Item;
            var errorIds = new List<ErrorDetail>();

            errorIds.AddRange(ValidateOfferPrice.ValidateAgentInRQ(offerPriceRQ.DistributionChain));

            if (errorIds.Count == 0)
            {
                errorIds.AddRange(ValidateOfferPrice.ValidateOfferId(pricedOffer?.SelectedOffer?.FirstOrDefault()?.SelectedOfferItem));
                errorIds.AddRange(ValidateOfferPrice.ValidatePaxList(offerPriceRQ?.Request?.DataLists.PaxList));
            }

            return ErrorHelper<T>.GetErrors(errorIds, hostingEnvironment);
        }

        public Response<T> ValidateUpsellConfigResponse(IEnumerable<UpsellOffersData> upsellConfigResponse, IHostEnvironment hostingEnvironment)
        {
            var errorIds = ValidateOfferPrice.ValidateUpsellConfigResponse(upsellConfigResponse);

            return ErrorHelper<T>.GetErrors(errorIds, hostingEnvironment);
        }

        public Response<T> CreateTechnicalErrorResponse(IHostEnvironment hostingEnvironment)
        {
            var errorIds = ValidateOfferPrice.CreateTechnicalErrorResponse();

            return ErrorHelper<T>.GetErrors(errorIds, hostingEnvironment);
        }
    }
}
