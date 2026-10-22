using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Products.Baggage;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OfferPriceRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Helper;
using VAA.EAPI.NDC.Offer.Infrastructure.Mapper;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.BspMarket;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using static VAA.EAPI.Domain.Virgin.Models.Common.Price;

using LengthUnitCodeType = VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OfferPriceRS.LengthUnitCodeType;
using WeightUnitCodeType = VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OfferPriceRS.WeightUnitCodeType;

namespace VAA.EAPI.NDC.Offer.Mapper
{
    public class OfferPriceRSMapper
    {
        public static IATA_OfferPriceRS MapOfferPriceResponse(OfferResponse offerResponse, IGenericDataMapper genericDataMapper, bool isNonIataAgent, BspMarketPaymentData bspMarketPaymentData, IEnumerable<NDCWarning> nDCWarnings, bool? itaAccountCode, bool isCashPaymentAllowed, bool isEasyFopAllowed, bool isCardPaymentAllowed)
        {
            var paxSegmentList = new List<PaxSegmentType>();
            var paxJourneyList = new List<PaxJourneyType>();
            var priceClassList = new List<PriceClassType>();
            var datedMarketingSegmentList = new List<DatedMarketingSegmentType>();
            var datedOperatingSegmentList = new List<DatedOperatingSegmentType>();
            var datedOperatingLegList = new List<DatedOperatingLegType>();
            var baggageAllowanceList = new List<BaggageAllowanceType>();
            var serviceDefinitionList = new List<ServiceDefinitionType>();
            var originDestList = GetOriginDestinations(offerResponse);

            int serviceIdCountLocal = 0; 

            var paymentFunctions = new List<PaymentFunctionsOfferType>();

            if (offerResponse?.FareTypeRequest?.FareTypeCode == Constants.Application.PrivateFareTypeCode)
            {
                if (isCashPaymentAllowed)
                    paymentFunctions.AddRange(GetPaymentFOP());

                if (isEasyFopAllowed)
                    paymentFunctions.AddRange(GetEasyFOPPayment());
            }
            else
            {
                if (isCashPaymentAllowed)
                    paymentFunctions.AddRange(GetPaymentFOP());

                if (isEasyFopAllowed)
                    paymentFunctions.AddRange(GetEasyFOPPayment());

                if (bspMarketPaymentData.IsCreditCardAccepted)
                    paymentFunctions.AddRange(GetPaymentFunctions(bspMarketPaymentData));
            }

            PaymentFunctionsOfferType[] result = paymentFunctions.Any() ? paymentFunctions.ToArray() : null;

            return new()
            {
                Items = new OfferPriceResponseType[1]
                    {
                        new OfferPriceResponseType()
                        {
                             PricedOffer = GetPricedOffer(offerResponse, genericDataMapper,  originDestList,  paxSegmentList, baggageAllowanceList, priceClassList,paxJourneyList,datedMarketingSegmentList,datedOperatingSegmentList,datedOperatingLegList,ref serviceIdCountLocal, serviceDefinitionList,itaAccountCode),
                             OtherOffers = GetUpsellOffers(offerResponse, genericDataMapper,  originDestList, paxSegmentList, baggageAllowanceList, priceClassList,paxJourneyList,datedMarketingSegmentList,datedOperatingSegmentList,datedOperatingLegList,ref serviceIdCountLocal, serviceDefinitionList,itaAccountCode),
                             DataLists=new DataListsType()
                             {
                                BaggageAllowanceList = baggageAllowanceList.ToArray(),
                                PaxList = GetPaxDetails(offerResponse.DataList.CustomerDetails),
                                PaxSegmentList = paxSegmentList.ToArray(),
                                PaxJourneyList = paxJourneyList.ToArray(),
                                PriceClassList = priceClassList.ToArray(),
                                ServiceDefinitionList = serviceDefinitionList.Any() ? serviceDefinitionList.ToArray() : null,
                                OriginDestList = originDestList.ToArray(),
                                DatedMarketingSegmentList = datedMarketingSegmentList.ToArray(),
                                DatedOperatingSegmentList = datedOperatingSegmentList.ToArray(),
                                DatedOperatingLegList = datedOperatingLegList.ToArray()
                             },
                            PaymentFunctions = result,
                            Warning = nDCWarnings.Any() ? GetWarnings(nDCWarnings) : null
                        }
                    },
            };
        }

        private static PaymentFunctionsOfferType[] GetPaymentFOP()
        {
            return new PaymentFunctionsOfferType[]
            {
                new PaymentFunctionsOfferType
                {
                    PaymentSupportedMethod = new PaymentSupportedMethodType
                    {
                        PaymentTypeCode = "CA"
                    }
                }
            };
        }

        private static PaymentFunctionsOfferType[] GetEasyFOPPayment()
        {
            return new PaymentFunctionsOfferType[]
            {
                new PaymentFunctionsOfferType
                {
                    PaymentSupportedMethod = new PaymentSupportedMethodType
                    {
                        PaymentTypeCode = "AC",
                        PaymentBrandCode = "EP"
                    }
                }
            };
        }

        private static PaymentFunctionsOfferType[] GetPaymentFunctions(BspMarketPaymentData bspMarketPaymentData)
        {
            var acceptedCardDetails = GetAcceptedPaymentCardDetails(bspMarketPaymentData);

            return acceptedCardDetails.Select(card => new PaymentFunctionsOfferType
            {
                PaymentSupportedMethod = new PaymentSupportedMethodType
                {
                    PaymentBrandCode = card,
                    PaymentTypeCode = "CC"
                }
            }).ToArray();
        }

        private static List<string> GetAcceptedPaymentCardDetails(BspMarketPaymentData bspMarketPaymentData)
        {
            List<string> acceptedCreditCards = new();
            if (bspMarketPaymentData.Amex == true)
            {
                acceptedCreditCards.Add("AX");
            }
            if (bspMarketPaymentData.Diners == true)
            {
                acceptedCreditCards.Add("DC");
            }
            if (bspMarketPaymentData.Discover == true)
            {
                acceptedCreditCards.Add("DS");
            }
            if (bspMarketPaymentData.Mastercard == true)
            {
                acceptedCreditCards.Add("CA");
            }
            if (bspMarketPaymentData.Maestro == true)
            {
                acceptedCreditCards.Add("TO");
            }
            if (bspMarketPaymentData.UATP == true)
            {
                acceptedCreditCards.Add("TP");
            }
            if (bspMarketPaymentData.Visa == true)
            {
                acceptedCreditCards.Add("VI");
            }
            return acceptedCreditCards;
        }

        private static WarningType[] GetWarnings(IEnumerable<NDCWarning> nDCWarnings)
        {
            return nDCWarnings.Select(x => new WarningType()
            {
                Code = Convert.ToString(x.Code),
                DescText = x.ShortText,
                LangCode = Constants.Application.LangCode,
                TypeCode = x.Status,
                TagText = x.TagText
            }).ToArray();
        }

        private static List<OriginDestType> GetOriginDestinations(OfferResponse offerResponse)
        {
            List<OriginDestType> originDestList = new();
            foreach (var onD in offerResponse.DataList.OriginDestinationAssociations)
            {
                originDestList.Add(new OriginDestType
                {
                    DestCode = onD.OriginDestination.DestinationAirport,
                    OriginCode = onD.OriginDestination.OriginAirport,
                    OriginDestID = onD.OdLabel,
                    PaxJourneyRefID = []
                });
            }
            return originDestList;
        }

