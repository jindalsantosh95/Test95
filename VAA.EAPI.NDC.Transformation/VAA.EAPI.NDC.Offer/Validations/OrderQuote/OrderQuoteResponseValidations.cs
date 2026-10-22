using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;

namespace VAA.EAPI.NDC.Offer.Validations.OrderQuote
{
    public class OrderQuoteResponseValidations
    {
        public static Response<IATA_OrderReshopRS> ValidateOrderQuoteResponse(OfferResponse offerResponse, bool allUpsellOffers, IHostEnvironment hostingEnvironment)
        {

            var errorIds = VaidateMandatoryFields(offerResponse, allUpsellOffers, hostingEnvironment);

            return ErrorHelper<IATA_OrderReshopRS>.GetErrors(errorIds, hostingEnvironment);
        }

        private static List<ErrorDetail> VaidateMandatoryFields(OfferResponse offerResponse, bool allUpsellOffers, IHostEnvironment hostingEnvironment)
        {
            var errorIds = new List<ErrorDetail>();

            if (offerResponse.Error != null && offerResponse.Error.Any(err => err.Code.ToUpper() == Constants.Application.EAPI_INVALID_ERROR_REPRICE_RESPONSE_SPECIFIC))
            {
                errorIds.Add(new ErrorDetail()
                {
                    IsDynamicMessage = false,
                    ErrorCode = "912_20",
                });
            }

            if (offerResponse.Error != null && offerResponse.Error.Any(err => err.Code.ToUpper() == Constants.Application.EAPI_INVALID_ERROR_RESPONSE_GENERAL))
            {
                errorIds.Add(new ErrorDetail()
                {
                    IsDynamicMessage = false,
                    ErrorCode = "911_22",
                });
            }

            if (offerResponse.Error.Any(err => err.Code.ToUpper() == "10124"))
            {
                errorIds.Add(new ErrorDetail()
                {
                    IsDynamicMessage = false,
                    ErrorCode = "913_2",
                });
            }

            if (offerResponse.Error.Any(err => err.Code.ToUpper() == "10118"))
            {
                errorIds.Add(new ErrorDetail()
                {
                    IsDynamicMessage = false,
                    ErrorCode = "ORDERQUOTE_OfferRefID_InValid",
                });
            }

            if (allUpsellOffers == true)
            {
                errorIds.Add(new ErrorDetail()
                {
                    IsDynamicMessage = false,
                    ErrorCode = "913_2",
                });
            }
            return errorIds;
        }

        public static IEnumerable<NDCWarning> ValidateQuoteRefundOffer(OfferResponse offerResponse, IHostEnvironment hostEnvironment)
        {
            var warnings = new List<NDCWarning>();

            var totalAddCollectAmount = 0m;
            var totalRefundAmount = 0m;

            var upsellOffers = offerResponse?.Offers?.Where(o => o.UpsellOffer).ToList();

            var selectedOffer = offerResponse?.Offers?.FirstOrDefault(o => o.Selected);

            bool hasRefundInUpsell = upsellOffers?.FirstOrDefault()?.OfferItems.Any(oi => oi.OfferitemPricing.Any(p => p.RefundPrice != null)) == true;

            var offersToProcess = new List<OfferDetails>();

            if (hasRefundInUpsell)
            {
                offersToProcess.AddRange(upsellOffers);
                if (selectedOffer != null)
                    offersToProcess.Add(selectedOffer);
            }
            else if (selectedOffer != null)
            {
                offersToProcess.Add(selectedOffer);
            }

            string currencyCode = null; 
            foreach (var offer in offersToProcess)
            {
                foreach (var offerItem in offer.OfferItems)
                {
                    var paxCount = offerItem.Services.First().ServiceBeneficiaries.Count();
                    var addCollectPrice = offerItem.OfferitemPricing.Select(p => p.AddCollectPrice).FirstOrDefault();
                    var refundPrice = offerItem.OfferitemPricing.Select(p => p.RefundPrice).FirstOrDefault();

                    totalAddCollectAmount += Convert.ToDecimal(addCollectPrice?.Amount?.MonetaryAmount) * paxCount;
                    totalRefundAmount += Convert.ToDecimal(refundPrice?.FirstOrDefault()?.Amount?.MonetaryAmount) * paxCount;
                    currencyCode = refundPrice?.FirstOrDefault()?.Amount.BaseCurrency;
                }
            }
            if ((totalAddCollectAmount - totalRefundAmount) < 0 || (totalAddCollectAmount == 0 && totalRefundAmount < 0))
            {
                var RefundAmountDislay = Math.Abs((totalAddCollectAmount == 0) ? totalRefundAmount : (totalAddCollectAmount - totalRefundAmount)); 
                var DocUrl = new Link()
                {
                     Href = "https://help.virginatlantic.com/gb/en/contact-forms/refund-form.html?webform=ticket-refunds",
                };
                warnings.Add(new NDCWarning
                {
                    IsDynamicMessage = true,
                    Code = "745_1",
                    DocURL = DocUrl,
                    DynamicMessagesContents = new List<DynamicMessagesContent>
                    {
                        new DynamicMessagesContent { Fieldname = "{XX.XX}", ReplaceWith = RefundAmountDislay.ToString("F2") },
                        new DynamicMessagesContent { Fieldname = "{CurrencyCode}", ReplaceWith = currencyCode }
                    }
                });
            }

            return ErrorHelper<NDCWarning>.GetWarnings(warnings, hostEnvironment);      
         }


    }
}
