using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Baggage;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Helper;
using VAA.EAPI.NDC.Offer.Infrastructure.Mapper;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.BspMarket;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using Penalty = VAA.EAPI.Domain.Virgin.Models.Common.Penalty;

namespace VAA.EAPI.NDC.Offer.Mapper.OrderQuote
{
    public class OrderQuoteRSMapper
    {
        public static Response<IATA_OrderReshopRS> OrderQuoteHoldBookingRSMapping(OfferResponse offerResponse, bool isCashPaymentAllowed, bool isEasyFopAllowed, BspMarketPaymentData bspMarketPaymentData, bool isNonIataAgent)
        {
            var orderReshopRS = new IATA_OrderReshopRS();
            var paymentFunctions = new List<PaymentFunctionsOfferType>();

            if (isCashPaymentAllowed)
                paymentFunctions.AddRange(GetPaymentFOP());

            if (isEasyFopAllowed)
                paymentFunctions.AddRange(GetEasyFOPPayment());

            if (bspMarketPaymentData.IsCreditCardAccepted)
                paymentFunctions.AddRange(GetPaymentFunctions(bspMarketPaymentData));

            PaymentFunctionsOfferType[] result = paymentFunctions.Any() ? paymentFunctions.ToArray() : null;

            orderReshopRS.Items = new OrderReshopResponseType[1]
            {
                new OrderReshopResponseType
                {
                    Order = new OrderKeyType
                    {
                        OrderID = offerResponse.OrderId,
                        OwnerCode = Constants.Application.OwnerCode
                    },

                    ReshopResults = new ReshopResultsType
                    {
                        Item = new NoChangeIndType()
                        {
                            NoChangeInd = offerResponse.NoChangeId
                        }
                    },
                     PaymentFunctions = result
                }
            };

            return new Response<IATA_OrderReshopRS>()
            {
                Value = orderReshopRS,
                Status = EAPI.Domain.Entities.Common.Status.OK
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

        public static Response<IATA_OrderReshopRS> OrderQuoteRSMapping(bool isHoldBookingReshop, bool isReshopDisruption, OfferResponse offerResponse, OfferResponse offerDocument, IGenericDataMapper genericDataMapper, [Optional] System.Collections.Generic.List<EAPI.Domain.Entities.Common.NDCWarning> nDCWarnings, bool isCashPaymentAllowed, bool isEasyFopAllowed, BspMarketPaymentData bspMarketPaymentData, bool isNonIataAgent)
        {
            var orderReshopRS = new IATA_OrderReshopRS();

            List<BaggageAllowanceType> baggageAllowanceList = new();
            List<DatedMarketingSegmentType> datedMarketingSegmentList = new();
            List<DatedOperatingLegType> datedOperatingLegList = new();
            List<DatedOperatingSegmentType> datedOperatingSegmentList = new();
            List<PaxJourneyType> paxJourneyList = new();
            List<PaxSegmentType> paxSegmentList = new();
            List<PenaltyType> penaltyList = new();
            List<ServiceDefinitionType> servDefinitionList = new();
            List<PriceClassType> priceClassList = new();
            ServiceId serviceIdCount = new();
            var orderItemServices = offerDocument?.Offers?.First().OrderItemServices;

            var paymentFunctions = new List<PaymentFunctionsOfferType>();

            if (offerDocument?.FormOfPayments is not null)
            {
                if (offerDocument.FormOfPayments.Value == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum && isCashPaymentAllowed)
                    paymentFunctions.AddRange(GetPaymentFOP());

                if (offerDocument.FormOfPayments.Value == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay && isEasyFopAllowed)
                    paymentFunctions.AddRange(GetEasyFOPPayment());

                if (offerDocument.FormOfPayments.Value == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum && bspMarketPaymentData.IsCreditCardAccepted)
                    paymentFunctions.AddRange(GetPaymentFunctions(bspMarketPaymentData));
            }

            PaymentFunctionsOfferType[] result = paymentFunctions.Any() ? paymentFunctions.ToArray() : null;

            var paxList = offerResponse.DataList.CustomerDetails.Select(customer =>
            {
                var pax = new PaxType
                {
                    PaxID = customer.CustId,
                    PTC = customer.Ptc == Constants.Application.CNNPTCCode ? Constants.Application.ChildPTCCode : customer.Ptc
                };

                if (!string.IsNullOrEmpty(customer.LinkedInfantId))
                {
                    pax.PaxRefID = customer.LinkedInfantId;
                }

                return pax;
            });

            var originDestList = offerResponse.DataList.OriginDestinationAssociations.Select(onD => new OriginDestType()
            {
                DestCode = onD.OriginDestination.DestinationAirport,
                OriginCode = onD.OriginDestination.OriginAirport,
                OriginDestID = onD.OdLabel,
                PaxJourneyRefID = Array.Empty<string>()
            }).ToList();

            var upsellReshopOffers = new OfferResponse()
            {
                ShoppingResponseId = offerResponse.ShoppingResponseId,
                TotalOffers = offerResponse.TotalOffers,
                Offers = isReshopDisruption ? Enumerable.Empty<OfferDetails>() : offerResponse.Offers.Where(x => x.UpsellOffer == true),
                DataList = offerResponse.DataList
            };

            var reQuotedOffer = new OfferResponse()
            {
                ShoppingResponseId = offerResponse.ShoppingResponseId,
                TotalOffers = offerResponse.TotalOffers,
                Offers = offerResponse.Offers.Where(x => x.Selected && !x.UpsellOffer),
                DataList = offerResponse.DataList
            };

            orderReshopRS.Items = new OrderReshopResponseType[1]
                {
                    new OrderReshopResponseType()
                    {
                        Order = new OrderKeyType
                        {
                            OrderID = offerResponse.OrderId,
                            OwnerCode = Constants.Application.OWNERCODE
                        },
                        ReshopResults = new ReshopResultsType
                        {
                            Item = new ReshopOffersType
                            {
                                RequotedOffer = GetReshopOffers(isHoldBookingReshop,isReshopDisruption,reQuotedOffer, orderItemServices, genericDataMapper,  baggageAllowanceList, datedMarketingSegmentList, datedOperatingLegList, datedOperatingSegmentList, originDestList.ToList(), paxJourneyList, paxSegmentList, penaltyList, priceClassList,servDefinitionList, serviceIdCount,offerDocument).FirstOrDefault(),
                                Offer = GetReshopOffers(isHoldBookingReshop,isReshopDisruption,upsellReshopOffers, orderItemServices, genericDataMapper, baggageAllowanceList, datedMarketingSegmentList, datedOperatingLegList, datedOperatingSegmentList, originDestList.ToList(), paxJourneyList, paxSegmentList, penaltyList, priceClassList,servDefinitionList, serviceIdCount,offerDocument)
                            }
                        },
                        DataLists = new DataListsType
                        {
                            BaggageAllowanceList = baggageAllowanceList.ToArray(),
                            DatedMarketingSegmentList = datedMarketingSegmentList.ToArray(),
                            DatedOperatingLegList = datedOperatingLegList.ToArray(),
                            DatedOperatingSegmentList= datedOperatingSegmentList.ToArray(),
                            OriginDestList = originDestList.ToArray(),
                            PaxJourneyList= paxJourneyList.ToArray(),
                            PenaltyList = offerResponse.DataList.Penalty.Any() ? GetPenaltyList(offerResponse.DataList.Penalty) : null,
                            ServiceDefinitionList=servDefinitionList.ToArray(),
                            PaxList = AssociateInfantV2(paxList).ToArray(),
                            PaxSegmentList  = paxSegmentList.ToArray(),
                            PriceClassList = priceClassList.ToArray(),
                        },
                        Warning = GetWarnings(nDCWarnings),
                        PaymentFunctions = result
                    }
                };

            return new Response<IATA_OrderReshopRS>()
            {
                Value = orderReshopRS,
                Status = EAPI.Domain.Entities.Common.Status.OK
            };
        }

        private static WarningType[] GetWarnings(List<NDCWarning> nDCWarnings)
        {
            return nDCWarnings?.Select(x => new WarningType()
            {
                Code = Convert.ToString(x.Code),
                DescText = x?.ShortText,
                LangCode = Constants.Application.LangCode,
                TypeCode = x?.Type,
                TagText = x?.TagText,
                URL = x?.DocURL?.Href,
            }).ToArray();
        }

        private static PenaltyType[] GetPenaltyList(IEnumerable<Penalty> penalties)
        {
            var penaltyPricelist = penalties.Distinct().Where(x => x.Id.Contains("-CHANGE")).ToList();
            if (penaltyPricelist.Any())
            {
                return penaltyPricelist?.Select(p => new PenaltyType()
                {
                    DescText = [p.Text],
                    PenaltyID = p.Id,
                    TypeCode = Enum.Parse<PenaltyTypeCodeType>(p.Type),
                    Price = new()
                    {
                        TotalAmount = new()
                        {
                            CurCode = p.MaxPrice.Amount.BaseCurrency,
                            Value = Convert.ToDecimal(p.MaxPrice.Amount.MonetaryAmount)
                        }
                    }
                }).ToArray();
            }
            return null;
        }

        private static IEnumerable<PaxType> AssociateInfantV2(IEnumerable<PaxType> paxList)
        {
            var associatedPaxList = paxList;

            if (associatedPaxList.Any(x => x.PTC == Constants.Application.INF))
            {
                var adtPax = associatedPaxList.Where(x => x.PTC == Constants.Application.ADT);
                var infPax = associatedPaxList.Where(x => x.PTC == Constants.Application.INF);

                for (int i = 0; i < infPax.Count(); i++)
                {
                    adtPax.ElementAt(i).PaxRefID = infPax.ElementAt(i).PaxID;
                }
            }

            return associatedPaxList;
        }

        private static ChangedOfferType[] GetReshopOffers(bool isHoldBookingReshop, bool isReshopDisruption, OfferResponse offerResponse, IEnumerable<OrderItemServices> orderItemServices, IGenericDataMapper genericDataMapper, List<BaggageAllowanceType> baggageAllowanceList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<OriginDestType> originDestList, List<PaxJourneyType> paxJourneyList, List<PaxSegmentType> paxSegmentList, List<PenaltyType> penaltyList, List<PriceClassType> priceClassList, List<ServiceDefinitionType> servDefinitionList, ServiceId serviceIdCount, OfferResponse offerDocument)
        {

            return offerResponse.Offers.Select(offer =>
            {
                ChangedOfferType changedOfferType = new()
                {
                    OfferID = offer.OfferId,
                    OwnerCode = Constants.Application.OWNERCODE,
                    AddedOfferItem = GetAddedOfferItem(isHoldBookingReshop, isReshopDisruption, offer, offerResponse, orderItemServices, genericDataMapper, datedMarketingSegmentList, datedOperatingLegList, datedOperatingSegmentList, originDestList, paxJourneyList, paxSegmentList, penaltyList, priceClassList, servDefinitionList, serviceIdCount, offerDocument),
                    JourneyOverview = GetJourneyOverview(offer.OfferItems.First(), offerResponse.DataList, genericDataMapper, paxJourneyList, priceClassList, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, originDestList),
                    BaggageAssociations = GetBaggageAssociations(offer, offerResponse.DataList, genericDataMapper, paxJourneyList, baggageAllowanceList, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, originDestList, priceClassList),
                };

                changedOfferType.OfferExpirationTimeLimitDateTime = DateTime.Parse(offer.OfferExpirationTimeLimit).ToUniversalTime();
                changedOfferType.OfferExpirationTimeLimitDateTimeSpecified = true;

                return changedOfferType;
            }).ToArray();
        }

        private static BaggageAssociationsType[] GetBaggageAssociations(OfferDetails offer, DataList dataList, IGenericDataMapper genericDataMapper, List<PaxJourneyType> paxJourneyList, List<BaggageAllowanceType> baggageAllowanceList, List<PaxSegmentType> paxSegmentList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, List<OriginDestType> originDestList, List<PriceClassType> priceClassList)
        {
            var offerTypeBaggageAllowances = new List<BaggageAssociationsType>();
            var baggageAllowancesDict = dataList.BaggageAllowances.ToDictionary(bag => bag.BaggageAllowanceId);
            var services = offer.OfferItems.SelectMany(offerItem => offerItem.Services).Where(service => service.BaggageRefIds != null && service.BaggageRefIds.Any());

            foreach (var service in services)
            {
                foreach (var bagID in service.BaggageRefIds)
                {
                    if (baggageAllowancesDict.TryGetValue(bagID, out var baggage))
                    {
                        var baggageAllowanceId = GetBaggageAllowanceId(baggage, baggageAllowanceList);
                        var existingBaggageAssociation = offerTypeBaggageAllowances.FirstOrDefault(oBA => oBA.BaggageAllowanceRefID.First() == baggageAllowanceId);

                        if (existingBaggageAssociation == null)
                        {
                            offerTypeBaggageAllowances.Add(new BaggageAssociationsType
                            {
                                BaggageAllowanceRefID = new[] { baggageAllowanceId },
                                PaxRefID = service.ServiceBeneficiaries.ToArray(),
                                OfferFlightAssociations = new OfferFlightAssociationsType
                                {
                                    Item = new PaxJourneyRefType
                                    {
                                        PaxJourneyRefID = new[] { GetPaxJourneyId(service, dataList, genericDataMapper, paxJourneyList, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, originDestList) }
                                    },
                                    ItemElementName = ItemChoiceType1.PaxJourneyRef
                                }
                            });
                        }
                        else
                        {
                            // Get existing journey and Pax lists
                            var existingPaxJourneyIds = (existingBaggageAssociation.OfferFlightAssociations.Item as PaxJourneyRefType)?.PaxJourneyRefID.ToList() ?? new List<string>();
                            var existingPaxIds = existingBaggageAssociation.PaxRefID.ToList();

                            // Add new Pax and Journey IDs
                            existingPaxIds.AddRange(service.ServiceBeneficiaries);
                            existingPaxJourneyIds.Add(GetPaxJourneyId(service, dataList, genericDataMapper, paxJourneyList, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, originDestList));

                            // Update existing baggage association
                            existingBaggageAssociation.OfferFlightAssociations.Item = new PaxJourneyRefType { PaxJourneyRefID = existingPaxJourneyIds.Distinct().ToArray() };
                            existingBaggageAssociation.PaxRefID = existingPaxIds.Distinct().ToArray();
                        }
                    }
                }
            }

            return offerTypeBaggageAllowances.ToArray();
        }

        private static string GetBaggageAllowanceId(BaggageAllowances baggage, List<BaggageAllowanceType> baggageAllowanceList)
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

                var baggageAllowance = new BaggageAllowanceType()
                {
                    BaggageAllowanceID = bagID,
                    TypeCode = baggage.BaggageAllowanceTypeCode,
                    PieceAllowance = new PieceAllowanceType()
                    {
                        TotalQty = Convert.ToDecimal(baggage.FreeBaggageAllowance.AllowedBagCnt)
                    }
                };

                if (baggage.BaggageAllowanceTypeCode == Constants.Application.CarryOnBag && baggage?.FreeBaggageAllowance?.BagDescriptors?.FirstOrDefault()?.BagDescriptorName.Contains(Constants.Application.CarryOnBaggageText) == true)
                {
                    baggageAllowance.DescText = new[] { baggage?.FreeBaggageAllowance?.BagDescriptors?.FirstOrDefault()?.BagDescriptorName };
                }

                if (baggage.FreeBaggageAllowance?.DimensionAllowance != null)
                {
                    baggageAllowance.MaximumDimensionAllowance = baggage.FreeBaggageAllowance?.DimensionAllowance!.Select(da =>
                    new DimensionAllowanceType
                    {
                        LengthUnitOfMeasurement = (Domain.RequestResponseModels.OrderReshopRS.LengthUnitCodeType)da.LengthUnitOfMeasurement,
                        HeightMeasure = Convert.ToDecimal(da.HeightMeasure),
                        WidthMeasure = Convert.ToDecimal(da.WidthMeasure),
                        LengthMeasure = Convert.ToDecimal(da.LengthMeasure),
                        LinearMeasure = Convert.ToDecimal(da.LinearMeasure),
                        HeightMeasureSpecified = true,
                        WidthMeasureSpecified = true,
                        LengthMeasureSpecified = true,
                        LinearMeasureSpecified = true
                    }).ToArray();
                }

                if (baggage.FreeBaggageAllowance?.WeightAllowance != null)
                {
                    List<WeightAllowanceType> weightAllowances = new();
                    var weightAllowanceInKGM = baggage.FreeBaggageAllowance?.WeightAllowance!.FirstOrDefault(wa => wa.WeightUnitOfMeasurement == EAPI.Domain.Virgin.Models.Products.Baggage.WeightUnitCodeType.KGM);
                    var weightAllowanceInLBR = baggage.FreeBaggageAllowance?.WeightAllowance!.FirstOrDefault(wa => wa.WeightUnitOfMeasurement == EAPI.Domain.Virgin.Models.Products.Baggage.WeightUnitCodeType.LBR);

                    if (weightAllowanceInKGM != null)
                    {
                        weightAllowances.Add(new WeightAllowanceType
                        {
                            MaximumWeightMeasure = Convert.ToDecimal(weightAllowanceInKGM.MaximumWeightMeasure),
                            WeightUnitOfMeasurement = (Domain.RequestResponseModels.OrderReshopRS.WeightUnitCodeType)weightAllowanceInKGM.WeightUnitOfMeasurement,
                            MaximumWeightMeasureSpecified = true,
                            TotalMaximumWeightMeasureSpecified = true,
                            TotalMaximumWeightMeasure = baggageAllowance.PieceAllowance.TotalQty * Convert.ToDecimal(weightAllowanceInKGM.MaximumWeightMeasure)
                        });
                    }

                    if (weightAllowanceInLBR != null)
                    {
                        weightAllowances.Add(new WeightAllowanceType
                        {
                            MaximumWeightMeasure = Convert.ToDecimal(weightAllowanceInLBR.MaximumWeightMeasure),
                            WeightUnitOfMeasurement = (Domain.RequestResponseModels.OrderReshopRS.WeightUnitCodeType)weightAllowanceInLBR.WeightUnitOfMeasurement,
                            MaximumWeightMeasureSpecified = true,
                            TotalMaximumWeightMeasureSpecified = true,
                            TotalMaximumWeightMeasure = baggageAllowance.PieceAllowance.TotalQty * Convert.ToDecimal(weightAllowanceInLBR.MaximumWeightMeasure)
                        });
                    }

                    baggageAllowance.WeightAllowance = weightAllowances.ToArray();
                }


                baggageAllowanceList.Add(baggageAllowance);

                return baggageAllowance.BaggageAllowanceID;
            }
        }