        private static NewOfferType GetPricedOffer(OfferResponse offerResponse, IGenericDataMapper genericDataMapper, List<OriginDestType> originDestList, List<PaxSegmentType> paxSegmentList, List<BaggageAllowanceType> baggageAllowanceList, List<PriceClassType> priceClassList, List<PaxJourneyType> paxJourneyList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, ref int serviceIdCount, List<ServiceDefinitionType> serviceDefinitionList, bool? itaAccountCode)
        {
            var pricedOffer = offerResponse.Offers.FirstOrDefault(x => !x.UpsellOffer);

            if (pricedOffer != null)
            {
                int serviceIdCountLocal = serviceIdCount;
                var totalAmount = pricedOffer.SalePrice?.First(s => s.PriceComponentType == PriceComponentTypes.BasePriceEnum).Amount;

                NewOfferType newOfferType = new()
                {
                    OfferID = pricedOffer.OfferId,
                    OwnerCode = Constants.Application.OWNERCODE,
                    OfferItem = GetOfferItems(pricedOffer, genericDataMapper, offerResponse, paxSegmentList, priceClassList, originDestList, paxJourneyList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, ref serviceIdCountLocal, serviceDefinitionList, itaAccountCode,offerResponse?.FareTypeRequest?.FareTypeCode,offerResponse?.FareTypeRequest?.FareSubTypeCode),
                    JourneyOverview = GetJourneyOverview(pricedOffer.OfferItems.FirstOrDefault(), offerResponse.DataList, genericDataMapper, originDestList, paxSegmentList, paxJourneyList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, priceClassList),
                    BaggageAssociations = GetBaggageAssociations(pricedOffer, offerResponse.DataList, genericDataMapper, originDestList, paxSegmentList, baggageAllowanceList, paxJourneyList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList),
                    OfferExpirationTimeLimitDateTime = Convert.ToDateTime(Convert.ToDateTime(pricedOffer.OfferExpirationTimeLimit).ToString("yyy-MM-ddTHH:mm:ss")).ToUniversalTime(),
                    OfferExpirationTimeLimitDateTimeSpecified = true
                };

                serviceIdCount = serviceIdCountLocal;
                return newOfferType;
            }

            return null;
        }

        private static OtherOffersType GetUpsellOffers(OfferResponse offerResponse, IGenericDataMapper genericDataMapper, List<OriginDestType> originDestList, List<PaxSegmentType> paxSegmentList, List<BaggageAllowanceType> baggageAllowanceList, List<PriceClassType> priceClassList, List<PaxJourneyType> paxJourneyList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, ref int serviceIdCount, List<ServiceDefinitionType> serviceDefinitionList, bool? itaAccountCode)
        {
            if (!offerResponse.Offers.Any(x => x.UpsellOffer))
                return null;
            else
            {
                int serviceIdCountLocal = serviceIdCount;
                var airOffers = new List<NewOfferType>();

                foreach (var offer in offerResponse.Offers.Where(x => x.UpsellOffer))
                {
                    var totalAmount = offer.SalePrice.First(s => s.PriceComponentType == PriceComponentTypes.BasePriceEnum).Amount;

                    airOffers.Add(new NewOfferType()
                    {
                        OfferID = offer.OfferId,
                        OwnerCode = Constants.Application.OWNERCODE,
                        OfferItem = GetOfferItems(offer, genericDataMapper, offerResponse, paxSegmentList, priceClassList, originDestList, paxJourneyList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, ref serviceIdCountLocal, serviceDefinitionList, itaAccountCode,offerResponse?.FareTypeRequest?.FareTypeCode,offerResponse?.FareTypeRequest?.FareSubTypeCode),
                        JourneyOverview = GetJourneyOverview(offer.OfferItems?.FirstOrDefault(), offerResponse.DataList, genericDataMapper, originDestList, paxSegmentList, paxJourneyList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, priceClassList),
                        BaggageAssociations = GetBaggageAssociations(offer, offerResponse.DataList, genericDataMapper, originDestList, paxSegmentList, baggageAllowanceList, paxJourneyList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList),
                        OfferExpirationTimeLimitDateTime = Convert.ToDateTime(Convert.ToDateTime(offer.OfferExpirationTimeLimit).ToString("yyy-MM-ddTHH:mm:ss")).ToUniversalTime(),
                        OfferExpirationTimeLimitDateTimeSpecified = true
                    });
                }

                serviceIdCount = serviceIdCountLocal;

                return new OtherOffersType { Offer = airOffers.ToArray() };
            }
        }

        private static NewOfferItemType[] GetOfferItems(OfferDetails offer, IGenericDataMapper genericDataMapper, OfferResponse offerResponse, List<PaxSegmentType> paxSegmentList, List<PriceClassType> priceClassList, List<OriginDestType> originDestList, List<PaxJourneyType> paxJourneyList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, ref int serviceIdCount, List<ServiceDefinitionType> serviceDefinitionList, bool? itaAccountCode, string fareTypeCode,string fareSubTypeCode)
        {
            int serviceIdCountLocal = serviceIdCount;
            var offerItemList = new List<NewOfferItemType>();
            bool accountCode = offerResponse.Parties.Participants.Any(x => x.AccountCode == "1");
            var baseCurrency = offer?.SalePrice?.FirstOrDefault()?.Amount?.BaseCurrency;
            var currencyDecimalFormat = DecimalHelper.GetDecimalFormat(offer?.SalePrice?.FirstOrDefault()!.Amount.MonetaryAmount);

            foreach (var offerItem in offer.OfferItems)
            {
                var currOfferBaseFare = offerItem.OfferitemPricing.First().SalePrice.First(s => s.PriceComponentType == PriceComponentTypes.BaseFareEnum);
                var currOfferBasePrice = offerItem.OfferitemPricing.First().SalePrice.First(s => s.PriceComponentType == PriceComponentTypes.BasePriceEnum);
                var currOfferTax = offerItem.OfferitemPricing.First().SalePrice.First(x => x.PriceComponentType == PriceComponentTypes.TotalTaxEnum);

                NewOfferItemType newOfferItem = new()
                {
                    MandatoryInd = true,
                    MandatoryIndSpecified = true,
                    OfferItemID = offerItem.OfferItemId,
                    FareDetail = GetFareDetail(offerItem, genericDataMapper, offerResponse.DataList, baseCurrency, paxSegmentList, priceClassList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, accountCode, currencyDecimalFormat, itaAccountCode,  fareTypeCode, fareSubTypeCode),
                    Price = new()
                    {
                        BaseAmount = new AmountType()
                        {
                            CurCode = currOfferBasePrice.Amount.BaseCurrency,
                            Value = Convert.ToDecimal(currOfferBaseFare.Amount.MonetaryAmount)
                        },
                        TotalAmount = new AmountType()
                        {
                            CurCode = currOfferBasePrice.Amount.BaseCurrency,
                            Value = Convert.ToDecimal(currOfferBasePrice.Amount.MonetaryAmount),
                        },
                        TaxSummary = new List<TaxSummaryType>()
                        {
                           new TaxSummaryType()
                           {
                               TotalTaxAmount = new()
                               {
                                   CurCode = currOfferTax.Amount.BaseCurrency,
                                   Value = Convert.ToDecimal(currOfferTax.Amount.MonetaryAmount)
                               },
                               Tax = GetTaxBreakdown(currOfferTax.TaxBreakup)
                           }
                        }.ToArray()
                    },
                    PaymentTimeLimit = new()
                    {
                        Item = new PaymentTimeLimitDateType()
                        {
                            PaymentTimeLimitDateTime = Convert.ToDateTime(offer.TicketingTimeLimit).ToUniversalTime(),
                            PaymentTimeLimitDateTimeSpecified = true
                        }
                    }
                };

                var journeyIds = new List<string>();

                foreach (var offerItemService in offerItem.Services)
                {
                    var journeyID = GetPaxJourneyId(offerItemService, offerResponse.DataList, genericDataMapper, originDestList, paxSegmentList, paxJourneyList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList);
                    journeyIds.Add(journeyID);
                }


                var service = GetService(offerItem.Services, journeyIds, ref serviceIdCountLocal).ToList();
                service.AddRange(GetAmenitiesServiceList(offerItem.Services?.FirstOrDefault()?.ServiceBeneficiaries, offerItem.Services, offerResponse, genericDataMapper, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, ref serviceIdCountLocal, serviceDefinitionList));

                newOfferItem.Service = service.ToArray();

                offerItemList.Add(newOfferItem);
            }

            serviceIdCount = serviceIdCountLocal;
            return offerItemList.ToArray();
        }

