using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.OrderQuote;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderQuoteRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;

namespace VAA.EAPI.NDC.Offer.Validations.OrderQuote
{
    public static class MandatoryFieldsValidations
    {
        public static List<ErrorDetail> ValidateRequestFields(OrderRepriceQuery orderQuoteRQ)
        {
            var errorIds = new List<ErrorDetail>();
            string ordinal = orderQuoteRQ?.Parties?.Participants?.FirstOrDefault()?.Ordinal;
            bool checkOrdinal = Convert.ToBoolean(orderQuoteRQ?.Parties?.Participants?.FirstOrDefault(x => x.PartyType == VAA.EAPI.Domain.Virgin.Models.Common.Participant.PartyTypeEnum.Seller)?.Ordinal.StartsWith("1"));

            if (orderQuoteRQ?.PricedOffer.SelectedOffer.OfferRefID == null || orderQuoteRQ?.PricedOffer.SelectedOffer.OfferItems == null || string.IsNullOrEmpty(orderQuoteRQ?.OrderId) || string.IsNullOrEmpty(orderQuoteRQ.PricedOffer.SelectedOffer.OwnerCode) || string.IsNullOrEmpty(orderQuoteRQ.OwnerCode))
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "912_5",
                });
                return errorIds;
            }

            if (orderQuoteRQ.PricedOffer.SelectedOffer.OfferItems?.Any(selectedOfferItem => selectedOfferItem.OfferItemId == null || selectedOfferItem.ServiceBeneficiaries == null) ?? false)
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "912_5",
                });
                return errorIds;
            }

            if (string.IsNullOrEmpty(ordinal) || (ordinal.Equals("0")) || (ordinal.Any(Char.IsLetter)) ||
             (!checkOrdinal))
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "912_24",
                });
                return errorIds;

            }

            return errorIds;

        }

        //ValidateOrderQuoteRequest
        public static List<ErrorDetail> ValidateOrdinalinRequestFields(OrderRepriceQuery orderQuoteRQ)
        {
            var errorIds = new List<ErrorDetail>();
            string ordinal = orderQuoteRQ?.Parties?.Participants?.FirstOrDefault()?.Ordinal;
            bool checkOrdinal = Convert.ToBoolean(orderQuoteRQ?.Parties?.Participants?.FirstOrDefault(x => x.PartyType == VAA.EAPI.Domain.Virgin.Models.Common.Participant.PartyTypeEnum.Seller)?.Ordinal.StartsWith("1"));

            if (string.IsNullOrEmpty(ordinal) || (ordinal.Equals("0")) || (ordinal.Any(Char.IsLetter)) ||
             (!checkOrdinal))
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "912_24",
                });
                return errorIds;

            }

            return errorIds;
        }

        public static List<ErrorDetail> ValidateOrderIDinRQ(OrderRepriceQuery orderQuoteRQ)
        {
            var errorIds = new List<ErrorDetail>();

            if (orderQuoteRQ?.OwnerCode != "VS")
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "912_2",
                });
            }

            if (!RegexCompiled.IsMatch(orderQuoteRQ.OrderId, "^[A-Za-z0-9-|]*$"))
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "129_3",
                });
            }

            return errorIds;
        }

        public static List<ErrorDetail> ValidateOfferIDinRQ(OrderRepriceQuery orderQuoteRQ)
        {
            var errorIds = new List<ErrorDetail>();
            var offers = orderQuoteRQ?.PricedOffer?.SelectedOffer;
            if (string.IsNullOrEmpty(offers.OwnerCode))
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "912_2",
                });
            }

            if (string.IsNullOrEmpty(offers?.OfferRefID))
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "912_3",
                });
            }

            if (!RegexCompiled.IsMatch(offers.OfferRefID, "^[A-Za-z0-9-|]*$"))
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "913_1",
                });
            }

            if (offers.OfferItems != null)
            {
                if (offers.OfferItems.Any(i => string.IsNullOrEmpty(i.OfferItemId)))
                {
                    errorIds.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "912_3",
                    });
                }

                if (offers.OfferItems.Any(i => !RegexCompiled.IsMatch(i.OfferItemId, "^[A-Za-z0-9-|]*$")))
                {
                    errorIds.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "913_1",
                    });
                }

                if (offers.OfferItems.Any(item => item.ServiceBeneficiaries?.Any(pax => pax.IsNullOrWhiteSpace()) ?? false))
                {
                    errorIds.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "912_4",
                    });
                    return errorIds;
                }
            }
            return errorIds;
        }

        public static IEnumerable<ErrorDetail> ValidateOrderDetails(OrderDocument orderDocument)
        {
            var errorIds = new List<ErrorDetail>();

            // Check Order is not found in the Order store of NDC
            if (orderDocument == null)
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "129_1",
                });

            }
            else if (orderDocument.Warnings != null && orderDocument.Warnings.Any())
            {
                if (orderDocument.Warnings.Any(x => x.Code == "129"))
                {
                    errorIds.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "129_1",
                    });
                }
               
            }

            return errorIds;
        }

        public static List<ErrorDetail> ValidateOrderDB(OrderRepriceQuery orderRepriceRQ, OfferResponse offerDoc)
        {
            int validOfferItem = 0;
            var errorIds = new List<ErrorDetail>();

            if (offerDoc.OrderId != orderRepriceRQ.OrderId)
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "129_3",
                });

                return errorIds;
            }

            var offerItems = offerDoc?.Offers.Where(offer => offer.OfferId == orderRepriceRQ?.PricedOffer?.SelectedOffer?.OfferRefID)
                                                .SelectMany(x => x.OfferItems)
                                                .Select(x => x.OfferItemId);

            var repriceOfferItemIds = orderRepriceRQ?.PricedOffer?.SelectedOffer?.OfferItems.Select(x => x.OfferItemId);

            validOfferItem = offerItems.Join(repriceOfferItemIds,
                                            item => item,
                                            reprice => reprice,
                                            (item, reprice) => new { item, reprice }
                                            ).Count();

            if (validOfferItem != orderRepriceRQ?.PricedOffer?.SelectedOffer?.OfferItems.Count())
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "913_1",
                });

                return errorIds;
            }

            return errorIds;
        }

        public static List<ErrorDetail> ValidateOfferItemIDinDB(OrderRepriceQuery orderRepriceRQ, OfferResponse offerDoc)
        {
            var errorIds = new List<ErrorDetail>();
            var reshopDocOfferItems = offerDoc != null && offerDoc.Offers != null  ? offerDoc.Offers.FirstOrDefault(offer => offer.OfferId == orderRepriceRQ?.PricedOffer?.SelectedOffer?.OfferRefID && offer.OfferItems.Count() == orderRepriceRQ?.PricedOffer?.SelectedOffer?.OfferItems.Count())?.OfferItems : null;

            if (reshopDocOfferItems != null)
            {
                var orderQuoteRequestOfferItems = orderRepriceRQ?.PricedOffer?.SelectedOffer?.OfferItems.ToArray();

                foreach (var offerItem in orderQuoteRequestOfferItems)
                {
                    var matchingOfferItemInOfferDoc = reshopDocOfferItems.First(x => x.Services.First().ServiceBeneficiaries.Any(svc => offerItem.ServiceBeneficiaries.Contains(svc)));
                    if (offerItem.OfferItemId != matchingOfferItemInOfferDoc.OfferItemId)
                    {
                        errorIds.Add(new ErrorDetail
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "913_1",
                        });
                        return errorIds;
                    }
                }
            }
            else
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "913_1",
                });
            }

            return errorIds;
        }

        public static List<ErrorDetail> ValidatePaxList(OrderRepriceQuery orderRepriceRQ, OfferResponse offerDoc)
        {

            var errorIds = new List<ErrorDetail>();
            var validPax = true;

            if (offerDoc != null)
            {

                var dataFromDB = offerDoc.DataList.CustomerDetails.Select(x => x.CustId);
                var dataFromRQPaxList = orderRepriceRQ.CustomerDetails.Where(x => x.Ptc is not null).Select(x => x.CustId);
                var dataFromRQSelectedOffer = orderRepriceRQ.PricedOffer.SelectedOffer.OfferItems.SelectMany(x => x.ServiceBeneficiaries).Select(x => x);

                if (dataFromDB == null || dataFromRQPaxList == null || dataFromRQSelectedOffer == null)
                {
                    validPax = false;
                }

                if (dataFromDB.Count() == dataFromRQPaxList.Count() && dataFromDB.Count() == dataFromRQSelectedOffer.Count() && dataFromDB.All(dataFromRQPaxList.Contains) && dataFromDB.All(dataFromRQSelectedOffer.Contains))
                {
                    foreach (var fromDB in offerDoc.DataList.CustomerDetails)
                    {
                        foreach (var fromRQ in (orderRepriceRQ.CustomerDetails.Where(x => x.CustId == fromDB.CustId)))
                        {
                            fromRQ.Ptc = fromDB.Ptc;
                        }
                    }
                }
                else
                {
                    validPax = false;
                }

                if (!validPax)
                {
                    errorIds.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "912_4",
                    });
                }
            }

            return errorIds;
        }

        public static List<ErrorDetail> ValidateOrderDBServiceBeneficiaries(OrderRepriceQuery orderRepriceRQ, OfferResponse offerDoc)
        {
            var errorIds = new List<ErrorDetail>();
            var selectedOffer = offerDoc.Offers.FirstOrDefault(x => x.OfferId == orderRepriceRQ.PricedOffer.SelectedOffer?.OfferRefID);

            if (selectedOffer == null)
            {
                return errorIds;
            }

            var validPax = true;

            var selectedOfferItems = orderRepriceRQ.PricedOffer.SelectedOffer?.OfferItems;

            if (selectedOfferItems == null)
            {
                return errorIds;
            }

            foreach (var offerItem in selectedOffer.OfferItems)
            {
                var correspondingOfferItem = selectedOfferItems.FirstOrDefault(oI => oI.OfferItemId == offerItem.OfferItemId);

                if (correspondingOfferItem == null || correspondingOfferItem.ServiceBeneficiaries == null)
                {
                    validPax = false;
                    break;
                }

                var serviceBeneficiaries = offerItem.Services.FirstOrDefault()?.ServiceBeneficiaries;

                if (serviceBeneficiaries == null || serviceBeneficiaries.Count() != correspondingOfferItem.ServiceBeneficiaries.Count())
                {
                    validPax = false;
                    break;
                }

                if (!correspondingOfferItem.ServiceBeneficiaries.All(sb => serviceBeneficiaries.Contains(sb)))
                {
                    validPax = false;
                    break;
                }
            }

            if (!validPax)
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "912_4",
                });
            }

            return errorIds;
        }

        public static IEnumerable<ErrorDetail> ValidateOrderQuoteRequest(DistributionChainLinkType[] distributionChain)
        {
            var errors = new List<ErrorDetail>();
            string totalDC = distributionChain.Length.ToString();

            if (!distributionChain.Any(x => x.OrgRole == DistributionChainRoleCodeType.Carrier))
            {
                errors.Add(new ErrorDetail
                {
                    ErrorCode = "914_12",
                    IsDynamicMessage = false
                });
            }

            if (errors.Count == 0)
            {
                foreach (var dC in distributionChain)
                {
                    if (dC.ParticipatingOrg == null || string.IsNullOrEmpty(dC.ParticipatingOrg.OrgID))
                    {
                        errors.Add(new ErrorDetail
                        {
                            ErrorCode = "912_1",
                            IsDynamicMessage = false
                        });
                    }
                    else if (dC.OrgRole == DistributionChainRoleCodeType.Carrier && dC.Ordinal != totalDC)
                    {
                        errors.Add(new ErrorDetail
                        {
                            ErrorCode = "912_24",
                            IsDynamicMessage = false
                        });
                    }
                    else if (dC.OrgRole == DistributionChainRoleCodeType.Carrier && dC.ParticipatingOrg.OrgID != Constants.Application.OwnerCode)
                    {
                        errors.Add(new ErrorDetail
                        {
                            ErrorCode = "914_13",
                            IsDynamicMessage = false
                        });
                    }
                    else if ((dC.SalesAgent != null && string.IsNullOrEmpty(dC.SalesAgent.SalesAgentID)) || (dC.SalesBranch != null && string.IsNullOrEmpty(dC.SalesBranch.SalesBranchID)))
                    {
                        errors.Add(new ErrorDetail
                        {
                            ErrorCode = "912_1",
                            IsDynamicMessage = false
                        });
                    }
                    else if (string.IsNullOrEmpty(dC.Ordinal) || (dC.Ordinal.Equals("0")) || (dC.Ordinal.Any(Char.IsLetter)) || (dC.OrgRole == DistributionChainRoleCodeType.Seller && !dC.Ordinal.StartsWith("1")))
                    {
                        errors.Add(new ErrorDetail
                        {
                            ErrorCode = "912_24",
                            IsDynamicMessage = false
                        });
                    }

                }
            }
            return errors;
        }
    }
}