        private static BaggageAllowanceType CheckBaggageAllowanceExistance(BaggageAllowances baggage, List<BaggageAllowanceType> baggageAllowanceList)
        {
            if (baggage.FreeBaggageAllowance?.DimensionAllowance != null && baggage.FreeBaggageAllowance?.WeightAllowance != null)
            {

                var existingP2PBagAllowanceType = baggageAllowanceList.FirstOrDefault(bal => bal.TypeCode == baggage.BaggageAllowanceTypeCode
                                                && bal.PieceAllowance?.TotalQty == baggage.FreeBaggageAllowance?.AllowedBagCnt
                                                && bal.MaximumDimensionAllowance?.First().LinearMeasure == Convert.ToDecimal(baggage.FreeBaggageAllowance?.DimensionAllowance?.First().LinearMeasure)
                                                && bal.MaximumDimensionAllowance?.ElementAt(1)?.LinearMeasure.ToString() == baggage.FreeBaggageAllowance?.DimensionAllowance?.ElementAt(1)?.LinearMeasure.ToString()
                                                && bal.WeightAllowance?.FirstOrDefault()?.MaximumWeightMeasure == Convert.ToDecimal(baggage.FreeBaggageAllowance?.WeightAllowance?.FirstOrDefault()?.MaximumWeightMeasure)
                                                && bal.WeightAllowance?.ElementAt(1)?.MaximumWeightMeasure.ToString() == baggage.FreeBaggageAllowance?.WeightAllowance?.ElementAt(1).MaximumWeightMeasure.ToString());


                return existingP2PBagAllowanceType;
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

        private static JourneyOverviewType GetJourneyOverview(OfferItem offerItem, DataList dataList, IGenericDataMapper genericDataMapper, List<PaxJourneyType> paxJourneyList, List<PriceClassType> priceClassList, List<PaxSegmentType> paxSegmentList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, List<OriginDestType> originDestList)
        {
            return new JourneyOverviewType
            {
                JourneyPriceClass = offerItem.Services.Select(service => new JourneyPriceClassType
                {
                    PaxJourneyRefID = GetPaxJourneyId(service, dataList, genericDataMapper, paxJourneyList, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, originDestList),
                    PriceClassRefID = GetPriceClassId(service, dataList, priceClassList)
                }).ToArray()
            };
        }


        private static AddedOfferItemType[] GetAddedOfferItem(bool isHoldBookingReshop, bool isReshopDisruption, OfferDetails offerDetails, OfferResponse offerResponse, IEnumerable<OrderItemServices> orderItemServices, IGenericDataMapper genericDataMapper, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<OriginDestType> originDestList, List<PaxJourneyType> paxJourneyList, List<PaxSegmentType> paxSegmentList, List<PenaltyType> penaltyList, List<PriceClassType> priceClassList, List<ServiceDefinitionType> servDefinitionList, ServiceId serviceIdCount, OfferResponse offerDocument)
        {
            var offerItemList = new List<AddedOfferItemType>();
            bool accountCode = offerResponse.Parties?.Participants?.Any(x => x.AccountCode == "1") ?? false;
            var baseCurrency = offerResponse.DataList.Penalty?.FirstOrDefault()?.MaxPrice?.Amount.BaseCurrency ?? offerDetails.SalePrice?.FirstOrDefault()?.Amount.BaseCurrency;
            var currencyDecimalFormat = DecimalHelper.GetDecimalFormat(offerResponse.DataList.Penalty?.FirstOrDefault()?.MaxPrice.Amount.MonetaryAmount ?? offerDetails.SalePrice.FirstOrDefault()?.Amount.MonetaryAmount);
            var differentialTypeCode = new DifferentialTypeCodeType();
            var ticketingTimeLimit = offerDetails?.TicketingTimeLimit;

            foreach (var offerItem in offerDetails.OfferItems)
            {
                if (isReshopDisruption)
                {
                    differentialTypeCode = DifferentialTypeCodeType.EvenExchange;
                }
                else if (isHoldBookingReshop)
                {
                    differentialTypeCode = DifferentialTypeCodeType.AddCol;
                }
                else if ((offerItem.OfferitemPricing.FirstOrDefault()?.SalePriceDifference.FirstOrDefault(s => s.PriceComponentType == Price.PriceComponentTypes.BaseFareEnum)?.Amount.MonetaryAmount == "0.00") &&
                    offerItem.OfferitemPricing.FirstOrDefault()?.SalePriceDifference.FirstOrDefault(s => s.PriceComponentType == Price.PriceComponentTypes.TotalTaxEnum)?.Amount.MonetaryAmount == "0.00")
                {
                    differentialTypeCode = DifferentialTypeCodeType.EvenExchange;
                }
                else if (offerItem.OfferitemPricing.FirstOrDefault()?.AddCollectPrice?.Amount != null)
                {
                    differentialTypeCode = DifferentialTypeCodeType.AddCol;
                }
                else if (offerItem.OfferitemPricing.FirstOrDefault()?.RefundPrice?.FirstOrDefault().Amount != null)
                {
                    differentialTypeCode = DifferentialTypeCodeType.Refund;
                }
                else
                {
                    differentialTypeCode = DifferentialTypeCodeType.EvenExchange;
                }

                // Get all unique beneficiaries for current offerItem's services
                var offerItemBeneficiaries = offerItem.Services.SelectMany(s => s.ServiceBeneficiaries).Distinct().OrderBy(x => x);

                var matchingOrderItems = orderItemServices?.Where(ois =>
                {
                    var oisBeneficiaries = ois.ServiceBeneficiaries ?? Enumerable.Empty<string>();
                    return oisBeneficiaries.Intersect(offerItemBeneficiaries).Any();
                }) ?? new List<OrderItemServices>();

                // Build ExistingOrderItem list for this AddedOfferItem
                var existingOrderItemList = matchingOrderItems.Select(match => new ExistingOrderItemType
                {
                    OrderItemRefID = match.OrderItemId,
                    ExistingService = GetExistingServiceList(match.DeleteServices, match.RetainServices)
                }).ToArray();


                AddedOfferItemType addedOfferItem = new()
                {
                    OfferItemID = offerItem.OfferItemId,
                    MandatoryInd = true,
                    MandatoryIndSpecified = true,
                    ReshopPrice = new ReshopPriceType()
                    {
                        Item = new PriceDifferentialType()
                        {
                            DifferentialTypeCode = differentialTypeCode,
                            DiffPrice = new DiffPriceAndFareDetailsType()
                            {
                                FareDetail = GetDiffFareDetail(offerItem),
                                Price = GetDiffPrice(offerItem, isHoldBookingReshop, isReshopDisruption)
                            },
                            NewPrice = new PriceAndFareDetailsType()
                            {
                                FareDetail = GetNewPriceFareDetail(offerItem, offerResponse.DataList, genericDataMapper, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, servDefinitionList, priceClassList, baseCurrency, accountCode, currencyDecimalFormat, offerDocument),
                                Price = GetNewFarePrice(offerItem.OfferitemPricing.First().SalePrice, isReshopDisruption)
                            },
                            OldPrice = new PriceAndFareDetailsType()
                            {
                                Price = GetOldFarePrice(offerItem.OfferitemPricing.First().OriginalSalePrice)
                            }
                        }
                    },
                    PaymentTimeLimit = string.IsNullOrEmpty(ticketingTimeLimit) ? null : new PaymentTimeLimitType
                    {
                        Item = new PaymentTimeLimitDateType
                        {
                            PaymentTimeLimitDateTime = Convert.ToDateTime(ticketingTimeLimit).ToUniversalTime(),
                            PaymentTimeLimitDateTimeSpecified = true
                        }
                    },
                    ExistingOrderItem = existingOrderItemList
                };

                // Add Penalty Change Fee RefID
                if (offerItem.OfferitemPricing.First().PenaltyPrices != null && offerItem.OfferitemPricing.First().PenaltyPrices.Count > 0)
                {
                    var penIDs = GetPenaltyRefID(offerItem.OfferitemPricing.First().PenaltyPrices.FirstOrDefault(), penaltyList);

                    addedOfferItem.PenaltyRefID = penIDs;
                    var priceDiff = addedOfferItem.ReshopPrice.Item as PriceDifferentialType;

                    if (priceDiff.DiffPrice.FareDetail?.Length > 0) { priceDiff.DiffPrice.FareDetail.First().PenaltyRefID = penIDs; }
                    else
                    {
                        var paxRefIds = offerItem.Services.FirstOrDefault()?.ServiceBeneficiaries?.ToArray();
                        priceDiff.DiffPrice.FareDetail = new DifferentialFareDetailType[]
                        {
                            new DifferentialFareDetailType
                            {
                                PaxRefID = paxRefIds,
                                PenaltyRefID = penIDs
                        } };
                    }
                }

                // Build Services
                var journeyIds = new List<string>();

                foreach (var offerItemService in offerDetails.OfferItems.First().Services)
                {
                    var journeyID = GetPaxJourneyId(offerItemService, offerResponse.DataList, genericDataMapper, paxJourneyList, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, originDestList);
                    journeyIds.Add(journeyID);
                }

                var service = Enumerable.Empty<OfferedServiceType>();
                service = GetServices(offerItem.Services, journeyIds, serviceIdCount);
                service = service.Concat(GetAmenitiesServiceList(offerItem.Services.First().ServiceBeneficiaries, offerItem.Services, offerResponse, genericDataMapper, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, servDefinitionList, serviceIdCount));

                addedOfferItem.Service = service.ToArray();

                offerItemList.Add(addedOfferItem);
            }

            return offerItemList.ToArray();
        }
        private static ExistingServiceType[] GetExistingServiceList(IEnumerable<string> deleteService, IEnumerable<string> retainedService)
        {
            List<ExistingServiceType> existingServiceType = new();
            var retainedSvc = retainedService?.Distinct();
            var deletedSvc = deleteService?.Distinct();

            existingServiceType.AddRange(retainedSvc?.Select(svc => new ExistingServiceType
            {
                ServiceRefID = svc,
                UnchangedInd = true,
                UnchangedIndSpecified = true
            }) ?? Enumerable.Empty<ExistingServiceType>()
            );

            existingServiceType.AddRange(deletedSvc?.Select(svc => new ExistingServiceType
            {
                ServiceRefID = svc,
                FutureStatusCode = Constants.Application.CancelledOrderItemStatus
            }) ?? Enumerable.Empty<ExistingServiceType>()
            );

            return existingServiceType.ToArray();
        }

        private static IEnumerable<OfferedServiceType> GetAmenitiesServiceList(IEnumerable<string> paxList, IEnumerable<OfferService> serviceOfferings, OfferResponse offerResponse, IGenericDataMapper genericDataMapper, List<PaxSegmentType> paxSegmentList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, List<ServiceDefinitionType> servDefinitionList, ServiceId serviceIdCount)
        {
            List<OfferedServiceType> serviceList = new();

            foreach (var offSegRef in serviceOfferings.SelectMany(ser => ser.ProductSegmentAssociation?.OfferSegmentRefIds ?? Enumerable.Empty<string>()))
            {
                var offerSegment = offerResponse.DataList?.OfferSegments?.FirstOrDefault(offSeg => offSeg.OfferSegmentId == offSegRef);
                var productComponentRefIds = offerSegment?.ProductComponentRefIds;

                if (productComponentRefIds != null && productComponentRefIds.Any() && offerSegment != null)
                {
                    var flightSegment = offerResponse.DataList.FlightSegments.FirstOrDefault(seg => seg.SegmentId == offerSegment.FlightSegmentRefId);
                    if (flightSegment == null)
                        continue;

                    var serviceDefID = GetServiceList(productComponentRefIds, offerResponse, genericDataMapper, servDefinitionList);
                    var segmentID = GetPaxSegmentId(genericDataMapper, offerSegment, flightSegment, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList);

                    var existingService = serviceList.FirstOrDefault(ser =>
                        (ser.OfferServiceAssociation.Item as OfferServiceDefinitionRefType)?.ServiceDefinitionRefID == serviceDefID);

                    if (existingService == null)
                    {
                        OfferedServiceType service = new()
                        {
                            ServiceID = IdCreator.CreateNewServiceID(++serviceIdCount.Id),
                            PaxRefID = paxList.ToArray(),
                            OfferServiceAssociation = GetOfferServiceAssociation(segmentID)
                        };

                        var offerServiceDefRef = service.OfferServiceAssociation.Item as OfferServiceDefinitionRefType;
                        List<string> segmentIDList = new() { segmentID };

                        if (offerServiceDefRef != null)
                        {
                            offerServiceDefRef.ServiceDefinitionRefID = serviceDefID;
                            (offerServiceDefRef.OfferFlightAssociations.Item as PaxSegmentRefListType).PaxSegmentRefID = segmentIDList.ToArray();
                        }

                        serviceList.Add(service);
                    }
                    else
                    {
                        var offerServiceDefRef = existingService.OfferServiceAssociation.Item as OfferServiceDefinitionRefType;
                        var paxSegmentRefList = offerServiceDefRef?.OfferFlightAssociations.Item as PaxSegmentRefListType;

                        if (paxSegmentRefList != null)
                        {
                            // Append new segmentID to existing list without losing previous ones
                            var existingSegmentIDs = paxSegmentRefList.PaxSegmentRefID?.ToList() ?? new List<string>();
                            if (!existingSegmentIDs.Contains(segmentID))
                            {
                                existingSegmentIDs.Add(segmentID);
                                paxSegmentRefList.PaxSegmentRefID = existingSegmentIDs.ToArray();
                            }
                        }
                    }
                }
            }

            return serviceList;
        }

        private static FareDetailType[] GetNewPriceFareDetail(OfferItem reshopOfferItem, DataList dataList, IGenericDataMapper genericDataMapper, List<PaxSegmentType> paxSegmentList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, List<ServiceDefinitionType> servDefinitionList, List<PriceClassType> priceClassList, string baseCurrency, bool accountCode, string currencyDecimalFormat, OfferResponse offerDocument)
        {
            List<FareDetailType> fareDetailTypes = new();
            FareDetailType fareDetailType = new()
            {
                PaxRefID = reshopOfferItem.Services.First().ServiceBeneficiaries.ToArray(),
                FareComponent = GetFareComponent(reshopOfferItem.Services, dataList, genericDataMapper, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, servDefinitionList, priceClassList, baseCurrency, accountCode, currencyDecimalFormat, offerDocument),

            };
            fareDetailTypes.Add(fareDetailType);

            return fareDetailTypes.ToArray();
        }

        private static FareComponentType[] GetFareComponent(IEnumerable<OfferService> offerServiceList, DataList dataList, IGenericDataMapper genericDataMapper, List<PaxSegmentType> paxSegmentList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, List<ServiceDefinitionType> servDefinitionList, List<PriceClassType> priceClassList, string baseCurrency, bool accountCode, string currencyDecimalFormat, OfferResponse offerDocument)
        {
            List<FareComponentType> fareComponents = new();
            foreach (var ond in dataList.OriginDestinationAssociations)
            {
                var services = (from journeyList in dataList.Journey
                                join service in offerServiceList
                                on journeyList.JourneyId equals ond.JourneyRefIds.FirstOrDefault()
                                where service.JourneyRefIds.Contains(journeyList.JourneyId)
                                select service);
                if (services != null && services.Count() > 0)
                {
                    var service = services.FirstOrDefault();
                    var paxSegmentRefIDs = new List<string>();
                    foreach (var serv in services)
                    {
                        paxSegmentRefIDs.AddRange(GetPaxSegmentRefIDs(serv.ProductSegmentAssociation.OfferSegmentRefIds, genericDataMapper, dataList, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList));
                    }
                    fareComponents.Add(new()
                    {
                        CancelRestrictions = GetCancelRestrictionsTypes(service, dataList, baseCurrency, currencyDecimalFormat),
                        ChangeRestrictions = GetChangeRestrictionsTypes(service, dataList, baseCurrency, currencyDecimalFormat),
                        FareBasisCode = dataList.PriceClass.First(x => x.Id == service.PriceClassRefIds.FirstOrDefault()).FareBasisCode,
                        FareTypeCode = offerDocument?.FareTypeRequest?.FareSubTypeCode,
                        NegotiatedCode = offerDocument?.FareTypeRequest?.FareTypeCode,
                        PaxSegmentRefID = paxSegmentRefIDs.ToArray(),
                        PriceClassRefID = GetPriceClassId(service, dataList, priceClassList)
                    });
                }
            }

            return fareComponents.ToArray();
        }

        private static string[] GetJourneyIDs(OfferService service, DataList dataList, IGenericDataMapper genericDataMapper, List<BaggageAllowanceType> baggageAllowanceList, List<PaxJourneyType> paxJourneyList, List<PriceClassType> priceClassList, List<PaxSegmentType> paxSegmentList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, List<OriginDestType> originDestList)
        {
            List<string> journeyIDs = new();
            var journeyID = GetPaxJourneyId(service, dataList, genericDataMapper, paxJourneyList, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, originDestList);
            journeyIDs.Add(journeyID);

            return journeyIDs.ToArray();
        }
        private static OfferedServiceType[] GetServices(IEnumerable<OfferService> services, IEnumerable<string> journeyIds, ServiceId serviceIdCount)
        {
            List<OfferedServiceType> offeredServiceTypes = new();

            if (services.Any())
            {
                OfferedServiceType offeredServiceType = new()
                {
                    OfferServiceAssociation = new OfferServiceAssociationType()
                    {
                        Item = new PaxJourneyRefType()
                        {
                            PaxJourneyRefID = journeyIds.ToArray()
                        }
                    },
                    ServiceID = IdCreator.CreateNewServiceID(++serviceIdCount.Id),
                    PaxRefID = services.First().ServiceBeneficiaries.ToArray(),
                };

                offeredServiceTypes.Add(offeredServiceType);
            }

            return offeredServiceTypes.ToArray();
        }

        private static OfferServiceAssociationType GetOfferServiceAssociation(string segmentID)
        {
            return new()
            {
                Item = new OfferServiceDefinitionRefType()
                {
                    OfferFlightAssociations = new OfferFlightAssociationsType()
                    {
                        ItemElementName = ItemChoiceType1.PaxSegmentReferences,
                        Item = new PaxSegmentRefListType()
                        {
                            PaxSegmentRefID = new string[1]
                            {
                                segmentID
                            }
                        }
                    }
                }
            };
        }

        private static string GetServiceList(IEnumerable<string> productComponentRefIds, OfferResponse offerResponse, IGenericDataMapper genericDataMapper, List<ServiceDefinitionType> servDefinitionList)
        {
            var serviceDefinationID = new List<string>();
            var productComponentList = offerResponse.DataList.ProductComponents.Where(x => productComponentRefIds.Contains(x.Id)).ToList();
            if (servDefinitionList.Any(sd => CompareOrderQuoteDescList(sd.Desc, productComponentList, genericDataMapper)))
            {
                return servDefinitionList.First(sd => CompareOrderQuoteDescList(sd.Desc, productComponentList, genericDataMapper)).ServiceDefinitionID;
            }
            else
            {
                var serviceDefinition = new ServiceDefinitionType()
                {
                    Name = Constants.Application.Amenity,
                    ServiceDefinitionID = IdCreator.CreateServiceDefinitionRefID(servDefinitionList.Count),
                    Desc = GetOrderQuoteDescriptionList(offerResponse.DataList.ProductComponents, productComponentRefIds, genericDataMapper),
                    OwnerCode = Constants.Application.OWNERCODE
                };

                servDefinitionList.Add(serviceDefinition);

                return serviceDefinition.ServiceDefinitionID;
            }
        }

        private static bool CompareOrderQuoteDescList(DescType[] desc, List<ProductComponent> productComponents, IGenericDataMapper genericDataMapper)
        {
            bool found = false;

            List<DescType> descList = new();

            if (desc.Count() == productComponents.Count)
            {
                productComponents.ForEach(comp =>
                {
                    found = desc.Any(desc => desc.DescText == comp.Description);

                    if (!found)
                        descList.Add(new DescType()
                        {
                            DescText = comp.Description
                        });
                });

                if (descList.Count > 0)
                    found = false;
                else
                    found = true;
            }

            return found;
        }

        private static DescType[] GetOrderQuoteDescriptionList(IEnumerable<ProductComponent> productComponents, IEnumerable<string> productComponentRefIds, IGenericDataMapper genericDataMapper)
        {
            List<DescType> descriptionList = new();
            var productComponentList = productComponents.Where(x => productComponentRefIds.Contains(x.Id)).ToList();
            productComponentList.ForEach(comp =>
            {
                DescType desc = new()
                {
                    DescText = comp.Description
                };
                descriptionList.Add(desc);
            });

            return descriptionList.ToArray();
        }

        private static string[] GetPenaltyRefID(PenaltyPrice penaltyPrices, List<PenaltyType> penaltyList)
        {
            List<string> penIDs = new();

            if (penaltyList.Any(x => x.Price.TotalAmount.Value == Convert.ToDecimal(penaltyPrices.Amount.MonetaryAmount) && x.Price.TotalAmount.CurCode == penaltyPrices.Amount.BaseCurrency))
            {
                var penaltyId = penaltyList.Where(x => x.Price.TotalAmount.Value == Convert.ToDecimal(penaltyPrices.Amount.MonetaryAmount) && x.Price.TotalAmount.CurCode == penaltyPrices.Amount.BaseCurrency).Select(x => x.PenaltyID).First();
                penIDs.Add(penaltyId);
                return penIDs.ToArray();
            }
            else
            {
                var penaltyType = CreateReshopPenalty(penaltyPrices, penaltyList);
                penaltyList.Add(penaltyType);

                penIDs.Add(penaltyType.PenaltyID);
                return penIDs.ToArray();
            }

        }

        private static PenaltyType CreateReshopPenalty(PenaltyPrice penaltyPrices, List<PenaltyType> penaltyList)
        {
            var penaltyType = new PenaltyType()
            {
                PenaltyID = IdCreator.CreateReshopPenaltyID(penaltyList.Count),
                DescText = new string[1] { "Change Penalty" },
                TypeCode = PenaltyTypeCodeType.Change,
                Price = new PenaltyPriceType()
                {
                    TotalAmount = new AmountType()
                    {
                        Value = Convert.ToDecimal(penaltyPrices.Amount.MonetaryAmount),
                        CurCode = penaltyPrices.Amount.BaseCurrency
                    }
                }

            };
            return penaltyType;
        }

        private static string GetPaxJourneyId(OfferService offerService, DataList dataList, IGenericDataMapper genericDataMapper, List<PaxJourneyType> paxJourneyList, List<PaxSegmentType> paxSegmentList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, List<OriginDestType> originDestList)
        {
            var duration = dataList.Journey.First(j => j.JourneyId == offerService.JourneyRefIds.FirstOrDefault()).Duration;

            TimeSpan journeyDuration = TimeSpan.Parse(duration, CultureInfo.InvariantCulture);

            var segments = new List<string>();
            var segmentRefs = new List<string>();

            foreach (var oSegID in offerService.ProductSegmentAssociation.OfferSegmentRefIds)
            {
                var offerSeg = dataList.OfferSegments.First(seg => seg.OfferSegmentId == oSegID);
                segments.Add(GetPaxSegmentId(genericDataMapper, offerSeg, dataList.FlightSegments.First(seg => seg.SegmentId == offerSeg.FlightSegmentRefId), paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList));
            }

            var paxJourneyId = GetPaxJourneyId(journeyDuration, segments, paxJourneyList);

            AddJourneyReftoODList(paxJourneyId, offerService.JourneyRefIds, dataList, originDestList);

            return paxJourneyId;
        }

        private static void AddJourneyReftoODList(string paxJourneyId, IEnumerable<string> journeyRefIds, DataList dataList, List<OriginDestType> originDestList)
        {
            var oDRef = dataList.OriginDestinationAssociations.First(od => od.JourneyRefIds.Contains(journeyRefIds.FirstOrDefault())).OdLabel;

            foreach (var ond in originDestList)
            {
                var paxJourneyrefIds = ond.PaxJourneyRefID.ToList();
                if (ond.OriginDestID == oDRef && !ond.PaxJourneyRefID.Any(jour => jour == paxJourneyId))
                {
                    paxJourneyrefIds.Add(paxJourneyId);
                    ond.PaxJourneyRefID = paxJourneyrefIds.ToArray();
                }
            }
        }


        private static string GetPaxJourneyId(TimeSpan journeyDuration, List<string> segments, List<PaxJourneyType> paxJourneyList)
        {
            if (paxJourneyList.Any(x => x.PaxSegmentRefID.SequenceEqual(segments)))
            {
                return paxJourneyList.First(x => x.PaxSegmentRefID.SequenceEqual(segments)).PaxJourneyID;
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

        private static PriceType GetNewFarePrice(List<Price> salePrice, bool isReshopDisruption)
        {
            PriceType priceType = new()
            {
                BaseAmount = new AmountType()
                {
                    CurCode = salePrice.First(s => s.PriceComponentType == Price.PriceComponentTypes.BaseFareEnum).Amount.BaseCurrency,
                    Value = isReshopDisruption
                        ? 0.00m
                        : Convert.ToDecimal(salePrice.First(s => s.PriceComponentType == Price.PriceComponentTypes.BaseFareEnum).Amount.MonetaryAmount)
                },
                TotalAmount = new AmountType()
                {
                    CurCode = salePrice.First(s => s.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum).Amount.BaseCurrency,
                    Value = isReshopDisruption ? 0.00m: Convert.ToDecimal(salePrice.First(s => s.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount)
                },
                TaxSummary = GetNewPriceTaxSummary(salePrice, isReshopDisruption)
            };

            return priceType;
        }

        private static TaxSummaryType[] GetNewPriceTaxSummary(List<Price> salePrice, bool isReshopDisruption)
        {
            return salePrice.Where(x => x.PriceComponentType == Price.PriceComponentTypes.TotalTaxEnum).Select(totalTax =>
                new TaxSummaryType()
                {
                    TotalTaxAmount = new AmountType()
                    {
                        CurCode = totalTax.Amount.BaseCurrency,
                        Value = isReshopDisruption ? 0.00m : Convert.ToDecimal(totalTax.Amount.MonetaryAmount)
                    },
                    Tax = GetTaxBreakdown(totalTax.TaxBreakup, isReshopDisruption),
                    TotalRefundableTaxAmount = isReshopDisruption
                        ? new AmountType
                        {
                            CurCode = totalTax.Amount.BaseCurrency,
                            Value = 0.00m
                        }
                        : null
                }).ToArray();
        }

        private static TaxType[] GetTaxBreakdown(IEnumerable<Tax> taxes, bool isReshopDisruption = false)
        {
            return taxes?.Select(tax => new TaxType()
            {
                Amount = new AmountType()
                {
                    CurCode = tax.Amount.BaseCurrency,
                    Value = isReshopDisruption ? 0.00m : Convert.ToDecimal(tax.Amount?.MonetaryAmount)
                },
                TaxCode = tax?.Code,
                DescText = tax?.Name,
                CollectionPointTax = tax?.CollectionPoints != null ? GetCollectionPointTaxes(tax) : null,
                Country = tax?.CountryCode != null ? new CountryType()
                {
                    CountryCode = tax?.CountryCode
                } : null

            }).ToArray();
        }

        private static CollectionPointTaxType[] GetCollectionPointTaxes(Tax tax)
        {
            var collectionPointTax = new List<CollectionPointTaxType>();
            tax.CollectionPoints.ForEach(collectionPoint =>
            {
                var collectiontax = new CollectionPointTaxType()
                {
                    AirportAmount = new AmountType()
                    {
                        CurCode = collectionPoint?.Amount?.BaseCurrency,
                        Value = Convert.ToDecimal(collectionPoint?.Amount?.MonetaryAmount)
                    },
                    Station = new StationType()
                    {
                        IATA_LocationCode = collectionPoint?.LocationCode,
                    }
                };
                collectionPointTax.Add(collectiontax);
            });

            return collectionPointTax.ToArray();
        }

        private static string GetPriceClassId(OfferService offerService, DataList dataList, List<PriceClassType> priceClassList)
        {
            var priceClass = dataList.PriceClass.FirstOrDefault(p => p.Id == offerService.PriceClassRefIds.FirstOrDefault());

            if (priceClassList.Any(x => x.Name == priceClass.ProductName))
            {
                var priceClassID = priceClassList.Where(x => x.Name == priceClass.ProductName).Select(x => x.PriceClassID).First();
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

        private static DifferentialFareDetailType[] GetDiffFareDetail(OfferItem offerItem)
        {
            List<DifferentialFareDetailType> differentialFareDetailTypes = new();

            var penaltyRefID = offerItem.Services.First().PenaltyRef?.ChangePenaltyRefIds?.Where(x => x.Contains("-CHANGE"))?.ToArray();

            if (penaltyRefID.IsNullOrEmpty())
                return differentialFareDetailTypes.ToArray();

            DifferentialFareDetailType differentialFareDetailType = new()
            {
                PaxRefID = offerItem.Services.First().ServiceBeneficiaries.ToArray(),
                PenaltyRefID = penaltyRefID
            };

            differentialFareDetailTypes.Add(differentialFareDetailType);

            return differentialFareDetailTypes.ToArray();
        }


        private static DifferentialPriceType GetDiffPrice(OfferItem offerItem, bool isHoldBookingReshop, bool isReshopDisruption)
        {
            var pricing = offerItem.OfferitemPricing.First();

            var oldTaxes = offerItem.OfferitemPricing.FirstOrDefault()?.OriginalSalePrice.FirstOrDefault(x => x.PriceComponentType == Price.PriceComponentTypes.TotalTaxEnum)?.TaxBreakup;
            var newTaxes = offerItem.OfferitemPricing.FirstOrDefault()?.SalePrice.FirstOrDefault(x => x.PriceComponentType == Price.PriceComponentTypes.TotalTaxEnum)?.TaxBreakup;

            var diffPriceTaxes = GetDiffPriceTaxBreakup(newTaxes, oldTaxes);


            var price = new DifferentialPriceType()
            {
                BaseAmount = new AmountType()
                {
                    CurCode = offerItem.OfferitemPricing.First().SalePriceDifference.First(d => d.PriceComponentType == Price.PriceComponentTypes.BaseFareEnum).Amount.BaseCurrency,
                    Value = Convert.ToDecimal(offerItem.OfferitemPricing.First().SalePriceDifference.First(d => d.PriceComponentType == Price.PriceComponentTypes.BaseFareEnum).Amount.MonetaryAmount),
                },
                TaxSummary = GetTaxSummary(offerItem.OfferitemPricing.First().SalePriceDifference, diffPriceTaxes, isReshopDisruption)
            };

            price.TotalAmount = new AmountType()
            {
                CurCode = price.BaseAmount.CurCode,
                Value = isReshopDisruption ? 0.00m: price.BaseAmount.Value + price.TaxSummary.First().TotalTaxAmount.Value
            };


            if (isReshopDisruption)
            {
                price.BaseAmount.Value = 0.00m;
                price.DueByAirlineAmount = new AmountType()
                {
                    CurCode = price.BaseAmount.CurCode,
                    Value = 0.00m
                };
                price.DueToAirlineAmount = new AmountType()
                {
                    CurCode = price.BaseAmount.CurCode,
                    Value = 0.00m
                };
            }
            else if (isHoldBookingReshop)
            {
                price.DueByAirlineAmount = new AmountType()
                {
                    CurCode = price.BaseAmount.CurCode,
                    Value = 0.00m
                };

                price.DueToAirlineAmount = new AmountType()
                {
                    CurCode = price.BaseAmount.CurCode,
                    Value = Convert.ToDecimal(offerItem.OfferitemPricing.FirstOrDefault()?.SalePrice?.FirstOrDefault(s => s.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum)?.Amount.MonetaryAmount)
                };
            }

            else
            {
                price.DueByAirlineAmount = new AmountType()
                {
                    CurCode = price.BaseAmount.CurCode,
                    Value = offerItem.OfferitemPricing.FirstOrDefault()?.RefundPrice != null ? Convert.ToDecimal(offerItem.OfferitemPricing.FirstOrDefault()?.RefundPrice.FirstOrDefault()?.Amount.MonetaryAmount) : 0.00m
                };
                price.DueToAirlineAmount = new AmountType()
                {
                    CurCode = price.BaseAmount.CurCode,
                    Value = offerItem.OfferitemPricing.FirstOrDefault()?.AddCollectPrice != null ? Convert.ToDecimal(offerItem.OfferitemPricing.FirstOrDefault()?.AddCollectPrice?.Amount.MonetaryAmount) : 0.00m

                };
            }

            return price;
        }

        private static IEnumerable<Tax> GetDiffPriceTaxBreakup(IEnumerable<Tax> newTaxes, IEnumerable<Tax> oldTaxes)
        {
            var diffTaxes = new List<Tax>();
            var remainingOldTaxes = oldTaxes.ToList();
            foreach (var tax in newTaxes)
            {
                var oldTax = remainingOldTaxes.FirstOrDefault(o => o.Code == tax.Code);
                diffTaxes.Add(new Tax
                {
                    Code = tax.Code,
                    Amount = new CurrencyAmount
                    {
                        BaseCurrency = tax.Amount.BaseCurrency,
                        MonetaryAmount = (Convert.ToDecimal(tax.Amount.MonetaryAmount) - Convert.ToDecimal(oldTax?.Amount.MonetaryAmount ?? "0")).ToString()
                    },
                    Name = tax?.Name,
                    CountryCode = tax?.CountryCode
                });
                if (oldTax != null)
                    remainingOldTaxes.Remove(oldTax);
            }
            foreach (var oldTax in remainingOldTaxes)
            {
                diffTaxes.Add(new Tax
                {
                    Code = oldTax.Code,
                    Amount = new CurrencyAmount
                    {
                        BaseCurrency = oldTax.Amount.BaseCurrency,
                        MonetaryAmount = (-Convert.ToDecimal(oldTax.Amount.MonetaryAmount)).ToString()
                    },
                    Name = oldTax?.Name,
                    CountryCode = oldTax?.CountryCode
                });
            }
            return diffTaxes;
        }

        private static TaxSummaryType[] GetTaxSummary(List<Price> salePriceDifference, IEnumerable<Tax> taxes, bool isReshopDisruption)
        {
            return salePriceDifference.Where(x => x.PriceComponentType == Price.PriceComponentTypes.TotalTaxEnum).Select(totalTax => new TaxSummaryType()
            {
                TotalTaxAmount = new AmountType()
                {
                    CurCode = totalTax.Amount.BaseCurrency,
                    Value = isReshopDisruption ? 0.00m : Convert.ToDecimal(totalTax.Amount.MonetaryAmount)
                },
                Tax = GetTaxBreakdown(taxes, isReshopDisruption)
            }).ToArray();
        }

        private static FareDetailType[] GetOldPriceFareDetail(OfferItem offerItem)
        {
            return new FareDetailType[]{new()
            {
                PaxRefID = offerItem.Services.First().ServiceBeneficiaries.ToArray()
            } };
        }

        private static PriceType GetOldFarePrice(List<Price> originalSalePrice)
        {
            return new PriceType()
            {
                BaseAmount = new AmountType()
                {
                    CurCode = originalSalePrice.First(s => s.PriceComponentType == Price.PriceComponentTypes.BaseFareEnum)?.Amount.BaseCurrency,
                    Value = Convert.ToDecimal(originalSalePrice.First(s => s.PriceComponentType == Price.PriceComponentTypes.BaseFareEnum)?.Amount.MonetaryAmount)
                },
                TotalAmount = new AmountType()
                {
                    CurCode = originalSalePrice.First(s => s.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum)?.Amount.BaseCurrency,
                    Value = Convert.ToDecimal(originalSalePrice.First(s => s.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum)?.Amount.MonetaryAmount)
                },
                TaxSummary = GetOldPriceTaxSummary(originalSalePrice)
            };
        }

        private static TaxSummaryType[] GetOldPriceTaxSummary(List<Price> originalSalePrice)
        {
            return originalSalePrice.Where(x => x.PriceComponentType == Price.PriceComponentTypes.TotalTaxEnum).Select(totalTax =>
                new TaxSummaryType()
                {
                    TotalTaxAmount = new AmountType()
                    {
                        CurCode = totalTax.Amount.BaseCurrency,
                        Value = Convert.ToDecimal(totalTax.Amount.MonetaryAmount)
                    },
                    Tax = GetTaxBreakdown(totalTax.TaxBreakup)
                }).ToArray();
        }
        private static string[] GetPaxSegmentRefIDs(IEnumerable<string> offerSegmentRefIds, IGenericDataMapper genericDataMapper, DataList dataList, List<PaxSegmentType> PaxSegmentList, List<DatedMarketingSegmentType> DatedMarketingSegmentList, List<DatedOperatingSegmentType> DatedOperatingSegmentList, List<DatedOperatingLegType> DatedOperatingLegList)
        {
            List<string> paxSegIDs = new();
            foreach (var oSegID in offerSegmentRefIds)
            {
                var offerSeg = dataList.OfferSegments.First(seg => seg.OfferSegmentId == oSegID);
                paxSegIDs.Add(GetPaxSegmentId(genericDataMapper, offerSeg, dataList.FlightSegments.First(seg => seg.SegmentId == offerSeg.FlightSegmentRefId), PaxSegmentList, DatedMarketingSegmentList, DatedOperatingSegmentList, DatedOperatingLegList));
            }
            return paxSegIDs.ToArray();
        }

        private static string GetPaxSegmentId(IGenericDataMapper genericDataMapper, OfferSegment offerFlightSegment, Segment flightSegment, List<PaxSegmentType> paxSegmentList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList)
        {
            var datedMarketingSegmentId = CreateDatedMarketingSegmentType(flightSegment, genericDataMapper, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList);

            if (paxSegmentList != null && paxSegmentList.Any(seg => seg.MarketingCarrierRBD_Code == offerFlightSegment.ClassOfService && (seg.CabinTypeAssociationChoice?.Item as CabinTypeType)?.CabinTypeName == offerFlightSegment.CabinType.CabinName && (seg.CabinTypeAssociationChoice?.Item as CabinTypeType)?.CabinTypeCode == offerFlightSegment.CabinType.CabinCode && seg.DatedMarketingSegmentRefId == datedMarketingSegmentId))
            {
                return paxSegmentList.First(seg => seg.MarketingCarrierRBD_Code == offerFlightSegment.ClassOfService && (seg.CabinTypeAssociationChoice?.Item as CabinTypeType)?.CabinTypeName == offerFlightSegment.CabinType.CabinName && (seg.CabinTypeAssociationChoice?.Item as CabinTypeType)?.CabinTypeCode == offerFlightSegment.CabinType.CabinCode && seg.DatedMarketingSegmentRefId == datedMarketingSegmentId)?.PaxSegmentID;
            }
            else
            {
                var paxSegmentType = CreatePaxSegmentType(genericDataMapper, offerFlightSegment, flightSegment, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList);
                paxSegmentList!.Add(paxSegmentType);
                return paxSegmentType.PaxSegmentID;
            }
        }

        private static PaxSegmentType CreatePaxSegmentType(IGenericDataMapper genericDataMapper, OfferSegment offerFlightSegment, Segment flightSegment, List<PaxSegmentType> paxSegmentList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList)
        {
            return new PaxSegmentType()
            {
                PaxSegmentID = IdGenerator.CreatePaxSegmentID(paxSegmentList.Count),
                CabinTypeAssociationChoice = new CabinTypeAssociationChoiceType()
                {
                    Item = new CabinTypeType()
                    {
                        CabinTypeCode = offerFlightSegment?.CabinType?.CabinCode,
                        CabinTypeName = offerFlightSegment?.CabinType?.CabinName
                    }
                },
                MarketingCarrierRBD_Code = offerFlightSegment!.ClassOfService,
                DatedMarketingSegmentRefId = CreateDatedMarketingSegmentType(flightSegment, genericDataMapper, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList)
            };
        }

        private static string CreateDatedMarketingSegmentType(Segment flightSegment, IGenericDataMapper genericDataMapper, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList)
        {
            var scheduledArrivalTime = Convert.ToDateTime(flightSegment?.ScheduledArrivalTime.LocalDateTime);
            var scheduledDepartureTime = Convert.ToDateTime(flightSegment.ScheduledDepartureTime.LocalDateTime);

            if (datedMarketingSegmentList.Any(dMS => dMS.CarrierDesigCode == flightSegment.MarketingCarrier
                                                     && dMS.CarrierName == genericDataMapper.GetOperatingCarrierNameByCode(flightSegment.MarketingCarrier)
                                                     && dMS.Arrival.AircraftScheduledDateTime == scheduledArrivalTime
                                                     && dMS.Dep.AircraftScheduledDateTime == scheduledDepartureTime
                                                     && dMS.MarketingCarrierFlightNumberText == flightSegment.MarketingFlightNumber))
            {
                return datedMarketingSegmentList.First(dMS => dMS.CarrierDesigCode == flightSegment.MarketingCarrier
                                                              && dMS.CarrierName == genericDataMapper.GetOperatingCarrierNameByCode(flightSegment.MarketingCarrier)
                                                              && dMS.Arrival.AircraftScheduledDateTime == scheduledArrivalTime
                                                              && dMS.Dep.AircraftScheduledDateTime == scheduledDepartureTime
                                                              && dMS.MarketingCarrierFlightNumberText == flightSegment.MarketingFlightNumber).DatedMarketingSegmentId;
            }
            else
            {
                var datedMarketingSegmentType = new DatedMarketingSegmentType
                {
                    Arrival = new TransportPointType
                    {
                        IATA_LocationCode = flightSegment.DestinationAirportCode,
                        AircraftScheduledDateTime = scheduledArrivalTime,
                        AircraftScheduledDateTimeSpecified = true,
                        TerminalName = flightSegment.Legs.First().DestinationTerminal
                    }
                };

                var transportDepType = new TransportPointType
                {
                    IATA_LocationCode = flightSegment.OriginAirportCode,
                    AircraftScheduledDateTime = scheduledDepartureTime,
                    AircraftScheduledDateTimeSpecified = true,
                    TerminalName = flightSegment.Legs.First().DestinationTerminal
                };

                datedMarketingSegmentType.Dep = transportDepType;
                datedMarketingSegmentType.CarrierDesigCode = flightSegment.MarketingCarrier;
                datedMarketingSegmentType.CarrierName = genericDataMapper.GetOperatingCarrierNameByCode(flightSegment.MarketingCarrier);
                datedMarketingSegmentType.MarketingCarrierFlightNumberText = flightSegment.MarketingFlightNumber;
                datedMarketingSegmentType.DatedMarketingSegmentId = IdGenerator.CreateDatedMarketingSegmentID(datedMarketingSegmentList.Count);
                datedMarketingSegmentType.DatedOperatingSegmentRefId = GetDatedOperatingSegmentID(flightSegment, datedOperatingSegmentList.Count, genericDataMapper, datedOperatingSegmentList, datedOperatingLegList);

                datedMarketingSegmentList.Add(datedMarketingSegmentType);

                return datedMarketingSegmentType.DatedMarketingSegmentId;
            }
        }

        private static string GetDatedOperatingSegmentID(Segment flightSegment, int count, IGenericDataMapper genericDataMapper, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList)
        {
            var datedOperatingSegment = CreateDatedOperatingSegment(flightSegment, count, genericDataMapper, datedOperatingLegList);
            datedOperatingSegmentList.Add(datedOperatingSegment);
            return datedOperatingSegment.DatedOperatingSegmentId;
        }

        private static DatedOperatingSegmentType CreateDatedOperatingSegment(Segment flightSegment, int count, IGenericDataMapper genericDataMapper, List<DatedOperatingLegType> datedOperatingLegList)
        {
            var datedOperatingSegmentType = new DatedOperatingSegmentType();

            datedOperatingSegmentType.DatedOperatingSegmentId = IdGenerator.CreateDatedOperatingSegmentID(count);
            datedOperatingSegmentType.CarrierName = string.IsNullOrEmpty(flightSegment.OperatingCarrier) ? genericDataMapper.GetOperatingCarrierNameByCode(flightSegment.MarketingCarrier) : genericDataMapper.GetOperatingCarrierNameByCode(flightSegment.OperatingCarrier);
            datedOperatingSegmentType.CarrierDesigCode = string.IsNullOrEmpty(flightSegment.OperatingCarrier) ? flightSegment.MarketingCarrier : flightSegment.OperatingCarrier;
            datedOperatingSegmentType.OperatingCarrierFlightNumberText = string.IsNullOrEmpty(flightSegment.OperatingFlightNumber) ? flightSegment.MarketingFlightNumber : flightSegment.OperatingFlightNumber;

            TimeSpan time = TimeSpan.Parse(flightSegment.FlightDuration);
            datedOperatingSegmentType.Duration = GetDurationV2(time);

            List<string> datedOperatingLegRefId = new List<string>();

            if (flightSegment.Legs != null)
            {
                foreach (var legRef in flightSegment.Legs)
                {
                    datedOperatingLegRefId.Add(CreateDatedOperatingLegType(flightSegment.AirCraft, legRef, datedOperatingLegList));
                }
            }
            datedOperatingSegmentType.DatedOperatingLegRefID = datedOperatingLegRefId.ToArray();

            return datedOperatingSegmentType;
        }

        private static string CreateDatedOperatingLegType(string aircraftTypeCode, Legs leg, List<DatedOperatingLegType> datedOperatingLegList)
        {
            var datedOperatingLeg = new DatedOperatingLegType()
            {
                Arrival = new TransportPointType
                {
                    IATA_LocationCode = leg.DestinationAirportCode,
                    AircraftScheduledDateTime = Convert.ToDateTime(leg.ScheduledArrivalTime.LocalDateTime),
                    AircraftScheduledDateTimeSpecified = true,
                    TerminalName = leg.DestinationTerminal
                },
                CarrierAircraftType = string.IsNullOrEmpty(leg.CarrierAircraftTypeCode) ? null : new CarrierAircraftTypeType()
                {
                    CarrierAircraftTypeCode = leg.CarrierAircraftTypeCode,
                    CarrierAircraftTypeName = leg.CarrierAircraftTypeName,
                },
                Dep = new TransportPointType
                {
                    IATA_LocationCode = leg.OriginAirportCode,
                    AircraftScheduledDateTime = Convert.ToDateTime(leg.ScheduledDepartureTime.LocalDateTime),
                    AircraftScheduledDateTimeSpecified = true,
                    TerminalName = leg.OriginTerminal
                },
                IATA_AircraftType = new IATA_AircraftTypeType
                {
                    IATA_AircraftTypeCode = aircraftTypeCode
                },
                DatedOperatingLegID = IdCreator.CreateDatedOperatingLegID(datedOperatingLegList.Count),
                OnGroundDuration = !string.IsNullOrEmpty(leg.LegDuration) ? GetDurationV2(TimeSpan.FromMinutes(Convert.ToDouble(leg.LegDuration))) : null
            };
            datedOperatingLegList.Add(datedOperatingLeg);
            return datedOperatingLeg.DatedOperatingLegID;
        }

        private static string GetDurationV2(TimeSpan timeSpan)
        {
            return $"{Constants.Application.PT}{(timeSpan.Days * 24) + timeSpan.Hours}{Constants.Application.H}{timeSpan.Minutes}{Constants.Application.M}{timeSpan.Seconds}{Constants.Application.S}";
        }

        private static CancelRestrictionsType[] GetCancelRestrictionsTypes(OfferService offerService, DataList dataList, string baseCurrency, string currencyDecimalFormat)
        {

            var cancelRestrictions = new List<CancelRestrictionsType>();
            var penalty = new List<Penalty>();

            offerService?.PenaltyRef?.CancellationPenaltyRefIds?.ForEach(x => penalty.Add(dataList.Penalty.FirstOrDefault(p => p.Id == x)));
            if (penalty.Count > 0)
            {
                var groupedPenalties = penalty.GroupBy(x => x.Text);
                foreach (var pen in groupedPenalties)
                {
                    cancelRestrictions.AddRange(GetCancelRestrictions(pen.First(), baseCurrency, currencyDecimalFormat));
                }
            }

            return cancelRestrictions.ToArray();
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
                        return new List<CancelRestrictionsType>
                        {
                            new CancelRestrictionsType
                            {
                                AllowedModificationInd = true,
                                AllowedModificationIndSpecified = true,
                                JourneyStageCode = journeyStageCodeText == Constants.Application.AfterDeparture ? JourneyStageTypeCodeType.AfterDeparture : JourneyStageTypeCodeType.PriorToDeparture,
                                JourneyStageCodeSpecified = true,
                                Fee = new ChangeOrCancelFeeType { Amount = new AmountType { CurCode = baseCurrency, Value = Convert.ToDecimal(string.Format(currencyDecimalFormat, 0)) } }
                            }
                        };
                    case Constants.Application.NonRefundable:
                    default:
                        return new List<CancelRestrictionsType>
                        {
                            new CancelRestrictionsType
                            {
                                AllowedModificationInd = false,
                                AllowedModificationIndSpecified = true,
                                JourneyStageCode = journeyStageCodeText == Constants.Application.AfterDeparture ? JourneyStageTypeCodeType.AfterDeparture : JourneyStageTypeCodeType.PriorToDeparture,
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
                        return new List<CancelRestrictionsType>
                        {
                            new CancelRestrictionsType
                            {
                                AllowedModificationInd = true,
                                AllowedModificationIndSpecified = true,
                                JourneyStageCode =  JourneyStageTypeCodeType.PriorToDeparture,
                                JourneyStageCodeSpecified = true,
                                Fee = new ChangeOrCancelFeeType { Amount = new AmountType { CurCode = baseCurrency, Value = Convert.ToDecimal(string.Format(currencyDecimalFormat, 0)) } }
                            },
                            new CancelRestrictionsType
                            {
                                AllowedModificationInd = true,
                                AllowedModificationIndSpecified = true,
                                JourneyStageCode = JourneyStageTypeCodeType.AfterDeparture,
                                JourneyStageCodeSpecified = true,
                                Fee = new ChangeOrCancelFeeType { Amount = new AmountType { CurCode = baseCurrency, Value = Convert.ToDecimal(string.Format(currencyDecimalFormat, 0)) } }
                            }
                        };
                    case Constants.Application.RefundFee:
                        return new List<CancelRestrictionsType>
                        {
                            new CancelRestrictionsType
                            {
                                AllowedModificationInd = true,
                                AllowedModificationIndSpecified = true,
                                JourneyStageCode =  JourneyStageTypeCodeType.PriorToDeparture,
                                JourneyStageCodeSpecified = true,
                                Fee = new ChangeOrCancelFeeType { Amount = new AmountType { Value = penalty.MaxPrice.Amount.MonetaryAmount is null ? Convert.ToDecimal(string.Format(currencyDecimalFormat, 0)) : Convert.ToDecimal(penalty.MaxPrice.Amount.MonetaryAmount), CurCode = baseCurrency  } }
                            },
                            new CancelRestrictionsType
                            {
                                AllowedModificationInd = true,
                                AllowedModificationIndSpecified = true,
                                JourneyStageCode = JourneyStageTypeCodeType.AfterDeparture ,
                                JourneyStageCodeSpecified = true,
                                Fee = new ChangeOrCancelFeeType { Amount = new AmountType { Value = penalty.MaxPrice.Amount.MonetaryAmount is null ? Convert.ToDecimal(string.Format(currencyDecimalFormat, 0)) : Convert.ToDecimal( penalty.MaxPrice.Amount.MonetaryAmount), CurCode = baseCurrency  } }
                            }
                        };
                    default:
                        return new List<CancelRestrictionsType>
                        {
                            new CancelRestrictionsType
                            {
                                AllowedModificationInd = true,
                                AllowedModificationIndSpecified = true,
                                JourneyStageCode = string.Equals(journeyStageCodeText.Replace("-", " "), Constants.Application.BeforeDeparture, StringComparison.CurrentCultureIgnoreCase) ? JourneyStageTypeCodeType.PriorToDeparture : JourneyStageTypeCodeType.AfterDeparture,
                                JourneyStageCodeSpecified = true,
                                Fee = new ChangeOrCancelFeeType { Amount = new AmountType { Value = penalty.MaxPrice.Amount.MonetaryAmount is null ? Convert.ToDecimal(string.Format(currencyDecimalFormat, 0)) : Convert.ToDecimal(penalty.MaxPrice.Amount.MonetaryAmount), CurCode = baseCurrency  } }
                            }
                        };
                }
            }
        }

        private static ChangeRestrictionsType[] GetChangeRestrictionsTypes(OfferService offerService, DataList dataList, string baseCurrency, string currencyDecimalFormat)
        {
            var changeRestrictions = new List<ChangeRestrictionsType>();
            var penalty = new List<Penalty>();

            offerService?.PenaltyRef?.ChangePenaltyRefIds?.ForEach(penRefId =>
            penalty.Add(dataList.Penalty.First(pen => pen.Id == penRefId))
            );

            if (penalty.Count() > 0)
            {
                var groupedPenalties = penalty.GroupBy(x => x.Text);
                foreach (var pen in groupedPenalties)
                {
                    changeRestrictions.AddRange(GetChangeRestrict(pen.First(), baseCurrency, currencyDecimalFormat));
                }
            }

            return changeRestrictions.ToArray();
        }

        private static List<ChangeRestrictionsType> GetChangeRestrict(Penalty oIP, string baseCurrency, string currencyDecimalFormat)
        {
            string descriptionText = oIP.Text.Split(':').LastOrDefault()!.Trim();
            string journeyStageCodeText = oIP.Text.Split(':').FirstOrDefault()!.Trim();

            switch (descriptionText)
            {
                case Constants.Application.Free:
                case Constants.Application.ChangeAllowedFree:
                    return new List<ChangeRestrictionsType> {
                            new ChangeRestrictionsType
                            {
                                AllowedModificationInd = true,
                                AllowedModificationIndSpecified = true,
                                JourneyStageCode = journeyStageCodeText == Constants.Application.AfterDeparture ? JourneyStageTypeCodeType.AfterDeparture : JourneyStageTypeCodeType.PriorToDeparture,
                                JourneyStageCodeSpecified = true,
                                Fee = new ChangeOrCancelFeeType { Amount = new AmountType { CurCode = baseCurrency, Value = Convert.ToDecimal(string.Format(currencyDecimalFormat, 0)) } }
                            }
                        };
                case Constants.Application.ForFee:
                    return new List<ChangeRestrictionsType> {
                            new ChangeRestrictionsType
                            {
                                AllowedModificationInd = true,
                                AllowedModificationIndSpecified = true,
                                JourneyStageCode = journeyStageCodeText == Constants.Application.AfterDeparture ? JourneyStageTypeCodeType.AfterDeparture : JourneyStageTypeCodeType.PriorToDeparture,
                                JourneyStageCodeSpecified = true,
                                Fee = new ChangeOrCancelFeeType { Amount = new AmountType { Value = oIP.MaxPrice.Amount.MonetaryAmount is null ? Convert.ToDecimal(string.Format(currencyDecimalFormat, 0)) : Convert.ToDecimal(oIP.MaxPrice.Amount.MonetaryAmount), CurCode = baseCurrency  } }
                            }
                    };
                default:
                    return new List<ChangeRestrictionsType> {
                        new ChangeRestrictionsType
                        {
                                AllowedModificationInd = true,
                                AllowedModificationIndSpecified = true,
                                JourneyStageCode = string.Equals(journeyStageCodeText.Replace("-", " "), Constants.Application.BeforeDeparture, StringComparison.CurrentCultureIgnoreCase) ? JourneyStageTypeCodeType.PriorToDeparture : JourneyStageTypeCodeType.AfterDeparture,
                                JourneyStageCodeSpecified = true,
                            Fee = new ChangeOrCancelFeeType { Amount = new AmountType { Value = oIP.MaxPrice.Amount.MonetaryAmount is null ? Convert.ToDecimal(string.Format(currencyDecimalFormat, 0)) : Convert.ToDecimal(oIP.MaxPrice.Amount.MonetaryAmount), CurCode = baseCurrency  } }
                        }
                    };
            }
        }
    }
}