        private static FareDetailType[] GetFareDetail(OfferItem offerItem, IGenericDataMapper genericDataMapper, DataList dataList, string baseCurrency, List<PaxSegmentType> paxSegmentList, List<PriceClassType> priceClassList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, bool accountCode, string currencyDecimalFormat, bool? itaAccountCode, string fareTypeCode, string fareSubTypeCode)
        {
            return new List<FareDetailType>
            {
                new FareDetailType
                {
                    PaxRefID = offerItem.Services?.FirstOrDefault()?.ServiceBeneficiaries.ToArray(),
                    FareComponent = GetFareComponent(offerItem.Services, genericDataMapper, dataList, baseCurrency,paxSegmentList,priceClassList, datedMarketingSegmentList, datedOperatingSegmentList,datedOperatingLegList,accountCode, currencyDecimalFormat,itaAccountCode, fareTypeCode, fareSubTypeCode),
                }
            }.ToArray();
        }

        private static FareComponentType[] GetFareComponent(IEnumerable<OfferService> offerServiceList, IGenericDataMapper genericDataMapper, DataList dataList, string baseCurrency, List<PaxSegmentType> paxSegmentList, List<PriceClassType> priceClassList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, bool accountCode, string currencyDecimalFormat, bool? itaAccountCode,string fareTypeCode, string fareSubTypeCode)
        {
            List<FareComponentType> fareComponents = new();

            foreach (var ond in dataList.OriginDestinationAssociations)
            {
                var services = dataList.Journey.Join(offerServiceList, journeyList => journeyList.JourneyId, service => service.JourneyRefIds.FirstOrDefault(), (journeyList, service) => new { journeyList, service }).Where(x => x.service.JourneyRefIds.Contains(x.journeyList.JourneyId)).Select(x => x.service);

                if (services != null && services.Any())
                {
                    var service = services.FirstOrDefault();

                    foreach (var serv in offerServiceList)
                    {
                        var paxSegmentRefIDs = new List<string>();

                        paxSegmentRefIDs.AddRange(GetPaxSegmentRefIDs(serv.ProductSegmentAssociation.OfferSegmentRefIds, genericDataMapper, dataList, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList));

                        var cancelRestrictionsList = new List<CancelRestrictionsType>();

                        foreach (var x in service.PenaltyRef?.CancellationPenaltyRefIds)
                        {
                            var penalty = dataList.Penalty.FirstOrDefault(p => p.Id == x);
                            cancelRestrictionsList.AddRange(GetCancelRestrictions(penalty, baseCurrency, currencyDecimalFormat));
                        }

                        var changeRestrictionsList = new List<ChangeRestrictionsType>();

                        foreach (var x in service.PenaltyRef.ChangePenaltyRefIds)
                        {
                            var penalty = dataList.Penalty.First(p => p.Id == x);

                            changeRestrictionsList.AddRange(GetChangeRestrictions(penalty, baseCurrency, currencyDecimalFormat));
                        }

                        var farePriceClassRefId = serv?.PriceClassRefIds?.FirstOrDefault();
                        var fareBasisCode = dataList.PriceClass.FirstOrDefault(x => x.Id == farePriceClassRefId)?.FareBasisCode;

                        fareComponents.Add(new()
                        {
                            CancelRestrictions = cancelRestrictionsList.Take(2).ToArray(),
                            ChangeRestrictions = changeRestrictionsList.Take(2).ToArray(),
                            FareTypeCode = fareSubTypeCode,
                            FareBasisCode = fareBasisCode,
                            NegotiatedCode = fareTypeCode,
                            PaxSegmentRefID = paxSegmentRefIDs.ToArray(),
                            PriceClassRefID = GetPriceClassId(service, dataList, priceClassList)
                        });
                    }
                }
                return fareComponents.ToArray();
            }

            return null;
        }

        private static JourneyOverviewType GetJourneyOverview(OfferItem offerItem, DataList dataList, IGenericDataMapper genericDataMapper, List<OriginDestType> originDestList, List<PaxSegmentType> paxSegmentList, List<PaxJourneyType> paxJourneyList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, List<PriceClassType> priceClassList)
        {
            JourneyOverviewType journeyOverviewType = new();
            List<JourneyPriceClassType> journeyPriceClassTypes = new();

            foreach (var offerItemService in offerItem.Services)
            {
                JourneyPriceClassType journeyPriceClassType = new()
                {
                    PaxJourneyRefID = GetPaxJourneyId(offerItemService, dataList, genericDataMapper, originDestList, paxSegmentList, paxJourneyList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList)
                };
                journeyPriceClassType.PriceClassRefID = GetPriceClassId(offerItemService, dataList, priceClassList);
                journeyPriceClassTypes.Add(journeyPriceClassType);
            }

            journeyOverviewType.JourneyPriceClass = journeyPriceClassTypes.ToArray();
            return journeyOverviewType;
        }
        
        private static BaggageAssociationsType[] GetBaggageAssociations(OfferDetails offer, DataList dataList, IGenericDataMapper genericDataMapper, List<OriginDestType> originDestList, List<PaxSegmentType> paxSegmentList, List<BaggageAllowanceType> baggageAllowanceList, List<PaxJourneyType> paxJourneyList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList)
        {
            var associations = new Dictionary<string, BaggageAssociationsType>();
            var services = offer.OfferItems.SelectMany(o => o.Services);
            List<string> existingJourneys = new List<string>();
            
            foreach (var service in services)
            {
                if (service.BaggageRefIds == null ) continue;

                var paxRefIds = service.ServiceBeneficiaries?.ToList() ?? new List<string>();
                bool isInfantService = paxRefIds.Any(x => x.Contains(Constants.Application.Infant));

                foreach (var bagId in service.BaggageRefIds)
                {
                    var baggage = dataList.BaggageAllowances?.FirstOrDefault(x => x.BaggageAllowanceId == bagId);

                    if (baggage == null) continue;

                    var allowanceId = GetBaggageAllowanceId(baggage, baggageAllowanceList);
                    var journeyIds = GetJourneyIDs(service, dataList, genericDataMapper,originDestList, paxSegmentList, paxJourneyList,datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList)?.Distinct().ToList() ?? new List<string>();

                    if (!associations.TryGetValue(allowanceId, out var assoc))
                    {
                        assoc = new BaggageAssociationsType
                        {
                            BaggageAllowanceRefID = new[] { allowanceId },
                            PaxRefID = service.ServiceBeneficiaries.ToArray(),
                            OfferFlightAssociations = new OfferFlightAssociationsType
                            {
                                Item = new PaxJourneyRefType
                                {
                                    PaxJourneyRefID = [.. journeyIds]
                                },
                                ItemElementName = ItemChoiceType1.PaxJourneyRef
                            }
                        };

                        associations[allowanceId] = assoc;
                    }
                    else
                    {
                        var paxList = assoc.PaxRefID.ToList();
                        paxList.AddRange(service.ServiceBeneficiaries);
                        assoc.PaxRefID = paxList.Distinct().ToArray();

                        existingJourneys = ((PaxJourneyRefType)assoc.OfferFlightAssociations.Item).PaxJourneyRefID.ToList();
                        existingJourneys.AddRange(journeyIds);
                        ((PaxJourneyRefType)assoc.OfferFlightAssociations.Item).PaxJourneyRefID = existingJourneys.Distinct().ToArray();
                    }
                }
            }

            return [.. associations.Values.Distinct()];
        }

        private static string[] GetJourneyIDs(OfferService service, DataList dataList, IGenericDataMapper genericDataMapper, List<OriginDestType> originDestList, List<PaxSegmentType> paxSegmentList, List<PaxJourneyType> paxJourneyList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList)
        {
            return new string[1]
            {
                GetPaxJourneyId(service, dataList, genericDataMapper,originDestList, paxSegmentList, paxJourneyList,datedMarketingSegmentList, datedOperatingSegmentList,datedOperatingLegList)
            };
        }

