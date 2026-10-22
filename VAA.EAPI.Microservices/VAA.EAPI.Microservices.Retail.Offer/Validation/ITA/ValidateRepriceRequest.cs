using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.Reprice;
using VAA.EAPI.Microservices.Retail.Offer.Constants;
using static VAA.EAPI.Domain.Virgin.Models.Common.Product;

namespace VAA.EAPI.Microservices.Retail.Offer.Validation.Ita
{
    public class ValidateRepriceRequest
    {
        public static Result<Response<string>> OrderRepriceRequestValidator(OrderRepriceQuery requestData)
        {
            if (requestData == null) throw new ArgumentNullException(nameof(requestData));

            var errorList = new List<ErrorDetails>();
            var response = new Response<string>
            {
                Status = Status.OK,
                Errors = null
            };

            if (requestData.PricedOffer == null || requestData.PricedOffer.SelectedOffer == null)
            {
                response = GetErrorResponse(errorList, ErrorCodes.ShoppingRefIDMissing);
                response = GetErrorResponse(errorList, ErrorCodes.OfferRefIDMissing);
                response = GetErrorResponse(errorList, ErrorCodes.OfferItemIdMissing);
                response = GetErrorResponse(errorList, ErrorCodes.ProductTypeMissing);
                response = GetErrorResponse(errorList, ErrorCodes.ServiceBeneficiariesMissing);
            }
            else
            {
                if (string.IsNullOrEmpty(requestData.PricedOffer.SelectedOffer.ShoppingRefID))
                    response = GetErrorResponse(errorList, ErrorCodes.ShoppingRefIDMissing);

                if (string.IsNullOrEmpty(requestData.PricedOffer.SelectedOffer.OfferRefID))
                    response = GetErrorResponse(errorList, ErrorCodes.OfferRefIDMissing);

                if (requestData.PricedOffer.SelectedOffer.OfferItems == null || !requestData.PricedOffer.SelectedOffer.OfferItems.Any())
                {
                    response = GetErrorResponse(errorList, ErrorCodes.OfferItemIdMissing);
                    response = GetErrorResponse(errorList, ErrorCodes.ProductTypeMissing);
                    response = GetErrorResponse(errorList, ErrorCodes.ServiceBeneficiariesMissing);
                }
                else
                {
                    int serviceBeneficiaryCount = 0;

                    requestData.PricedOffer.SelectedOffer.OfferItems.ForEach(offerItem =>
                    {
                        if (string.IsNullOrEmpty(offerItem.OfferItemId))
                            response = GetErrorResponse(errorList, ErrorCodes.OfferItemIdMissing);
                        if (offerItem.Product == null || offerItem.Product.ProductType == null || !Enum.IsDefined(typeof(ProductTypes), offerItem.Product.ProductType))
                            response = GetErrorResponse(errorList, ErrorCodes.ProductTypeMissing);
                        if (offerItem.ServiceBeneficiaries == null || !offerItem.ServiceBeneficiaries.Any())
                            response = GetErrorResponse(errorList, ErrorCodes.ServiceBeneficiariesMissing);
                        else
                        {
                            bool isServiceBeneficiariesPresnt;

                            offerItem.ServiceBeneficiaries.ForEach(serviceBeneficiaries =>
                            {
                                isServiceBeneficiariesPresnt = false;
                                serviceBeneficiaryCount++;

                                if (string.IsNullOrEmpty(serviceBeneficiaries))
                                    response = GetErrorResponse(errorList, ErrorCodes.ServiceBeneficiariesMissing);
                                else
                                {
                                    if (requestData.CustomerDetails != null && (requestData.CustomerDetails != null || requestData.CustomerDetails.Any()))
                                    {
                                        requestData.CustomerDetails.ForEach(custDtl =>
                                        {
                                            if (custDtl.CustId == serviceBeneficiaries)
                                                isServiceBeneficiariesPresnt = true;
                                        });
                                    }
                                }

                                if (!isServiceBeneficiariesPresnt)
                                    response = GetErrorResponse(errorList, ErrorCodes.DataMisMatch);
                            });

                        }
                    });

                    if (requestData.CustomerDetails.Any() && serviceBeneficiaryCount != requestData.CustomerDetails.Count())
                        response = GetErrorResponse(errorList, ErrorCodes.DataMisMatch);
                }

            }

            if (requestData.CustomerDetails == null || !requestData.CustomerDetails.Any())
            {
                response = GetErrorResponse(errorList, ErrorCodes.CustIdMissing);
                response = GetErrorResponse(errorList, ErrorCodes.PtcMissingorInvalid);
            }
            else
            {
                requestData.CustomerDetails.ForEach(custDtls =>
                {
                    if (string.IsNullOrEmpty(custDtls.CustId))
                        response = GetErrorResponse(errorList, ErrorCodes.CustIdMissing);

                    if (string.IsNullOrEmpty(custDtls.Ptc) || Constants.Application.Ptc.All(a => a != custDtls.Ptc.ToUpper()))
                        response = GetErrorResponse(errorList, ErrorCodes.PtcMissingorInvalid);
                });
            }

            return response;
        }

        private static Response<string> GetErrorResponse(List<ErrorDetails> errorList, string errorCode)
        {
            errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());

            return new Response<string>
            {
                Status = Status.BusinessException,
                Errors = errorList
            };
        }
    }
}
