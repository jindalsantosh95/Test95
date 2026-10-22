using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OfferPriceRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.UpsellOffers;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;

namespace VAA.EAPI.NDC.Offer.Validations.OfferPrice
{
    public class ValidateOfferPrice
    {
        private static string[] allowedPaxTypes = new string[] { "ADT", "CNN", "CHD", "GBE", "INF" };

        private static readonly Dictionary<string, string> errorMap = new Dictionary<string, string>()
        {
            { "UNAUTHORIZED", Constants.Application.NDCTECHNICALERROR },
            { "INTERNALSERVERERROR", Constants.Application.NDCTECHNICALERROR },
            { "10118", Constants.Application.NDCOFFEREXPIREDERROR }
        };

        public static string MapExternalErrorToNDCError(string externalError)
        {
            string ndcError = errorMap.ContainsKey(externalError) ? errorMap[externalError] : Constants.Application.NDCTECHNICALERROR;

            return ndcError;
        }

        public static List<ErrorDetail> ValidateOfferId(IEnumerable<SelectedOfferItemType> offerIdSummary)
        {
            var errorIds = new List<ErrorDetail>();

            if (offerIdSummary == null || string.IsNullOrWhiteSpace(offerIdSummary.First().OfferItemRefID))
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "OFFERPRICE_OfferRefID_InValid",
                });
            }

            return errorIds;
        }

        public static List<ErrorDetail> ValidatePaxList(PaxType[] paxList)
        {
            var errors = new List<ErrorDetail>();
            if (paxList == null || paxList.Length == 0)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "143_1",
                });
            }
            else
            {
                var countAdt = paxList.Where(x => x.PTC == Constants.Application.AdultPTCCode).Count();
                var countChd = paxList.Where(x => x.PTC == Constants.Application.ChildPTCCode).Count();
                var countYoung = paxList.Where(x => x.PTC == Constants.Application.YoungAdultPTCCode).Count();
                var countInf = paxList.Where(x => x.PTC == Constants.Application.InfantPTCCode).Count();
                var countDistinctPaxId = paxList.Select(x => x.PaxID).Distinct().Count();

                if (countAdt < countInf)
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "324_1",
                    });
                }

                if ((countAdt + countChd + countYoung) > 9)
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "68_1",
                    });
                }

                if (((countAdt + countChd + countInf + countYoung) != paxList.Length) || countAdt == 0 || countDistinctPaxId < paxList.Length)
                {

                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "143_2",
                    });
                }
            }

            return errors;
        }

        public static List<ErrorDetail> ValidateAgencyInRequest(OfferResponse offerDocument, DistributionChainLinkType[] distributionChain)
        {
            var errors = new List<ErrorDetail>();
            var dc = distributionChain.Where(x => x.OrgRole == DistributionChainRoleCodeType.Seller);

            if (offerDocument.Parties.Participants.First(x => x.PartyType == Participant.PartyTypeEnum.Seller).AgencyID != dc.First().ParticipatingOrg.OrgID)
            {
                errors.Add(new ErrorDetail()
                {
                    ErrorCode = "368_1",
                    IsDynamicMessage = false
                });
            }

            return errors;
        }

        public static List<ErrorDetail> ValidatePax(OfferResponse offerDB, IATA_OfferPriceRQ offerPriceRequest, IGenericDataMapper genericDataMapper)
        {
            var errors = new List<ErrorDetail>();

            var paxListRQ = offerPriceRequest?.Request?.DataLists?.PaxList
                                              .Select(pax => new PaxType()
                                              {
                                                  PaxID = pax.PaxID,
                                                  PTC = genericDataMapper.ConvertNdcPtcToItaPtc(pax.PTC)
                                              })
                                              .DistinctBy(x => x.PTC);

            var paxListDB = offerDB?.DataList?.CustomerDetails
                                              .Select(customer => new PaxType()
                                              {
                                                  PaxID = customer.CustId,
                                                  PTC = customer.Ptc
                                              })
                                              .DistinctBy(x => x.PTC);

            if (paxListDB.Count() != paxListRQ.Count())
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "911_3",
                });

                return errors;
            }

            if (!paxListRQ.All(r => paxListDB.Any(db => db.PTC.Is(r.PTC))))
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "911_3",
                });

                return errors;
            }

            if (offerPriceRequest?.Request?.DataLists?.PaxList.Any(x => !allowedPaxTypes.Contains(x.PTC)) ?? false)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "911_3",
                });

                return errors;
            }

            return errors;
        }

        public static List<ErrorDetail> ValidateDuplicatePaxID(IATA_OfferPriceRQ offerPriceRequest)
        {
            return offerPriceRequest.Request
                                    .DataLists
                                    .PaxList
                                    .GroupBy(y => y.PaxID)
                                    .Where(g => g.Count() > 1)
                                    .SelectMany(g => g)
                                    .Select(x => new ErrorDetail
                                    {
                                        IsDynamicMessage = true,
                                        ErrorCode = "143_3",
                                        DynamicMessagesContents = new List<DynamicMessagesContent>()
                                        {
                                            new DynamicMessagesContent()
                                            {
                                                Fieldname ="{PaxID}",
                                                ReplaceWith = x.PaxID
                                            }
                                        }
                                    })
                                    .ToList();
        }

        public static List<ErrorDetail> ValidateAgentV2(OfferResponse offerDoc, ValidationRS validationRS)
        {
            bool isValid = true;
            var errors = new List<ErrorDetail>();
            var agencyListOfferDoc = offerDoc.Parties.Participants.Where(x => x.PartyType != Participant.PartyTypeEnum.Carrier);
            if (validationRS.ValidateResponse.KeyType == Constants.Application.KeyTypeDirect)
            {
                if (!offerDoc.Parties.Participants.Any(x=>x.AgencyID == validationRS.ValidateResponse.AgencyDetails.RequestAgencyId))
                    isValid = false;
            }
            else
            {
                if (agencyListOfferDoc.Count() > 3)
                    isValid = false;
                else
                {
                    foreach (var party in agencyListOfferDoc)
                    {
                        if (party?.PartyType == EAPI.Domain.Virgin.Models.Common.Participant.PartyTypeEnum.Seller)
                        {
                            if (party.AgencyID != validationRS.ValidateResponse.AgencyDetails.RequestAgencyId)
                                isValid = false;
                        }
                        else
                        {
                            if (party.AgencyID != validationRS.ValidateResponse.AggregatorDetails.AgencyId)
                                isValid = false;
                        }
                    }
                }
            }
            if (!isValid)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "368_1",
                });
            }
            return errors;
        }

        public static List<ErrorDetail> ValidateOfferFromDb(OfferResponse airlineOffers, string offerId)
        {
            var errorIds = new List<ErrorDetail>();

            if (airlineOffers?.Error != null && airlineOffers.Error.Any(error => MapExternalErrorToNDCError(error.Code.ToUpper()) == Constants.Application.NDCTECHNICALERROR))
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "Service_TechError",
                });
            }
            else if (airlineOffers?.Offers == null)
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "OFFERPRICE_OfferRefID_InValid",
                });
            }
            else if (!airlineOffers.Offers.Any(x => x.OfferId == offerId))
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "OFFERPRICE_OfferRefID_InValid",
                });
            }

            return errorIds;
        }

        public static List<ErrorDetail> ValidateOfferPriceResponse(OfferResponse offer)
        {
            var errorIds = new List<ErrorDetail>();

            if (offer?.Error != null)
            {
                if (offer.Error.Any(error => MapExternalErrorToNDCError(error.Code).Is(Constants.Application.NDCTECHNICALERROR)))
                {
                    errorIds.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "Service_TechError"
                    });
                }
                if (offer.Error.Any(error => MapExternalErrorToNDCError(error.Code).Is(Constants.Application.NDCOFFEREXPIREDERROR)))
                {
                    errorIds.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = Constants.Application.NDCOFFEREXPIREDERROR
                    });
                }
            }
            else if (offer?.Offers != null && !offer.Offers.Any(x => x.Selected))
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "913_2"
                });
            }

            return errorIds;
        }

        public static List<ErrorDetail> CreateTechnicalErrorResponse()
        {
            var errorIds = new List<ErrorDetail>();

            errorIds.Add(new ErrorDetail
            {
                IsDynamicMessage = false,
                ErrorCode = "Service_TechError"
            });

            return errorIds;
        }

        public static List<ErrorDetail> ValidateAgentInRQ(DistributionChainLinkType[] distributionChain)
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

        public static List<ErrorDetail> ValidateUpsellConfigResponse(IEnumerable<UpsellOffersData> upsellConfigResponse)
        {
            var errorIds = new List<ErrorDetail>();

            if (upsellConfigResponse.Any(x => x.Error != null))
            {
                errorIds.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "Service_TechError",
                });
            }

            return errorIds;

        }

        public static IEnumerable<NDCWarning> ValidateFormOfPaymentType(ValidationRS validationMSResponse, bool isCreditCardAccepted, IHostEnvironment hostEnvironment)
        {
            var warningIds = ResponseValidations.ValidateFormOfPaymentType(validationMSResponse,isCreditCardAccepted).ToList();

            return ErrorHelper<NDCWarning>.GetWarnings(warningIds, hostEnvironment);
        }

        public static List<ErrorDetail> ValidateOfferItemRefId(OfferResponse offerDB, IATA_OfferPriceRQ offerPriceRequest)
        {
            var errorIds = new List<ErrorDetail>();

            var selectedOfferList = (SelectedOfferListType)offerPriceRequest?.Request?.PricedOffer?.Item;
            var selectedOffers = selectedOfferList?.SelectedOffer;

            if (selectedOffers == null || offerDB?.Offers == null)
            {
                return errorIds;
            }

            foreach (var selectedOffer in selectedOffers)
            {
                if (selectedOffer?.SelectedOfferItem == null)
                {
                    errorIds.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "913_3"
                    });
                    return errorIds;
                }

                var offerItemsInDb = offerDB.Offers.FirstOrDefault(offer =>
                    offer.OfferId == selectedOffer.OfferRefID &&
                    (offer.OfferItems?.Count() ?? 0) == selectedOffer.SelectedOfferItem.Count())?.OfferItems;

                if (offerItemsInDb == null)
                {
                    errorIds.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "913_3"
                    });
                    return errorIds;
                }

                foreach (var selectedOfferItem in selectedOffer.SelectedOfferItem)
                {
                    if (selectedOfferItem == null
                        || string.IsNullOrWhiteSpace(selectedOfferItem.OfferItemRefID)
                        || selectedOfferItem.PaxRefID == null
                        || !selectedOfferItem.PaxRefID.Any())
                    {
                        errorIds.Add(new ErrorDetail
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "913_3"
                        });
                        return errorIds;
                    }
               
                    var matchingOfferItemInDb = offerItemsInDb.FirstOrDefault(x => string.Equals(x?.OfferItemId, selectedOfferItem.OfferItemRefID)
                        &&((x.ServiceBeneficiaries?.Any(b => selectedOfferItem.PaxRefID.Contains(b)) == true) || (x.Services?.Any(s => s?.ServiceBeneficiaries?.Any(b =>selectedOfferItem.PaxRefID.Contains(b)) == true) == true)));

                    if (matchingOfferItemInDb == null || selectedOfferItem.OfferItemRefID != matchingOfferItemInDb.OfferItemId)
                    {
                        errorIds.Add(new ErrorDetail
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "913_3"
                        });
                        return errorIds;
                    }
                }
            }

            return errorIds;
        }
    }
}