        public static string GetBaggageAllowanceId(BaggageAllowances baggage, List<BaggageAllowanceType> baggageAllowanceList)
        {
            var checkforBaggageAllowanceExists = CheckBaggageAllowanceExistance(baggage, baggageAllowanceList);

            if (checkforBaggageAllowanceExists != null)
            {
                return checkforBaggageAllowanceExists.BaggageAllowanceID;
            }
            else
            {
                string bagID = "";
                if (baggage.BaggageAllowanceTypeCode == Constants.Application.CheckedBag)
                {
                    bagID = IdCreator.CreateBaggageAllowanceID(baggageAllowanceList.Count(x => x.BaggageAllowanceID[..4] == Constants.Application.BAGA) + 1);
                }
                else if (baggage.BaggageAllowanceTypeCode == Constants.Application.CarryOnBag)
                {
                    bagID = IdCreator.CreateCabinBaggageAllowanceID(baggageAllowanceList.Count(x => x.BaggageAllowanceID[..3] == Constants.Application.CAB) + 1);
                }

                List<String> DescText = new();

                if (baggage.FreeBaggageAllowance.BagDescriptors != null)
                {
                    foreach (BagDescriptor BagDes in baggage.FreeBaggageAllowance.BagDescriptors)
                    {
                        DescText.Add(BagDes.BagDescriptorName);
                    }
                }

                var baggageAllowance = new BaggageAllowanceType()
                {
                    BaggageAllowanceID = bagID,
                    TypeCode = baggage.BaggageAllowanceTypeCode,
                    PieceAllowance = new PieceAllowanceType()
                    {
                        TotalQty = Convert.ToDecimal(baggage.FreeBaggageAllowance.AllowedBagCnt)
                    },
                    DescText = DescText.ToArray()
                };

                baggageAllowance = GetBaggageAllowanceTypeCodeDetails(baggage, baggageAllowance);

                baggageAllowanceList.Add(baggageAllowance);

                return baggageAllowance.BaggageAllowanceID;
            }
        }

        private static BaggageAllowanceType GetBaggageAllowanceTypeCodeDetails(BaggageAllowances baggage, BaggageAllowanceType baggageAllowance)
        {
            baggageAllowance.MaximumDimensionAllowance = baggage?.FreeBaggageAllowance?.DimensionAllowance?.Select(dA => new DimensionAllowanceType { HeightMeasure = Convert.ToDecimal(dA.HeightMeasure), WidthMeasure = Convert.ToDecimal(dA.WidthMeasure), LengthMeasure = Convert.ToDecimal(dA.LengthMeasure), LinearMeasure = Convert.ToDecimal(dA.LinearMeasure), LengthUnitOfMeasurement = (LengthUnitCodeType)dA.LengthUnitOfMeasurement, HeightMeasureSpecified = true, WidthMeasureSpecified = true, LengthMeasureSpecified = true, LinearMeasureSpecified = true }).ToArray();

            baggageAllowance.WeightAllowance = baggage?.FreeBaggageAllowance?.WeightAllowance?.Select(dA => new WeightAllowanceType { MaximumWeightMeasure = Convert.ToDecimal(dA.MaximumWeightMeasure), WeightUnitOfMeasurement = (WeightUnitCodeType)dA.WeightUnitOfMeasurement, MaximumWeightMeasureSpecified = true, TotalMaximumWeightMeasureSpecified = true, TotalMaximumWeightMeasure = baggageAllowance.PieceAllowance.TotalQty * Convert.ToDecimal(dA.MaximumWeightMeasure) }).ToArray();

            return baggageAllowance;
        }

        private static BaggageAllowanceType CheckBaggageAllowanceExistance(BaggageAllowances baggage, List<BaggageAllowanceType> baggageAllowanceList)
        {
            if (baggage.FreeBaggageAllowance?.DimensionAllowance != null && baggage.FreeBaggageAllowance?.WeightAllowance != null)
            {

                var existingPBagAllowanceType = baggageAllowanceList.Find(bal => bal.TypeCode == baggage.BaggageAllowanceTypeCode
                                                && bal.PieceAllowance?.TotalQty == baggage.FreeBaggageAllowance?.AllowedBagCnt
                                                && bal.MaximumDimensionAllowance?.FirstOrDefault()?.LengthMeasure == Convert.ToDecimal(baggage.FreeBaggageAllowance?.DimensionAllowance?.FirstOrDefault()?.LengthMeasure)
                                                && bal.MaximumDimensionAllowance?.FirstOrDefault()?.LengthUnitOfMeasurement.ToString() == baggage.FreeBaggageAllowance?.DimensionAllowance?.FirstOrDefault()?.LengthUnitOfMeasurement.ToString()
                                                && bal.WeightAllowance?.FirstOrDefault()?.MaximumWeightMeasure == Convert.ToDecimal(baggage.FreeBaggageAllowance?.WeightAllowance?.FirstOrDefault()?.MaximumWeightMeasure)
                                                && bal.WeightAllowance?.FirstOrDefault()?.WeightUnitOfMeasurement.ToString() == baggage.FreeBaggageAllowance?.WeightAllowance?.FirstOrDefault()?.WeightUnitOfMeasurement.ToString());


                return existingPBagAllowanceType;
            }
            else
            {
                var existingCodeShareBagAllowanceType = baggageAllowanceList.FirstOrDefault(bal => bal.TypeCode == baggage.BaggageAllowanceTypeCode
                                                && bal?.PieceAllowance?.TotalQty == baggage.FreeBaggageAllowance?.AllowedBagCnt
                                                && ((!(bal.WeightAllowance?.Any() ?? false) && !(baggage.FreeBaggageAllowance?.WeightAllowance?.Any() ?? false)) || (bal?.WeightAllowance?.FirstOrDefault()?.MaximumWeightMeasure == Convert.ToDecimal(baggage.FreeBaggageAllowance?.WeightAllowance?.FirstOrDefault()?.MaximumWeightMeasure)
                                                && bal?.WeightAllowance?.FirstOrDefault()?.WeightUnitOfMeasurement.ToString() == baggage.FreeBaggageAllowance?.WeightAllowance?.FirstOrDefault()?.WeightUnitOfMeasurement.ToString())));

                return existingCodeShareBagAllowanceType;
            }
        }

        private static string[] GetPaxSegmentRefIDs(IEnumerable<string> offerSegmentRefIds, IGenericDataMapper genericDataMapper, DataList dataList, List<PaxSegmentType> paxSegmentList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList)
        {
            List<string> paxSegIDs = new();
            foreach (var oSegID in offerSegmentRefIds)
            {
                var offerSeg = dataList.OfferSegments.First(seg => seg.OfferSegmentId == oSegID);
                paxSegIDs.Add(GetPaxSegmentId(genericDataMapper, offerSeg, dataList.FlightSegments.First(seg => seg.SegmentId == offerSeg.FlightSegmentRefId), paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList));
            }

            return paxSegIDs.ToArray();
        }

        private static string GetPaxSegmentId(IGenericDataMapper genericDataMapper, OfferSegment offerFlightSegment, Segment flightSegment, List<PaxSegmentType> paxSegmentList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList)
        {
            if (paxSegmentList != null && paxSegmentList.Any(seg => seg.MarketingCarrierRBD_Code == offerFlightSegment.ClassOfService && (seg.CabinTypeAssociationChoice.Item as CabinTypeType).CabinTypeName == offerFlightSegment.CabinType.CabinName && (seg.CabinTypeAssociationChoice.Item as CabinTypeType).CabinTypeCode == offerFlightSegment.CabinType.CabinCode && seg.DatedMarketingSegmentRefId == CreateDatedMarketingSegmentType(flightSegment, genericDataMapper, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList)))
            {
                var paxId = paxSegmentList.First(seg => seg.MarketingCarrierRBD_Code == offerFlightSegment.ClassOfService && (seg.CabinTypeAssociationChoice.Item as CabinTypeType).CabinTypeName == offerFlightSegment.CabinType.CabinName && (seg.CabinTypeAssociationChoice.Item as CabinTypeType).CabinTypeCode == offerFlightSegment.CabinType.CabinCode && seg.DatedMarketingSegmentRefId == CreateDatedMarketingSegmentType(flightSegment, genericDataMapper, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList)).PaxSegmentID;
                return paxId;
            }
            else
            {
                var paxSegmentType = CreatePaxSegmentType(offerFlightSegment, genericDataMapper, flightSegment, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList);
                paxSegmentList?.Add(paxSegmentType);
                return paxSegmentType.PaxSegmentID;
            }
        }

        public static string GetPaxJourneyId(OfferService offerService, DataList dataList, IGenericDataMapper genericDataMapper, List<OriginDestType> originDestList, List<PaxSegmentType> paxSegmentList, List<PaxJourneyType> paxJourneyList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList)
        {
            var duration = dataList.Journey.First(j => j.JourneyId == offerService.JourneyRefIds.FirstOrDefault()).Duration;
            TimeSpan journeyDuration = TimeSpan.Parse(duration, CultureInfo.InvariantCulture);

            var segments = new List<string>();

            foreach (var oSegID in offerService.ProductSegmentAssociation.OfferSegmentRefIds)
            {
                var offerSeg = dataList.OfferSegments.First(seg => seg.OfferSegmentId == oSegID);
                segments.Add(GetPaxSegmentId(genericDataMapper, offerSeg, dataList.FlightSegments.First(seg => seg.SegmentId == offerSeg.FlightSegmentRefId), paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList));
            }

            var paxJourneyId = GetPaxJourneyId(journeyDuration, segments, paxJourneyList);

            AddJourneyReftoODList(paxJourneyId, offerService.JourneyRefIds, dataList, originDestList);

            return paxJourneyId;
        }

        private static void AddJourneyReftoODList(string paxJourneyId, IEnumerable<string> JourneyRefIds, DataList dataList, List<OriginDestType> originDestList)
        {
            var ODRef = dataList.OriginDestinationAssociations.First(od => od.JourneyRefIds.Contains(JourneyRefIds.FirstOrDefault())).OdLabel;

            foreach (var ond in originDestList)
            {
                var paxJourneyrefIds = ond.PaxJourneyRefID.ToList();
                if (ond.OriginDestID == ODRef && !ond.PaxJourneyRefID.Any(jour => jour == paxJourneyId))
                {
                    paxJourneyrefIds.Add(paxJourneyId);
                    ond.PaxJourneyRefID = paxJourneyrefIds.ToArray();
                }
            }

        }

        public static string GetPaxJourneyId(TimeSpan journeyDuration, IEnumerable<string> segments, List<PaxJourneyType> paxJourneyList)
        {
            if (paxJourneyList.Any(x => x.PaxSegmentRefID.SequenceEqual(segments)))
            {
                var journeyId = paxJourneyList.First(x => x.PaxSegmentRefID.SequenceEqual(segments)).PaxJourneyID;
                return journeyId;
            }
            else
            {
                var paxJourneyType = new PaxJourneyType()
                {
                    PaxJourneyID = IdCreator.CreatePaxJourneyID(paxJourneyList.Count),
                    PaxSegmentRefID = segments.ToArray(),
                    Duration = string.Format("{0}{1}{2}{3}{4}{5}{6}", Constants.Application.PT, journeyDuration.Days * 24 + journeyDuration.Hours, Constants.Application.H, journeyDuration.Minutes, Constants.Application.M, journeyDuration.Seconds, Constants.Application.S)
                };
                paxJourneyList.Add(paxJourneyType);
                return paxJourneyType.PaxJourneyID;
            }
        }

        private static PaxSegmentType CreatePaxSegmentType(OfferSegment offerFlightSegment, IGenericDataMapper genericDataMapper, Segment flightSegment, List<PaxSegmentType> paxSegmentList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList)
        {
            return new PaxSegmentType()
            {
                PaxSegmentID = IdCreator.CreatePaxSegmentID(paxSegmentList.Count),
                CabinTypeAssociationChoice = GetCabinType(offerFlightSegment),
                MarketingCarrierRBD_Code = offerFlightSegment.ClassOfService,
                DatedMarketingSegmentRefId = CreateDatedMarketingSegmentType(flightSegment, genericDataMapper, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList)
            };
        }

        private static CabinTypeAssociationChoiceType GetCabinType(OfferSegment offerFlightSegment)
        {
            return new CabinTypeAssociationChoiceType()
            {
                Item = new CabinTypeType()
                {
                    CabinTypeCode = offerFlightSegment?.CabinType?.CabinCode,
                    CabinTypeName = offerFlightSegment?.CabinType?.CabinName
                }
            };
        }

        public static string GetPriceClassId(OfferService s, DataList dataList, List<PriceClassType> priceClassList)
        {
            var priceClass = dataList.PriceClass.First(p => p.Id == s.PriceClassRefIds.FirstOrDefault());
            if (priceClassList.Any(x => x.Name == priceClass.ProductName))
            {
                var priceClassID = priceClassList.First(x => x.Name == priceClass.ProductName).PriceClassID;
                return priceClassID;
            }
            else
            {
                List<CabinTypeType> cabinType = new List<CabinTypeType>();
                cabinType.Add(new CabinTypeType
                {
                    CabinTypeCode = priceClass.CabinType?.CabinCode,
                    CabinTypeName = priceClass.CabinType?.CabinName
                });
                var priceClassType = new PriceClassType()
                {
                    PriceClassID = IdCreator.CreatePriceClassID(priceClassList.Count),
                    Name = priceClass.ProductName,
                    CabinType = cabinType.ToArray()
                };

                priceClassList.Add(priceClassType);
                return priceClassType.PriceClassID;
            }
        }

        private static OfferedServiceType[] GetService(IEnumerable<OfferService> services, IEnumerable<string> journeyIds, ref int serviceIdCount)
        {
            int serviceIdCountLocal = serviceIdCount;

            var offeredServiceResponse = new List<OfferedServiceType>
            {
                new()
                {
                    OfferServiceAssociation = new OfferServiceAssociationType()
                    {
                        Item = new PaxJourneyRefType()
                        {
                            PaxJourneyRefID = journeyIds.ToArray()
                        }
                    },
                    ServiceID = IdCreator.CreateServiceID(++serviceIdCountLocal),
                    PaxRefID = services.First().ServiceBeneficiaries.ToArray(),
                }
            }.ToArray();

            serviceIdCount = serviceIdCountLocal;

            return offeredServiceResponse;
        }

        private static List<OfferedServiceType> GetAmenitiesServiceList(IEnumerable<string> paxList, IEnumerable<OfferService> serviceOfferings, OfferResponse offerResponse, IGenericDataMapper genericDataMapper, List<PaxSegmentType> paxSegmentList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, ref int serviceIdCount, List<ServiceDefinitionType> serviceDefinitionList)
        {
            int serviceIdCountLocal = serviceIdCount;
            List<OfferedServiceType> serviceList = new();

            foreach (OfferService ser in serviceOfferings)
            {
                foreach (var offSegRef in ser.ProductSegmentAssociation?.OfferSegmentRefIds ?? Enumerable.Empty<string>())
                {
                    // Find the OfferSegment and FlightSegment once
                    var offerSegment = offerResponse.DataList?.OfferSegments.FirstOrDefault(seg => seg.OfferSegmentId == offSegRef);
                    if (offerSegment == null) continue; // No matching OfferSegment

                    var flightSegment = offerResponse.DataList?.FlightSegments.FirstOrDefault(seg => seg.SegmentId == offerSegment.FlightSegmentRefId);
                    if (flightSegment == null) continue; // No matching FlightSegment

                    // Get ProductComponentRefIds once
                    var productComponentRefIds = offerSegment.ProductComponentRefIds;
                    if (productComponentRefIds == null || !productComponentRefIds.Any()) continue;

                    // Get service definition ID and segment ID
                    var serviceDefID = GetServiceList(serviceDefinitionList, flightSegment.OperatingCarrier ?? flightSegment.MarketingCarrier, offerResponse.DataList?.ProductComponents.Where(product => productComponentRefIds.Contains(product.Id)));
                    var segmentID = GetPaxSegmentId(genericDataMapper, offerSegment, flightSegment, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList);

                    // Check if service already exists in serviceList
                    var existingService = serviceList.Find(ser =>
                        (ser.OfferServiceAssociation.Item as OfferServiceDefinitionRefType)?.ServiceDefinitionRefID == serviceDefID);

                    if (existingService == null)
                    {
                        // Create a new service
                        var newService = new OfferedServiceType
                        {
                            ServiceID = IdCreator.CreateServiceID(++serviceIdCountLocal),
                            PaxRefID = paxList.ToArray(),
                            OfferServiceAssociation = GetOfferServiceAssociation()
                        };

                        var serviceDefRef = newService.OfferServiceAssociation.Item as OfferServiceDefinitionRefType;
                        if (serviceDefRef != null)
                        {
                            serviceDefRef.ServiceDefinitionRefID = serviceDefID;
                            serviceDefRef.OfferFlightAssociations.Item = new PaxSegmentRefListType
                            {
                                PaxSegmentRefID = new[] { segmentID }
                            };
                        }

                        serviceList.Add(newService);
                    }
                    else
                    {
                        // Add the segment ID to an existing service
                        var existingServiceDefRef = existingService.OfferServiceAssociation.Item as OfferServiceDefinitionRefType;
                        if (existingServiceDefRef != null)
                        {
                            var existingSegmentList = existingServiceDefRef.OfferFlightAssociations.Item as PaxSegmentRefListType;
                            if (existingSegmentList != null)
                            {
                                var segmentIDList = existingSegmentList.PaxSegmentRefID.ToList();
                                segmentIDList.Add(segmentID);
                                existingSegmentList.PaxSegmentRefID = segmentIDList.ToArray();
                            }
                        }
                    }
                }
            }

            serviceIdCount = serviceIdCountLocal;

            return serviceList;
        }

        private static string GetServiceList(List<ServiceDefinitionType> serviceDefinitionList, string operatingCarrier, IEnumerable<ProductComponent> services)
        {
            // Build a new definition using existing logic
            var newServiceDef = BuildServiceDefinition(operatingCarrier, services);

            // Try to find a matching one already in the list
            var existing = serviceDefinitionList.FirstOrDefault(existing =>
                existing.OwnerCode == newServiceDef.OwnerCode &&
                existing.Name == newServiceDef.Name &&
                existing.Desc.Length == newServiceDef.Desc.Length &&
                !existing.Desc.Where((desc, i) => desc.DescText != newServiceDef.Desc[i].DescText).Any());

            if (existing != null)
            {
                // A match exists — reuse its ID
                return existing.ServiceDefinitionID;
            }

            // No match — assign new ID and add it
            newServiceDef.ServiceDefinitionID = $"SRV{serviceDefinitionList.Count + 1}";
            serviceDefinitionList.Add(newServiceDef);
            return newServiceDef.ServiceDefinitionID;
        }

        private static ServiceDefinitionType BuildServiceDefinition(string operatingCarrier, IEnumerable<ProductComponent> productComponents)
        {
           var descList = productComponents.Where(p => !string.IsNullOrWhiteSpace(p.Description))
           .Select(p => p.Description.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(desc => desc, StringComparer.OrdinalIgnoreCase)
           .Select(desc => new DescType
           {
              DescText = desc
           }).ToArray();

            return new ServiceDefinitionType
            {
                OwnerCode = operatingCarrier,
                Name = Constants.Application.Amenity,
                Desc = [.. descList]
            };
        }

        private static OfferServiceAssociationType GetOfferServiceAssociation()
        {
            return new()
            {
                Item = new OfferServiceDefinitionRefType()
                {
                    OfferFlightAssociations = new OfferFlightAssociationsType()
                    {
                        ItemElementName = ItemChoiceType1.PaxSegmentReferences,
                        Item = new PaxSegmentRefListType()
                    }
                }
            };
        }

        private static string CreateDatedMarketingSegmentType(Segment flightSegment, IGenericDataMapper genericDataMapper, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList)
        {
            if (datedMarketingSegmentList.Any(dMS => dMS.CarrierDesigCode == flightSegment.MarketingCarrier
                                                    && dMS.CarrierName == genericDataMapper.GetOperatingCarrierNameByCode(flightSegment.MarketingCarrier)
                                                    && dMS.Arrival.AircraftScheduledDateTime == Convert.ToDateTime(flightSegment.ScheduledArrivalTime.LocalDateTime)
                                                    && dMS.Dep.AircraftScheduledDateTime == Convert.ToDateTime(flightSegment.ScheduledDepartureTime.LocalDateTime)
                                                    && dMS.MarketingCarrierFlightNumberText == flightSegment.MarketingFlightNumber
                                                    )
                )
            {
                return datedMarketingSegmentList.First(dMS => dMS.CarrierDesigCode == flightSegment.MarketingCarrier
                                                            && dMS.CarrierName == genericDataMapper.GetOperatingCarrierNameByCode(flightSegment.MarketingCarrier)
                                                            && dMS.Arrival.AircraftScheduledDateTime == Convert.ToDateTime(flightSegment.ScheduledArrivalTime.LocalDateTime) && dMS.Dep.AircraftScheduledDateTime == Convert.ToDateTime(flightSegment.ScheduledDepartureTime.LocalDateTime)
                                                            && dMS.MarketingCarrierFlightNumberText == flightSegment.MarketingFlightNumber
                                                       ).DatedMarketingSegmentId;
            }
            else
            {
                var datedMarketingSegmentType = new DatedMarketingSegmentType();
                var TransportArrivalType = new TransportPointType();
                TransportArrivalType.IATA_LocationCode = flightSegment.DestinationAirportCode;
                TransportArrivalType.AircraftScheduledDateTime = Convert.ToDateTime(flightSegment?.ScheduledArrivalTime.LocalDateTime);
                TransportArrivalType.AircraftScheduledDateTimeSpecified = true;
                TransportArrivalType.TerminalName = flightSegment.Legs?.FirstOrDefault()?.DestinationTerminal;
                datedMarketingSegmentType.Arrival = TransportArrivalType;

                var TransportDepType = new TransportPointType
                {
                    IATA_LocationCode = flightSegment.OriginAirportCode,
                    AircraftScheduledDateTime = Convert.ToDateTime(flightSegment.ScheduledDepartureTime.LocalDateTime),
                    AircraftScheduledDateTimeSpecified = true,
                    TerminalName = flightSegment.Legs?.FirstOrDefault()?.OriginTerminal
                };

                datedMarketingSegmentType.Dep = TransportDepType;
                datedMarketingSegmentType.CarrierDesigCode = flightSegment.MarketingCarrier;
                datedMarketingSegmentType.CarrierName = genericDataMapper.GetOperatingCarrierNameByCode(flightSegment.MarketingCarrier);
                datedMarketingSegmentType.MarketingCarrierFlightNumberText = flightSegment.MarketingFlightNumber;
                datedMarketingSegmentType.DatedMarketingSegmentId = IdCreator.CreateDatedMarketingSegmentID(datedMarketingSegmentList.Count);
                datedMarketingSegmentType.DatedOperatingSegmentRefId = GetDatedOperatingSegmentID(flightSegment, datedOperatingSegmentList.Count, genericDataMapper, datedOperatingSegmentList, datedOperatingLegList);

                datedMarketingSegmentList.Add(datedMarketingSegmentType);
                return datedMarketingSegmentType.DatedMarketingSegmentId;
            }
        }

        private static PaxType[] GetPaxDetails(IEnumerable<Customer> customers)
        {
            var paxList = customers.Select(customer => new PaxType()
            {
                PaxID = customer.CustId,
                PTC = customer.Ptc == Constants.Application.CNNPTCCode ? Constants.Application.ChildPTCCode : customer.Ptc
            }).ToList();

            var infPax = paxList.Where(x => x.PTC == Constants.Application.INF);
            if (infPax.Any())
            {
                var adtPax = paxList.Where(x => x.PTC == Constants.Application.ADT);

                for (int i = 0; i < infPax.Count(); i++)
                {
                    adtPax.ElementAt(i).PaxRefID = infPax.ElementAt(i).PaxID;
                }
            }
            return paxList.ToArray();
        }

        private static string GetDatedOperatingSegmentID(Segment flightSegment, int count, IGenericDataMapper genericDataMapper, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList)
        {
            // Creating the Dated Operating segment in response of dated MarketingSegment
            var datedOperatingSegment = CreateDatedOperatingSegment(flightSegment, count, genericDataMapper, datedOperatingLegList);

            datedOperatingSegmentList.Add(datedOperatingSegment);
            return datedOperatingSegment.DatedOperatingSegmentId;
        }

        private static DatedOperatingSegmentType CreateDatedOperatingSegment(Segment flightSegment, int count, IGenericDataMapper genericDataMapper, List<DatedOperatingLegType> datedOperatingLegList)
        {
            var flightDuration = TimeSpan.Parse(flightSegment.FlightDuration, CultureInfo.InvariantCulture);

            return new DatedOperatingSegmentType
            {
                DatedOperatingSegmentId = IdGenerator.CreateDatedOperatingSegmentID(count),
                CarrierName = string.IsNullOrEmpty(flightSegment.OperatingCarrier) ? genericDataMapper.GetOperatingCarrierNameByCode(flightSegment.MarketingCarrier) : genericDataMapper.GetOperatingCarrierNameByCode(flightSegment.OperatingCarrier),
                CarrierDesigCode = string.IsNullOrEmpty(flightSegment.OperatingCarrier) ? flightSegment.MarketingCarrier : flightSegment.OperatingCarrier,
                OperatingCarrierFlightNumberText = string.IsNullOrEmpty(flightSegment.OperatingFlightNumber) ? flightSegment.MarketingFlightNumber : flightSegment.OperatingFlightNumber,
                Duration = string.Format("{0}{1}{2}{3}{4}{5}{6}", Constants.Application.PT, flightDuration.Days * 24 + flightDuration.Hours, Constants.Application.H, flightDuration.Minutes, Constants.Application.M, flightDuration.Seconds, Constants.Application.S),
                DatedOperatingLegRefID = flightSegment.Legs.Select(leg => CreateDatedOperatingLegType(flightSegment.AirCraft, leg, datedOperatingLegList)).ToArray(),
                SecureFlightInd = Constants.Application.SecureFlightDefaultValue,
                SecureFlightIndSpecified = Constants.Application.SecureFlightDefaultValue
            };
        }

        private static string CreateDatedOperatingLegType(string aircraftTypecode, Legs leg, List<DatedOperatingLegType> datedOperatingLegList)
        {
            string onGroundDuration = string.Empty;

            if (!string.IsNullOrEmpty(leg.StopOverDuration))
            {
                TimeSpan timeSpan = TimeSpan.FromMinutes(Convert.ToDouble(leg.StopOverDuration));
                onGroundDuration = string.Format("{0}{1}{2}{3}{4}{5}{6}", Constants.Application.PT, timeSpan.Days * 24 + timeSpan.Hours, Constants.Application.H, timeSpan.Minutes, Constants.Application.M, timeSpan.Seconds, Constants.Application.S);
            }

            var datedOperatingLeg = new DatedOperatingLegType()
            {
                Arrival = new TransportPointType
                {
                    IATA_LocationCode = leg.DestinationAirportCode,
                    AircraftScheduledDateTime = Convert.ToDateTime(leg.ScheduledArrivalTime.LocalDateTime),
                    AircraftScheduledDateTimeSpecified = true,
                    TerminalName = leg.DestinationTerminal
                },
                Dep = new TransportPointType
                {
                    IATA_LocationCode = leg.OriginAirportCode,
                    AircraftScheduledDateTime = Convert.ToDateTime(leg.ScheduledDepartureTime.LocalDateTime),
                    AircraftScheduledDateTimeSpecified = true,
                    TerminalName = leg.OriginTerminal
                },
                CarrierAircraftType = string.IsNullOrEmpty(leg.CarrierAircraftTypeCode) ? null : new CarrierAircraftTypeType()
                {
                    CarrierAircraftTypeCode = leg.CarrierAircraftTypeCode,
                    CarrierAircraftTypeName = leg.CarrierAircraftTypeName,
                },
                IATA_AircraftType = new IATA_AircraftTypeType
                {
                    IATA_AircraftTypeCode = aircraftTypecode
                },
                DatedOperatingLegID = IdCreator.CreateDatedOperatingLegID(datedOperatingLegList.Count),
                OnGroundDuration = string.IsNullOrEmpty(onGroundDuration) ? null : onGroundDuration
            };

            datedOperatingLegList.Add(datedOperatingLeg);

            return datedOperatingLeg.DatedOperatingLegID;
        }

        private static List<ChangeRestrictionsType> GetChangeRestrictions(Penalty penalty, string baseCurrency, string currencyDecimalFormat)
        {
            string descriptionText = penalty.Text.Split(':').LastOrDefault()!.Trim();

            switch (descriptionText)
            {
                case Constants.Application.Free:
                case Constants.Application.ChangeAllowedFree:
                    return new List<ChangeRestrictionsType> {
                        new ChangeRestrictionsType
                        {
                            AllowedModificationInd = true,
                            AllowedModificationIndSpecified = true,
                            JourneyStageCode = GetJourneyStageCode(penalty.Text),
                            JourneyStageCodeSpecified = true,
                            Fee = new ChangeOrCancelFeeType {
                                Amount = new AmountType {
                                    CurCode = baseCurrency,
                                    Value = Convert.ToDecimal(string.Format(currencyDecimalFormat, 0))
                                }
                            }
                        }
                    };
                case Constants.Application.ChangeNotAllowed:
                    return new List<ChangeRestrictionsType> {
                        new ChangeRestrictionsType
                        {
                            AllowedModificationInd = false,
                            AllowedModificationIndSpecified = true,
                            JourneyStageCode = JourneyStageTypeCodeType.PriorToDeparture,
                            JourneyStageCodeSpecified = true
                        },
                        new ChangeRestrictionsType
                        {
                            AllowedModificationInd = false,
                            AllowedModificationIndSpecified = true,
                            JourneyStageCode = JourneyStageTypeCodeType.AfterDeparture,
                            JourneyStageCodeSpecified = true
                        }
                    };
                default:
                    if (descriptionText.Contains(Constants.Application.ChangeAllowedFee))
                    {
                        penalty.MaxPrice.Amount.MonetaryAmount = Regex.Match(descriptionText, @"\d+.+\d").Value;
                        currencyDecimalFormat = penalty.MaxPrice.Amount.MonetaryAmount is null ? currencyDecimalFormat : DecimalHelper.GetDecimalFormat(penalty.MaxPrice.Amount.MonetaryAmount);

                        return new List<ChangeRestrictionsType> {
                            new ChangeRestrictionsType
                            {
                                AllowedModificationInd = true,
                                AllowedModificationIndSpecified = true,
                                JourneyStageCode = JourneyStageTypeCodeType.PriorToDeparture,
                                JourneyStageCodeSpecified = true,
                                Fee = new ChangeOrCancelFeeType {
                                    Amount = new AmountType {
                                        CurCode = baseCurrency,
                                        Value = decimal.TryParse(penalty.MaxPrice.Amount?.MonetaryAmount, out var v) ? v : 0m
                                    }
                                }
                            }, new ChangeRestrictionsType
                            {
                                AllowedModificationInd = true,
                                AllowedModificationIndSpecified = true,
                                JourneyStageCode = JourneyStageTypeCodeType.AfterDeparture,
                                JourneyStageCodeSpecified = true,
                                Fee = new ChangeOrCancelFeeType {
                                    Amount = new AmountType {
                                        CurCode = baseCurrency,
                                        Value = decimal.TryParse(penalty.MaxPrice.Amount?.MonetaryAmount, out var v1) ? v1 : 0m
                                    }
                                }
                            }
                        };
                    }
                    return new List<ChangeRestrictionsType> {
                        new ChangeRestrictionsType
                        {
                            AllowedModificationInd = true,
                            AllowedModificationIndSpecified = true,
                            JourneyStageCode = GetJourneyStageCode(penalty.Text),
                            JourneyStageCodeSpecified = true,
                            Fee = new ChangeOrCancelFeeType {
                                Amount = new AmountType {
                                    CurCode = baseCurrency,
                                    Value = string.IsNullOrEmpty(penalty.MaxPrice.Amount?.MonetaryAmount)  ?  Convert.ToDecimal(string.Format(currencyDecimalFormat, 0)): Convert.ToDecimal(string.Format(currencyDecimalFormat, penalty.MaxPrice.Amount.MonetaryAmount))
                                }
                            }
                        }
                    };
            }
        }

        private static List<CancelRestrictionsType> GetCancelRestrictions(Penalty penalty, string baseCurrency, string currencyDecimalFormat)
        {
            string descriptionText = penalty.Text.Split(':').LastOrDefault()!.Trim();
            string journeyStageCodeText = penalty.Text.Split(':').FirstOrDefault()!.Trim();

            if (descriptionText != journeyStageCodeText)
            {
                switch (descriptionText)
                {
                    case Constants.Application.PartialRefund:
                        return new List<CancelRestrictionsType> {
                        new CancelRestrictionsType
                        {
                            AllowedModificationInd = true,
                            AllowedModificationIndSpecified = true,
                            JourneyStageCode = GetJourneyStageCode(journeyStageCodeText),
                            JourneyStageCodeSpecified = true,
                            Fee = new ChangeOrCancelFeeType {
                                Amount = new AmountType {
                                    CurCode = baseCurrency,
                                    Value = Convert.ToDecimal(string.Format(currencyDecimalFormat, 0))
                                }
                            }
                        }
                    };
                    case Constants.Application.FullyRefundable:
                        return new List<CancelRestrictionsType> {
                        new CancelRestrictionsType
                        {
                            AllowedModificationInd = true,
                            AllowedModificationIndSpecified = true,
                            JourneyStageCode = JourneyStageTypeCodeType.PriorToDeparture,
                            JourneyStageCodeSpecified = true,
                            Fee = new ChangeOrCancelFeeType {
                                Amount = new AmountType {
                                    CurCode = baseCurrency,
                                    Value = Convert.ToDecimal(string.Format(currencyDecimalFormat, 0))
                                }
                            }
                        }, new CancelRestrictionsType
                        {
                            AllowedModificationInd = true,
                            AllowedModificationIndSpecified = true,
                            JourneyStageCode = JourneyStageTypeCodeType.AfterDeparture,
                            JourneyStageCodeSpecified = true,
                            Fee = new ChangeOrCancelFeeType {
                                Amount = new AmountType {
                                    CurCode = baseCurrency,
                                    Value = Convert.ToDecimal(string.Format(currencyDecimalFormat, 0))
                                }
                            }
                        }
                    };
                    case Constants.Application.NonRefundable:
                    default:
                        return new List<CancelRestrictionsType> {
                        new CancelRestrictionsType
                        {
                            AllowedModificationInd = false,
                            AllowedModificationIndSpecified = true,
                            JourneyStageCode =GetJourneyStageCode(journeyStageCodeText),
                            JourneyStageCodeSpecified = true
                        }
                    };
                }
            }
            else
            {
                switch (descriptionText)
                {
                    case Constants.Application.FullyRefundable:
                        return new List<CancelRestrictionsType> {
                        new CancelRestrictionsType
                        {
                            AllowedModificationInd = true,
                            AllowedModificationIndSpecified = true,
                            JourneyStageCode = JourneyStageTypeCodeType.PriorToDeparture,
                            JourneyStageCodeSpecified = true,
                            Fee = new ChangeOrCancelFeeType {
                                Amount = new AmountType {
                                    CurCode = baseCurrency,
                                    Value = Convert.ToDecimal(string.Format(currencyDecimalFormat, 0))
                                }
                            }
                        }, new CancelRestrictionsType
                        {
                            AllowedModificationInd = true,
                            AllowedModificationIndSpecified = true,
                            JourneyStageCode = JourneyStageTypeCodeType.AfterDeparture,
                            JourneyStageCodeSpecified = true,
                            Fee = new ChangeOrCancelFeeType {
                                Amount = new AmountType {
                                    CurCode = baseCurrency,
                                    Value = Convert.ToDecimal(string.Format(currencyDecimalFormat, 0))
                                }
                            }
                        }
                    };
                    default:
                        if (descriptionText.Contains(Constants.Application.TicketRefundFee))
                        {
                            penalty.MaxPrice.Amount.MonetaryAmount = Regex.Match(descriptionText, @"\d+.+\d").Value;
                            currencyDecimalFormat = penalty.MaxPrice.Amount.MonetaryAmount is null ? currencyDecimalFormat : DecimalHelper.GetDecimalFormat(penalty.MaxPrice.Amount.MonetaryAmount);
                        }
                        return new List<CancelRestrictionsType> {
                            new CancelRestrictionsType
                            {
                                AllowedModificationInd = true,
                                AllowedModificationIndSpecified = true,
                                JourneyStageCode = JourneyStageTypeCodeType.PriorToDeparture,
                                JourneyStageCodeSpecified = true,
                                Fee = new ChangeOrCancelFeeType {
                                    Amount = new AmountType {
                                        CurCode = baseCurrency,
                                        Value = string.IsNullOrEmpty(penalty.MaxPrice.Amount?.MonetaryAmount) ? Convert.ToDecimal(string.Format(currencyDecimalFormat, 0)): Convert.ToDecimal(string.Format(currencyDecimalFormat, penalty.MaxPrice.Amount.MonetaryAmount))
                                    }
                                }
                            }, new CancelRestrictionsType
                            {
                                AllowedModificationInd = true,
                                AllowedModificationIndSpecified = true,
                                JourneyStageCode = JourneyStageTypeCodeType.AfterDeparture,
                                JourneyStageCodeSpecified = true,
                                Fee = new ChangeOrCancelFeeType {
                                    Amount = new AmountType {
                                        CurCode = baseCurrency,
                                        Value = string.IsNullOrEmpty(penalty.MaxPrice.Amount?.MonetaryAmount) ? Convert.ToDecimal(string.Format(currencyDecimalFormat, 0)): Convert.ToDecimal(string.Format(currencyDecimalFormat, penalty.MaxPrice.Amount.MonetaryAmount))
                                    }
                                }
                            }
                        };
                }
            }
        }

        private static JourneyStageTypeCodeType GetJourneyStageCode(string descText)
        {
            if (descText.Contains(Constants.Application.AfterDeparture, StringComparison.OrdinalIgnoreCase))
                return JourneyStageTypeCodeType.AfterDeparture;
            else
                return JourneyStageTypeCodeType.PriorToDeparture;
        }

        private static TaxType[] GetTaxBreakdown(IEnumerable<Tax> taxes)
        {
            return taxes.Select(tax => new TaxType()
            {
                Amount = new AmountType()
                {
                    CurCode = tax.Amount.BaseCurrency,
                    Value = Convert.ToDecimal(tax.Amount.MonetaryAmount)
                },
                TaxCode = tax.Code,
                DescText = tax.Name,
                CollectionPointTax = tax.CollectionPoints != null ? GetCollectionPointTaxes(tax) : null,
                Country = tax.CountryCode != null ? new()
                {
                    CountryCode = tax.CountryCode
                } : null
            }).ToArray();
        }

        private static CollectionPointTaxType[] GetCollectionPointTaxes(Tax tax)
        {
            return tax.CollectionPoints.Select(collectionPoint =>
            {
                return new CollectionPointTaxType()
                {
                    AirportAmount = new AmountType()
                    {
                        CurCode = collectionPoint.Amount.BaseCurrency,
                        Value = Convert.ToDecimal(collectionPoint.Amount.MonetaryAmount)
                    },
                    Station = new StationType()
                    {
                        IATA_LocationCode = collectionPoint.LocationCode,
                    }
                };
            }).ToArray();
        }
    }
}
