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
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;

using WarningType = VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS.WarningType;

namespace VAA.EAPI.NDC.Offer.Mapper
{
    public static class OrderReshopResponseMapper
    {
        public static IATA_OrderReshopRS MapReshopResponse(OfferResponse offerResponse, IGenericDataMapper genericDataMapper, [Optional] System.Collections.Generic.List<EAPI.Domain.Entities.Common.NDCWarning> nDCWarnings, IEnumerable<OrderItemServices> svcList, OrderDocument orderDocument, bool isReshopDisruption = false)
        {
            List<BaggageAllowanceType> baggageAllowanceList = new();
            List<DatedMarketingSegmentType> datedMarketingSegmentList = new();
            List<DatedOperatingLegType> datedOperatingLegList = new();
            List<DatedOperatingSegmentType> datedOperatingSegmentList = new();
            List<PaxJourneyType> paxJourneyList = new();
            List<PaxSegmentType> paxSegmentList = new();
            List<PenaltyType> penaltyList = new();
            List<PriceClassType> priceClassList = new();
            int priceClassIdCounter = 0;

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

            return new()
            {
                Items = new OrderReshopResponseType[1]
                {
                    new()
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
                                Offer = GetReshopOffers(offerResponse, genericDataMapper, baggageAllowanceList, datedMarketingSegmentList, datedOperatingLegList, datedOperatingSegmentList, originDestList, paxJourneyList, paxSegmentList, penaltyList, priceClassList, priceClassIdCounter, svcList, orderDocument, isReshopDisruption)
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
                            PaxList = paxList.ToArray(),
                            PaxSegmentList  = paxSegmentList.ToArray(),
                            PriceClassList = priceClassList.ToArray(),
                            PenaltyList = offerResponse.DataList.Penalty != null ? GetPenaltyList(offerResponse.DataList.Penalty) : null

                        },
                        Warning = (nDCWarnings?.Any() ?? false) ? GetWarnings(nDCWarnings) : null
                    }
                }
            };
        }

        private static PenaltyType[] GetPenaltyList(IEnumerable<Penalty> penalties)
        {
            return penalties?.Select(p => new PenaltyType()
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

        private static WarningType[] GetWarnings(List<NDCWarning> nDCWarnings)
        {
            return nDCWarnings.Select(x => new WarningType()
            {
                Code = Convert.ToString(x.Code),
                DescText = x.ShortText,
                LangCode = Constants.Application.LangCode,
                TypeCode = x.Type,
                TagText = x.TagText
            }).ToArray();
        }
        private static ChangedOfferType[] GetReshopOffers(OfferResponse offerResponse, IGenericDataMapper genericDataMapper, List<BaggageAllowanceType> baggageAllowanceList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<OriginDestType> originDestList, List<PaxJourneyType> paxJourneyList, List<PaxSegmentType> paxSegmentList, List<PenaltyType> penaltyList, List<PriceClassType> priceClassList, int priceClassIdCounter, IEnumerable<OrderItemServices> svcList, OrderDocument orderDocument, bool isReshopDisruption)
        {
            int serviceIdCounter = 0;
            var changedOffers = new List<ChangedOfferType>();

            foreach (var offer in offerResponse.Offers)
            {
                var changedOfferType = new ChangedOfferType
                {
                    OfferID = offer.OfferId,
                    OwnerCode = Constants.Application.OWNERCODE,
                    AddedOfferItem = GetAddedOfferItem(ref serviceIdCounter, offer, offerResponse, genericDataMapper, datedMarketingSegmentList, datedOperatingLegList, datedOperatingSegmentList, originDestList, paxJourneyList, paxSegmentList, penaltyList, priceClassList, priceClassIdCounter, svcList, orderDocument, isReshopDisruption),
                    JourneyOverview = GetJourneyOverview(offer.OfferItems.First(), offerResponse.DataList, genericDataMapper, paxJourneyList, priceClassList, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, originDestList),
                    BaggageAssociations = GetBaggageAssociations(offer, offerResponse.DataList, genericDataMapper, paxJourneyList, baggageAllowanceList, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, originDestList),
                    OfferExpirationTimeLimitDateTime = DateTime.Parse(offer.OfferExpirationTimeLimit).ToUniversalTime(),
                    OfferExpirationTimeLimitDateTimeSpecified = true
                };

                changedOffers.Add(changedOfferType);
            }

            return changedOffers.ToArray();
        }

        private static BaggageAssociationsType[] GetBaggageAssociations(OfferDetails offer, Infrastructure.Model.OfferMS.DataList dataList, IGenericDataMapper genericDataMapper, List<PaxJourneyType> paxJourneyList, List<BaggageAllowanceType> baggageAllowanceList, List<PaxSegmentType> paxSegmentList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, List<OriginDestType> originDestList)
        {
            List<BaggageAssociationsType> offerTypeBaggageAllowances = new();
            var baggageAllowancesDict = dataList.BaggageAllowances.ToDictionary(bag => bag.BaggageAllowanceId);
            var services = offer.OfferItems.SelectMany(offerItem => offerItem.Services);

            foreach (var service in services)
            {
                if (service.BaggageRefIds != null && service.BaggageRefIds.Any())
                {
                    foreach (var bagID in service.BaggageRefIds)
                    {
                        if (baggageAllowancesDict.TryGetValue(bagID, out var baggage))
                        {
                            var baggageAllowanceId = GetBaggageAllowanceId(baggage, baggageAllowanceList);

                            if (!offerTypeBaggageAllowances.Any(oBA => oBA.BaggageAllowanceRefID.First() == baggageAllowanceId))
                            {
                                offerTypeBaggageAllowances.Add(new BaggageAssociationsType()
                                {
                                    BaggageAllowanceRefID = new string[] { baggageAllowanceId },
                                    PaxRefID = service.ServiceBeneficiaries.ToArray(),
                                    OfferFlightAssociations = new OfferFlightAssociationsType
                                    {
                                        Item = new PaxJourneyRefType()
                                        {
                                            PaxJourneyRefID = new string[] { GetPaxJourneyId(service, dataList, genericDataMapper, paxJourneyList, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, originDestList) }
                                        },
                                        ItemElementName = ItemChoiceType1.PaxJourneyRef
                                    }
                                });
                            }
                            else
                            {
                                //Get existing journeyList
                                var paxJourneyIds = new List<string>();
                                paxJourneyIds.AddRange((offerTypeBaggageAllowances.First(o => o.BaggageAllowanceRefID.First() == baggageAllowanceId)
                                    .OfferFlightAssociations.Item as PaxJourneyRefType).PaxJourneyRefID);

                                //Get existing PaxList
                                var paxIds = new List<string>();
                                paxIds.AddRange(offerTypeBaggageAllowances.First(o => o.BaggageAllowanceRefID.First() == baggageAllowanceId)
                                .PaxRefID);

                                //Add new Pax from offeritem or services
                                paxIds.AddRange(service.ServiceBeneficiaries);

                                //Add new JourneyID from services 
                                paxJourneyIds.Add(GetPaxJourneyId(service, dataList, genericDataMapper, paxJourneyList, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, originDestList));

                                (offerTypeBaggageAllowances.First(o => o.BaggageAllowanceRefID.First() == baggageAllowanceId)
                                .OfferFlightAssociations.Item as PaxJourneyRefType).PaxJourneyRefID = paxJourneyIds.Distinct().ToArray();

                                offerTypeBaggageAllowances.First(o => o.BaggageAllowanceRefID.First() == baggageAllowanceId)
                                    .PaxRefID = paxIds.Distinct().ToArray();
                            }
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

                if (baggageAllowance.TypeCode == "CarryOn")
                {
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
                        baggageAllowance.WeightAllowance = baggage.FreeBaggageAllowance?.WeightAllowance!.Select(wa => new WeightAllowanceType
                        {
                            MaximumWeightMeasure = Convert.ToDecimal(wa.MaximumWeightMeasure),
                            WeightUnitOfMeasurement = (Domain.RequestResponseModels.OrderReshopRS.WeightUnitCodeType)wa.WeightUnitOfMeasurement,
                            MaximumWeightMeasureSpecified = true,
                            TotalMaximumWeightMeasureSpecified = true,
                            TotalMaximumWeightMeasure = baggageAllowance.PieceAllowance.TotalQty * Convert.ToDecimal(wa.MaximumWeightMeasure)
                        }).ToArray();
                    }

                    if (baggage.BaggageAllowanceTypeCode == Constants.Application.CarryOnBag && baggage?.FreeBaggageAllowance?.BagDescriptors?.FirstOrDefault()?.BagDescriptorName.Contains(Constants.Application.CarryOnBaggageText) == true)
                    {
                        baggageAllowance.DescText = new[] { baggage?.FreeBaggageAllowance?.BagDescriptors?.FirstOrDefault()?.BagDescriptorName };
                    }
                }

                if (baggageAllowance.TypeCode == "Checked")
                {
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
                                                && bal.MaximumDimensionAllowance?.FirstOrDefault()?.LengthMeasure == Convert.ToDecimal(baggage.FreeBaggageAllowance?.DimensionAllowance?.FirstOrDefault()?.LengthMeasure)
                                                && bal.MaximumDimensionAllowance?.FirstOrDefault()?.LengthUnitOfMeasurement.ToString() == baggage.FreeBaggageAllowance?.DimensionAllowance?.FirstOrDefault()?.LengthUnitOfMeasurement.ToString()
                                                && bal.WeightAllowance?.FirstOrDefault()?.MaximumWeightMeasure == Convert.ToDecimal(baggage.FreeBaggageAllowance?.WeightAllowance?.FirstOrDefault()?.MaximumWeightMeasure)
                                                && bal.WeightAllowance?.FirstOrDefault()?.WeightUnitOfMeasurement.ToString() == baggage.FreeBaggageAllowance?.WeightAllowance?.FirstOrDefault()?.WeightUnitOfMeasurement.ToString());


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

        private static JourneyOverviewType GetJourneyOverview(OfferItem offerItem, Infrastructure.Model.OfferMS.DataList dataList, IGenericDataMapper genericDataMapper, List<PaxJourneyType> paxJourneyList, List<PriceClassType> priceClassList, List<PaxSegmentType> paxSegmentList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, List<OriginDestType> originDestList)
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
        private static AddedOfferItemType[] GetAddedOfferItem(ref int serviceIdCounter, OfferDetails offerDetails, OfferResponse offerResponse, IGenericDataMapper genericDataMapper, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<OriginDestType> originDestList, List<PaxJourneyType> paxJourneyList, List<PaxSegmentType> paxSegmentList, List<PenaltyType> penaltyList, List<PriceClassType> priceClassList, int priceClassIdCounter, IEnumerable<OrderItemServices> svcList, OrderDocument orderDoc, bool isReshopDisruption)
        {
            FareTypeRequest fareTypeRequest = orderDoc.FareTypeRequest;
            bool isHoldBookingReshop = orderDoc?.TimeLimits is not null && orderDoc.OrderItems?.Any(x => x.AccountableDocumentRefs is null) == true;
            var ticketingTimeLimit = offerDetails?.TicketingTimeLimit;

            var addedOfferItems = new List<AddedOfferItemType>();
            var offerItems = offerDetails.OfferItems.ToList();

            var accountCode = offerResponse.Parties?.Participants?.Any(x => x.AccountCode == "1") ?? false;
            var baseCurrency = offerResponse.DataList.Penalty?.FirstOrDefault()?.MaxPrice?.Amount.BaseCurrency ?? offerDetails.SalePrice?.FirstOrDefault()?.Amount.BaseCurrency;

            var currencyDecimalFormat = DecimalHelper.GetDecimalFormat(offerResponse.DataList.Penalty?.FirstOrDefault()?.MaxPrice.Amount.MonetaryAmount ?? offerDetails.SalePrice.FirstOrDefault()?.Amount.MonetaryAmount);

            foreach (var offerItem in offerItems)
            {
                var pricing = offerItem.OfferitemPricing.FirstOrDefault();
                var fareDetailTypes = new List<FareDetailType>();
                var differentialTypeCode = new DifferentialTypeCodeType();

                var baseFareDiff = pricing?.SalePriceDifference?.FirstOrDefault(s => s.PriceComponentType == Price.PriceComponentTypes.BaseFareEnum)?.Amount.MonetaryAmount;
                var taxDiff = pricing?.SalePriceDifference?.FirstOrDefault(s => s.PriceComponentType == Price.PriceComponentTypes.TotalTaxEnum)?.Amount.MonetaryAmount;

                var isDisruptedHoldBookingReshop = isHoldBookingReshop && isReshopDisruption;

                if (isDisruptedHoldBookingReshop)
                {
                    differentialTypeCode = DifferentialTypeCodeType.EvenExchange;
                }
                else if (isHoldBookingReshop)
                {
                    differentialTypeCode = DifferentialTypeCodeType.AddCol;
                }
                else if (isReshopDisruption)
                {
                    differentialTypeCode = DifferentialTypeCodeType.EvenExchange;
                }
                else if (baseFareDiff == "0.00" && taxDiff == "0.00")
                {
                    differentialTypeCode = DifferentialTypeCodeType.EvenExchange;
                }
                else if (pricing?.AddCollectPrice?.Amount != null)
                {
                    differentialTypeCode = DifferentialTypeCodeType.AddCol;
                }
                else if (pricing?.RefundPrice?.FirstOrDefault()?.Amount != null)
                {
                    differentialTypeCode = DifferentialTypeCodeType.Refund;
                }
                else
                {
                    differentialTypeCode = DifferentialTypeCodeType.EvenExchange;
                }

                // Determine paxRefId from the offerItem
                var paxRefId = offerItem.Services?.FirstOrDefault()?.ServiceBeneficiaries;
                if (paxRefId == null || !paxRefId.Any()) continue;

                // Filter svcList by serviceBeneficiaries matching the current paxRefId
                var svcFiltered = svcList.Where(svc =>svc.ServiceBeneficiaries != null && paxRefId != null &&svc.ServiceBeneficiaries.Intersect(paxRefId).Any()).ToList();

                if (!svcFiltered.Any()) continue;

                // Group by svc.OrderItemId for each ExistingOrderItem
                var existingOrderItems = svcFiltered.GroupBy(svc => svc.OrderItemId)
                                         .Select(group => new ExistingOrderItemType
                                          {
                                            OrderItemRefID = group.Key,
                                            ExistingService = GetExistingServiceist(group)
                                         }).ToArray();

                var addedOfferItem = new AddedOfferItemType
                {
                    OfferItemID = offerItem.OfferItemId,
                    PaymentTimeLimit = string.IsNullOrEmpty(ticketingTimeLimit) ? null : new PaymentTimeLimitType
                    {
                        Item = new PaymentTimeLimitDateType
                                {
                                    PaymentTimeLimitDateTime = Convert.ToDateTime(ticketingTimeLimit).ToUniversalTime(),
                                    PaymentTimeLimitDateTimeSpecified = true
                                } 
                    },
                    MandatoryInd = true,
                    MandatoryIndSpecified = true,
                    ReshopPrice = new()
                    {
                        Item = new PriceDifferentialType
                        {
                            DifferentialTypeCode = differentialTypeCode,
                            OldPrice = new PriceAndFareDetailsType
                            {
                                Price = GetOldFarePrice(offerItem.OfferitemPricing.FirstOrDefault()?.OriginalSalePrice)
                            }
                        }
                    },
                    Service = GetServices(ref serviceIdCounter, offerItem.Services, offerResponse.DataList, genericDataMapper, fareDetailTypes, ref priceClassIdCounter, baseCurrency, accountCode, datedMarketingSegmentList, datedOperatingLegList, datedOperatingSegmentList, originDestList, paxJourneyList, paxSegmentList, priceClassList, currencyDecimalFormat, pricing?.SalePrice,fareTypeRequest),
                    ExistingOrderItem = existingOrderItems
                };

                (addedOfferItem.ReshopPrice.Item as PriceDifferentialType).NewPrice = new PriceAndFareDetailsType()
                {
                    FareDetail = fareDetailTypes.ToArray(),
                    Price = GetNewFarePrice(pricing?.SalePrice, isReshopDisruption)
                };

                var oldTaxes = offerItem.OfferitemPricing.FirstOrDefault()?.OriginalSalePrice.Where(x => x.PriceComponentType == Price.PriceComponentTypes.TotalTaxEnum).ToList().FirstOrDefault().TaxBreakup;
                var newTaxes = pricing?.SalePrice.Where(x => x.PriceComponentType == Price.PriceComponentTypes.TotalTaxEnum).ToList().FirstOrDefault().TaxBreakup;
                
                var diffPriceTaxes = GetDiffPriceTaxBreakup(newTaxes, oldTaxes);

                (addedOfferItem.ReshopPrice.Item as PriceDifferentialType).DiffPrice = new DiffPriceAndFareDetailsType()
                {
                    FareDetail = GetFareDetail(offerItem),
                    Price = GetPrice(offerItem,diffPriceTaxes, isHoldBookingReshop, isReshopDisruption)
                };

                addedOfferItems.Add(addedOfferItem);
            }

            return addedOfferItems.ToArray();
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
                    }
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
                    }
                });
            }

            return diffTaxes;
        }

        private static ExistingServiceType[] GetExistingServiceist(IEnumerable<OrderItemServices> svcList)
        {
            List<ExistingServiceType> existingServiceType = new();
            var retainedSvc = svcList?.First().RetainServices.Distinct();
            var deletedSvc = svcList?.First().DeleteServices?.Distinct();

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

        private static OfferedServiceType[] GetServices(ref int serviceIdCounter, IEnumerable<OfferService> services, Infrastructure.Model.OfferMS.DataList dataList, IGenericDataMapper genericDataMapper, List<FareDetailType> fareDetailTypes, ref int priceClassCounter, string baseCurrency, bool accountCode, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<OriginDestType> originDestList, List<PaxJourneyType> paxJourneyList, List<PaxSegmentType> paxSegmentList, List<PriceClassType> priceClassList, string currencyDecimalFormat, List<Price> salePrice, FareTypeRequest fareTypeRequest)
        {
            List<string> journeyIDs = new();
            List<string> paxIDs = new();
            List<OfferedServiceType> OfferedServiceList = new();
            List<FareComponentType> fareComponents = new();
            string serviceIDs = string.Empty;
            int priceClassId = priceClassCounter;
            string serviceID = IdGenerator.CreateNewServiceID(++serviceIdCounter);

            foreach (var service in services)
            {
                GetFareComponents(dataList, genericDataMapper, priceClassList, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, baseCurrency, service, fareComponents, accountCode, currencyDecimalFormat,fareTypeRequest);

                if (paxIDs.Count == 0)
                {
                    paxIDs.AddRange(service.ServiceBeneficiaries.ToArray());
                }
                serviceIDs = serviceID;

                journeyIDs.Add(GetPaxJourneyId(service, dataList, genericDataMapper, paxJourneyList, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, originDestList));
            }

            priceClassCounter = priceClassId;

            fareDetailTypes.Add(new FareDetailType()
            {
                FareComponent = fareComponents.ToArray(),
                PaxRefID = paxIDs.ToArray()
            });

            GetOfferService(journeyIDs, paxIDs, OfferedServiceList, serviceIDs);

            return OfferedServiceList.ToArray();
        }

        private static void GetOfferService(IEnumerable<string> journeyIDs, IEnumerable<string> paxIDs, List<OfferedServiceType> OfferedServiceList, string serviceIDs)
        {
            OfferedServiceList.Add(new()
            {
                OfferServiceAssociation = new()
                {
                    Item = new PaxJourneyRefType()
                    {
                        PaxJourneyRefID = journeyIDs.ToArray()
                    }
                },
                ServiceID = serviceIDs,
                PaxRefID = paxIDs.ToArray()
            });
        }

        private static void AddJourneyReftoODList(string paxJourneyId, IEnumerable<string> JourneyRefIds, VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS.DataList dataList, List<OriginDestType> originDestList)
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

        private static string GetPaxJourneyId(OfferService offerService, Infrastructure.Model.OfferMS.DataList dataList, IGenericDataMapper genericDataMapper, List<PaxJourneyType> paxJourneyList, List<PaxSegmentType> paxSegmentList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, List<OriginDestType> originDestList)
        {
            var duration = dataList.Journey.FirstOrDefault(j => j.JourneyId == offerService.JourneyRefIds.FirstOrDefault())?.Duration;

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

        private static void GetFareComponents(Infrastructure.Model.OfferMS.DataList dataList, IGenericDataMapper genericDataMapper, List<PriceClassType> priceClassList, List<PaxSegmentType> paxSegmentList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, string baseCurrency, OfferService service, List<FareComponentType> fareComponents, bool accountCode, string currencyDecimalFormat, FareTypeRequest fareTypeRequest)
        {
            fareComponents.Add(new()
            {
                CancelRestrictions = GetCancelRestrictionsTypes(service, dataList, baseCurrency, currencyDecimalFormat),
                ChangeRestrictions = GetChangeRestrictionsTypes(service, dataList, baseCurrency, currencyDecimalFormat),
                FareBasisCode = dataList.PriceClass.First(x => x.Id == service.PriceClassRefIds.FirstOrDefault()).FareBasisCode,
                FareTypeCode = fareTypeRequest?.FareSubTypeCode,
                NegotiatedCode = fareTypeRequest?.FareTypeCode,
                PaxSegmentRefID = GetPaxSegmentRefIDs(service.ProductSegmentAssociation.OfferSegmentRefIds, genericDataMapper, dataList, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList),
                PriceClassRefID = GetPriceClassId(service, dataList, priceClassList)
            });
        }

        private static PriceType GetNewFarePrice(List<Price> salePrice, bool isReshopDisruption)
        {
            PriceType priceType = new()
            {
                BaseAmount = new AmountType()
                {
                    CurCode = salePrice.FirstOrDefault(s => s.PriceComponentType == Price.PriceComponentTypes.BaseFareEnum)?.Amount.BaseCurrency,
                    Value = isReshopDisruption ? 0.00m: Convert.ToDecimal(salePrice.FirstOrDefault(s => s.PriceComponentType == Price.PriceComponentTypes.BaseFareEnum)?.Amount.MonetaryAmount)
                },
                TotalAmount = new AmountType()
                {
                    CurCode = salePrice.FirstOrDefault(s => s.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum)?.Amount.BaseCurrency,
                    Value = isReshopDisruption ? 0.00m : Convert.ToDecimal(salePrice.FirstOrDefault(s => s.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum)?.Amount.MonetaryAmount)
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
                        } : null
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
                TaxCode = tax.Code
            }).ToArray();
        }

        private static string GetPriceClassId(OfferService offerService, Infrastructure.Model.OfferMS.DataList dataList, List<PriceClassType> priceClassList)
        {
            var priceClass = dataList.PriceClass.First(p => p.Id == offerService.PriceClassRefIds.FirstOrDefault());

            if (priceClassList.Any(x => x.Name == priceClass.ProductName))
            {
                return priceClassList.First(x => x.Name == priceClass.ProductName).PriceClassID;
            }
            else
            {
                var priceClassType = new PriceClassType()
                {
                    PriceClassID = IdCreator.CreatePriceClassID(priceClassList.Count),
                    Name = priceClass.ProductName,
                    CabinType = new CabinTypeType[]
                    {
                        new()
                        {
                            CabinTypeCode = priceClass.CabinType?.CabinCode,
                            CabinTypeName = priceClass.CabinType?.CabinName
                        }
                    }
                };

                priceClassList.Add(priceClassType);

                return priceClassType.PriceClassID;
            }
        }

        private static DifferentialFareDetailType[] GetFareDetail(OfferItem offerItem)
        {
            List<DifferentialFareDetailType> differentialFareDetailTypes = new();

            var penaltyRefID = offerItem.Services?.First().PenaltyRef?.ChangePenaltyRefIds?.ToArray();

            if (penaltyRefID.IsNullOrEmpty()) return differentialFareDetailTypes.ToArray();

            DifferentialFareDetailType differentialFareDetailType = new()
            {
                PaxRefID = offerItem.Services.FirstOrDefault()?.ServiceBeneficiaries.ToArray(),
                PenaltyRefID = penaltyRefID
            };

            differentialFareDetailTypes.Add(differentialFareDetailType);

            return differentialFareDetailTypes.ToArray();
        }

        private static DifferentialPriceType GetPrice(OfferItem offerItem, IEnumerable<Tax> taxes, bool isHoldBookingReshop, bool isReshopDisruption)
        {
            var price = new DifferentialPriceType()
            {
                BaseAmount = new AmountType()
                {
                    CurCode = offerItem.OfferitemPricing.FirstOrDefault()?.SalePriceDifference.FirstOrDefault(d => d.PriceComponentType == Price.PriceComponentTypes.BaseFareEnum)?.Amount.BaseCurrency,
                    Value = Convert.ToDecimal(offerItem.OfferitemPricing.FirstOrDefault()?.SalePriceDifference.FirstOrDefault(d => d.PriceComponentType == Price.PriceComponentTypes.BaseFareEnum)?.Amount.MonetaryAmount),
                },
                TaxSummary = GetTaxSummary(offerItem.OfferitemPricing.FirstOrDefault()?.SalePriceDifference, taxes, isReshopDisruption)
            };

            price.TotalAmount = new AmountType()
            {
                CurCode = price.BaseAmount.CurCode,
                Value = isReshopDisruption
                    ? 0.00m
                    : price.BaseAmount.Value + price.TaxSummary.FirstOrDefault().TotalTaxAmount.Value
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
                    Value = offerItem.OfferitemPricing.FirstOrDefault()?.RefundPrice != null
                    ? Convert.ToDecimal(offerItem.OfferitemPricing.FirstOrDefault()?.RefundPrice.FirstOrDefault()?.Amount.MonetaryAmount): 0.00m
                };

                price.DueToAirlineAmount = new AmountType()
                {
                    CurCode = price.BaseAmount.CurCode,
                    Value = offerItem.OfferitemPricing.FirstOrDefault()?.AddCollectPrice != null
                     ? Convert.ToDecimal(offerItem.OfferitemPricing.FirstOrDefault()?.AddCollectPrice?.Amount.MonetaryAmount)
                     : 0.00m
                };

            }

            

            return price;
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
                PaxRefID = offerItem.Services.FirstOrDefault()?.ServiceBeneficiaries.ToArray(),
                Price = GetOldFarePrice(offerItem.OfferitemPricing.FirstOrDefault()?.OriginalSalePrice)
            } };
        }

        private static PriceType GetOldFarePrice(List<Price> originalSalePrice)
        {
            return new PriceType()
            {
                BaseAmount = new AmountType()
                {
                    CurCode = originalSalePrice.FirstOrDefault(s => s.PriceComponentType == Price.PriceComponentTypes.BaseFareEnum)?.Amount.BaseCurrency,
                    Value = Convert.ToDecimal(originalSalePrice.FirstOrDefault(s => s.PriceComponentType == Price.PriceComponentTypes.BaseFareEnum)?.Amount.MonetaryAmount)
                },
                TotalAmount = new AmountType()
                {
                    CurCode = originalSalePrice.FirstOrDefault(s => s.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum)?.Amount.BaseCurrency,
                    Value = Convert.ToDecimal(originalSalePrice.FirstOrDefault(s => s.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum)?.Amount.MonetaryAmount)
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

        private static string[] GetPaxSegmentRefIDs(IEnumerable<string> offerSegmentRefIds, IGenericDataMapper genericDataMapper, Infrastructure.Model.OfferMS.DataList dataList, List<PaxSegmentType> PaxSegmentList, List<DatedMarketingSegmentType> DatedMarketingSegmentList, List<DatedOperatingSegmentType> DatedOperatingSegmentList, List<DatedOperatingLegType> DatedOperatingLegList)
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
                return paxSegmentList.FirstOrDefault(seg => seg.MarketingCarrierRBD_Code == offerFlightSegment.ClassOfService && (seg.CabinTypeAssociationChoice?.Item as CabinTypeType)?.CabinTypeName == offerFlightSegment.CabinType.CabinName && (seg.CabinTypeAssociationChoice?.Item as CabinTypeType)?.CabinTypeCode == offerFlightSegment.CabinType.CabinCode && seg.DatedMarketingSegmentRefId == datedMarketingSegmentId)?.PaxSegmentID;
            }
            else
            {
                var paxSegmentType = CreatePaxSegmentType(offerFlightSegment, paxSegmentList, datedMarketingSegmentId);
                paxSegmentList!.Add(paxSegmentType);
                return paxSegmentType.PaxSegmentID;
            }
        }

        private static PaxSegmentType CreatePaxSegmentType(OfferSegment offerFlightSegment, List<PaxSegmentType> paxSegmentList, string datedMarketingSegmentId)
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
                DatedMarketingSegmentRefId = datedMarketingSegmentId
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
                        AircraftScheduledDateTimeSpecified = true
                    }
                };

                var transportDepType = new TransportPointType
                {
                    IATA_LocationCode = flightSegment.OriginAirportCode,
                    AircraftScheduledDateTime = scheduledDepartureTime,
                    AircraftScheduledDateTimeSpecified = true
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
            return new DatedOperatingSegmentType
            {
                DatedOperatingSegmentId = IdGenerator.CreateDatedOperatingSegmentID(count),
                CarrierName = string.IsNullOrEmpty(flightSegment.OperatingCarrier) ? genericDataMapper.GetOperatingCarrierNameByCode(flightSegment.MarketingCarrier) : genericDataMapper.GetOperatingCarrierNameByCode(flightSegment.OperatingCarrier),
                CarrierDesigCode = string.IsNullOrEmpty(flightSegment.OperatingCarrier) ? flightSegment.MarketingCarrier : flightSegment.OperatingCarrier,
                OperatingCarrierFlightNumberText = string.IsNullOrEmpty(flightSegment.OperatingFlightNumber) ? flightSegment.MarketingFlightNumber : flightSegment.OperatingFlightNumber,
                Duration = $"{Constants.Application.PT}{(TimeSpan.Parse(flightSegment.FlightDuration, CultureInfo.InvariantCulture).Days * 24) + TimeSpan.Parse(flightSegment.FlightDuration, CultureInfo.InvariantCulture).Hours}{Constants.Application.H}{TimeSpan.Parse(flightSegment.FlightDuration, CultureInfo.InvariantCulture).Minutes}{Constants.Application.M}{TimeSpan.Parse(flightSegment.FlightDuration, CultureInfo.InvariantCulture).Seconds}{Constants.Application.S}",
                DatedOperatingLegRefID = flightSegment.Legs.Select(legRef => CreateDatedOperatingLegType(flightSegment?.AirCraft, legRef, datedOperatingLegList)).ToArray()
            };
        }

        private static string CreateDatedOperatingLegType(string aircraftTypeCode, Legs leg, List<DatedOperatingLegType> datedOperatingLegList)
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
                    AircraftScheduledDateTimeSpecified = true
                },
                Dep = new TransportPointType
                {
                    IATA_LocationCode = leg.OriginAirportCode,
                    AircraftScheduledDateTime = Convert.ToDateTime(leg.ScheduledDepartureTime.LocalDateTime),
                    AircraftScheduledDateTimeSpecified = true
                },
                IATA_AircraftType = new IATA_AircraftTypeType
                {
                    IATA_AircraftTypeCode = aircraftTypeCode
                },
                DatedOperatingLegID = IdCreator.CreateDatedOperatingLegID(datedOperatingLegList.Count),
                OnGroundDuration = string.IsNullOrEmpty(onGroundDuration) ? null : onGroundDuration
            };
            datedOperatingLegList.Add(datedOperatingLeg);
            return datedOperatingLeg.DatedOperatingLegID;
        }

        private static string GetDurationV2(TimeSpan timeSpan)
        {
            return $"{Constants.Application.PT}{(timeSpan.Days * 24) + timeSpan.Hours}{Constants.Application.H}{timeSpan.Minutes}{Constants.Application.M}{timeSpan.Seconds}{Constants.Application.S}";
        }

        private static CancelRestrictionsType[] GetCancelRestrictionsTypes(OfferService offerService, Infrastructure.Model.OfferMS.DataList dataList, string baseCurrency, string currencyDecimalFormat)
        {
            var cancelRestrictions = new List<CancelRestrictionsType>();
            var penalty = new List<Penalty>();

            foreach (var penRefId in offerService?.PenaltyRef?.CancellationPenaltyRefIds ?? Enumerable.Empty<string>())
                penalty.Add(dataList.Penalty.First(pen => pen.Id == penRefId));

            var fareBasisCode = dataList.PriceClass.First(pC => pC.Id == offerService.PriceClassRefIds.FirstOrDefault()).FareBasisCode;
            var boundDetails = string.Empty;
            var allowedModificationInd = true;
            var allowedModificationIndSpecified = true;
            var journeyStageCodeSpecified = true;
            var flightsegments = new List<Segment>();

            foreach (var x in offerService.ProductSegmentAssociation.OfferSegmentRefIds)
            {
                flightsegments.Add(dataList.FlightSegments.First(z => z.SegmentId == dataList.OfferSegments.First(y => y.OfferSegmentId == x).FlightSegmentRefId));
            }

            var originBoundAirport = flightsegments.First().OriginAirportCode;
            var destinationBoundAirport = flightsegments.Last().DestinationAirportCode;
            boundDetails = $"{originBoundAirport}-{destinationBoundAirport}";
            bool nonRefundable = penalty.Any(pen => pen.Text == "BEFORE-DEPARTURE") && penalty.Any(pen => pen.Text == "AFTER-DEPARTURE");

            if (penalty.Count > 0)
            {
                foreach (var oIP in penalty)
                {
                    if (oIP?.MaxPrice != null)
                    {
                        if (fareBasisCode!.StartsWith('Y') || fareBasisCode.StartsWith('W') || fareBasisCode.StartsWith('J'))
                        {

                            boundDetails = $"{Constants.Application.PenaltyFreeCANCELLATIONS} - {originBoundAirport}-{destinationBoundAirport}";

                        }
                        else if (nonRefundable)
                        {
                            boundDetails = $"{Constants.Application.PenaltyNotFreeCANCELLATIONS} - {originBoundAirport}-{destinationBoundAirport}";
                        }
                        else
                        {
                            boundDetails = $"{originBoundAirport}-{destinationBoundAirport}";
                        }

                        allowedModificationInd = (oIP.MaxPrice != null && !nonRefundable);
                        allowedModificationIndSpecified = true;
                        journeyStageCodeSpecified = true;
                        CancelRestrictionsType cancelRestriction = GetCancelRestrict(oIP, boundDetails, allowedModificationInd, allowedModificationIndSpecified, journeyStageCodeSpecified);
                        bool containsItem = CheckCancelRestrictionExists(cancelRestrictions, cancelRestriction);

                        if (!containsItem)
                        {
                            cancelRestrictions.Add(cancelRestriction);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(fareBasisCode))
                        {
                            if (fareBasisCode.StartsWith('Y') || fareBasisCode.StartsWith('W') || fareBasisCode.StartsWith('J'))
                            {
                                allowedModificationInd = false;
                                allowedModificationIndSpecified = true;
                                journeyStageCodeSpecified = false;
                                CancelRestrictionsType cancelRestriction = GetCancelRestrict(oIP, $"{Constants.Application.PenaltyFreeCANCELLATIONS} - {boundDetails}", allowedModificationInd, allowedModificationIndSpecified, journeyStageCodeSpecified);
                                bool containsItem = CheckCancelRestrictionExists(cancelRestrictions, cancelRestriction);

                                if (!containsItem)
                                {
                                    cancelRestrictions.Add(cancelRestriction);
                                }
                            }
                            else
                            {
                                allowedModificationInd = false;
                                allowedModificationIndSpecified = true;
                                journeyStageCodeSpecified = false;
                                CancelRestrictionsType cancelRestriction = GetCancelRestrict(oIP, $"{Constants.Application.PenaltyNotFreeCANCELLATIONS} - {boundDetails}", allowedModificationInd, allowedModificationIndSpecified, journeyStageCodeSpecified);
                                bool containsItem = CheckCancelRestrictionExists(cancelRestrictions, cancelRestriction);

                                if (!containsItem)
                                {
                                    cancelRestrictions.Add(cancelRestriction);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                var priceClassRef = offerService?.PriceClassRefIds.First();
                foreach (var pc in dataList?.PriceClass!)
                {
                    if (pc?.Id == priceClassRef && pc!.ProductName.Contains("Flex"))
                    {
                        string boundOriginDest = $"{originBoundAirport}-{destinationBoundAirport}";
                        CancelRestrictionsType[] cancelRestriction = GetCancelRestrictFlex(boundOriginDest, allowedModificationInd, allowedModificationIndSpecified, journeyStageCodeSpecified, baseCurrency, currencyDecimalFormat);
                        cancelRestrictions.AddRange(cancelRestriction);
                    }
                }
            }

            foreach (var cancel in cancelRestrictions)
                cancel.DescText = null;

            return cancelRestrictions.ToArray();
        }

        private static CancelRestrictionsType GetCancelRestrict(Penalty oIP, string boundDetails, bool allowedModificationInd, bool allowedModificationIndSpecified, bool journeyStageCodeSpecified)
        {
            if (allowedModificationInd)
            {
                return new CancelRestrictionsType
                {
                    AllowedModificationInd = allowedModificationInd,
                    AllowedModificationIndSpecified = allowedModificationIndSpecified,
                    DescText = string.Empty,
                    JourneyStageCode = oIP.Text == "NO-SHOW" ? JourneyStageTypeCodeType.NoShow : oIP.Text == "BEFORE-DEPARTURE" ? JourneyStageTypeCodeType.PriorToDeparture : JourneyStageTypeCodeType.AfterDeparture,
                    JourneyStageCodeSpecified = journeyStageCodeSpecified
                };
            }
            else
            {
                return new CancelRestrictionsType
                {
                    AllowedModificationInd = allowedModificationInd,
                    AllowedModificationIndSpecified = allowedModificationIndSpecified,
                    DescText = string.Empty,
                    Fee = null,
                    JourneyStageCode = oIP.Text == "NO-SHOW" ? JourneyStageTypeCodeType.NoShow : oIP.Text == "BEFORE-DEPARTURE" ? JourneyStageTypeCodeType.PriorToDeparture : JourneyStageTypeCodeType.AfterDeparture,
                    JourneyStageCodeSpecified = journeyStageCodeSpecified
                };
            }
        }

        private static bool CheckCancelRestrictionExists(List<CancelRestrictionsType> cancelRestrictionsList, CancelRestrictionsType cancelRestrictions)
        {
            if (cancelRestrictionsList.Any(item => item.DescText.Contains(Constants.Application.PenaltyFreeCANCELLATIONS) || item.DescText.Contains(Constants.Application.PenaltyNotFreeCANCELLATIONS)))
            {
                return cancelRestrictionsList.Any(item => item.DescText == cancelRestrictions?.DescText);
            }
            else
            {
                return cancelRestrictionsList.Any(item => item.JourneyStageCode == cancelRestrictions?.JourneyStageCode && item.AllowedModificationInd == cancelRestrictions.AllowedModificationInd && item.Fee.Amount.CurCode == cancelRestrictions.Fee?.Amount?.CurCode && item.Fee.Amount.Value == cancelRestrictions.Fee?.Amount?.Value && item.DescText == cancelRestrictions.DescText);
            }
        }

        private static CancelRestrictionsType[] GetCancelRestrictFlex(string boundDetails, bool allowedModificationInd, bool allowedModificationIndSpecified, bool journeyStageCodeSpecified, string baseCurrency, string currencyDecimalFormat)
        {
            return new List<CancelRestrictionsType>
            {
                new() {
                  AllowedModificationInd = allowedModificationInd,
                  AllowedModificationIndSpecified = allowedModificationIndSpecified,
                  JourneyStageCodeSpecified = journeyStageCodeSpecified,
                  JourneyStageCode = JourneyStageTypeCodeType.PriorToDeparture,
                  DescText = boundDetails,
                  Fee = new ChangeOrCancelFeeType()
                  {
                    Amount = new AmountType()
                    {
                        CurCode = baseCurrency,
                        Value = Convert.ToDecimal(string.Format(currencyDecimalFormat, 0))
                    }
                  }
                },
                new() {
                  AllowedModificationInd = allowedModificationInd,
                  AllowedModificationIndSpecified = allowedModificationIndSpecified,
                  JourneyStageCodeSpecified = journeyStageCodeSpecified,
                  JourneyStageCode = JourneyStageTypeCodeType.AfterDeparture,
                  DescText = boundDetails,
                  Fee = new ChangeOrCancelFeeType()
                  {
                    Amount = new AmountType()
                    {
                        CurCode = baseCurrency,
                        Value = Convert.ToDecimal(string.Format(currencyDecimalFormat, 0))
                    }
                  }
                }
            }.ToArray();
        }

        private static ChangeRestrictionsType[] GetChangeRestrictionsTypes(OfferService offerService, Infrastructure.Model.OfferMS.DataList dataList, string baseCurrency, string currencyDecimalFormat)
        {
            var changeRestrictions = new List<ChangeRestrictionsType>();
            var penalty = new List<Penalty>();

            foreach (var penRefId in offerService?.PenaltyRef?.ChangePenaltyRefIds ?? Enumerable.Empty<string>())
                penalty.Add(dataList.Penalty.First(pen => pen.Id == penRefId));

            var fareBasisCode = dataList.PriceClass.First(pen => pen.Id == offerService.PriceClassRefIds.FirstOrDefault()).FareBasisCode;
            var boundDetails = string.Empty;
            var allowedModificationInd = true;
            var allowedModificationIndSpecified = true;
            var journeyStageCodeSpecified = true;
            var flightsegments = new List<Segment>();

            foreach (var oSeg in offerService.ProductSegmentAssociation.OfferSegmentRefIds)
            {
                flightsegments.Add(dataList.FlightSegments.First(fSeg => fSeg.SegmentId == dataList.OfferSegments.First(y => y.OfferSegmentId == oSeg).FlightSegmentRefId));
            }

            var originBoundAirport = flightsegments.First().OriginAirportCode;
            var destinationBoundAirport = flightsegments.Last().DestinationAirportCode;

            if (penalty.Count > 0)
            {
                foreach (var oIP in penalty)
                {
                    if (oIP?.MaxPrice != null)
                    {
                        if (fareBasisCode!.StartsWith('Y') || fareBasisCode.StartsWith('W') || fareBasisCode.StartsWith('J'))
                        {
                            boundDetails = $"{Constants.Application.PenaltyFreeCHANGES}-{originBoundAirport}-{destinationBoundAirport}";
                        }
                        else
                        {
                            boundDetails = $"{originBoundAirport}-{destinationBoundAirport}";
                        }

                        allowedModificationInd = (oIP.MaxPrice != null);
                        allowedModificationIndSpecified = true;
                        journeyStageCodeSpecified = true;
                        ChangeRestrictionsType cancelRestriction = GetChangeRestrict(oIP, boundDetails, allowedModificationInd, allowedModificationIndSpecified, journeyStageCodeSpecified, baseCurrency, currencyDecimalFormat);
                        bool containsItem = CheckChangeRestrictionExists(allowedModificationInd, changeRestrictions, cancelRestriction);

                        if (!containsItem)
                        {
                            changeRestrictions.Add(cancelRestriction);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(fareBasisCode))
                        {

                            if (fareBasisCode.StartsWith('Y') || fareBasisCode.StartsWith('W') || fareBasisCode.StartsWith('J'))
                            {
                                allowedModificationInd = false;
                                allowedModificationIndSpecified = true;
                                journeyStageCodeSpecified = true;
                                ChangeRestrictionsType cancelRestriction = GetChangeRestrict(oIP, Constants.Application.PenaltyFreeCANCELLATIONS + " - " + boundDetails, allowedModificationInd, allowedModificationIndSpecified, journeyStageCodeSpecified, baseCurrency, currencyDecimalFormat);
                                bool containsItem = CheckChangeRestrictionExists(allowedModificationInd, changeRestrictions, cancelRestriction);

                                if (!containsItem)
                                {
                                    changeRestrictions.Add(cancelRestriction);
                                }
                            }
                            else
                            {
                                allowedModificationInd = false;
                                allowedModificationIndSpecified = true;
                                journeyStageCodeSpecified = true;
                                ChangeRestrictionsType cancelRestriction = GetChangeRestrict(oIP, Constants.Application.PenaltyNotFreeCANCELLATIONS + " - " + boundDetails, allowedModificationInd, allowedModificationIndSpecified, journeyStageCodeSpecified, baseCurrency, currencyDecimalFormat);
                                bool containsItem = CheckChangeRestrictionExists(allowedModificationInd, changeRestrictions, cancelRestriction);

                                if (!containsItem)
                                {
                                    changeRestrictions.Add(cancelRestriction);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                var priceClassRef = offerService?.PriceClassRefIds.First();
                foreach (var pc in dataList?.PriceClass!)
                {
                    if (pc?.Id == priceClassRef && pc!.ProductName.Contains("Flex"))
                    {
                        string boundOriginDest = $"{originBoundAirport}-{destinationBoundAirport}";
                        ChangeRestrictionsType[] changeRestriction = GetChangeRestrictFlex(boundOriginDest, allowedModificationInd, allowedModificationIndSpecified, journeyStageCodeSpecified, baseCurrency, currencyDecimalFormat);
                        changeRestrictions.AddRange(changeRestriction);
                    }
                }
            }

            foreach (var change in changeRestrictions)
                change.DescText = null;

            return changeRestrictions.ToArray();
        }

        private static ChangeRestrictionsType GetChangeRestrict(Penalty oIP, string descText, bool allowedModificationInd, bool allowedModificationIndSpecified, bool journeyStageCodeSpecified, string baseCurrency, string currencyDecimalFormat)
        {
            if (allowedModificationInd)
            {
                return new ChangeRestrictionsType
                {
                    AllowedModificationInd = allowedModificationInd,
                    AllowedModificationIndSpecified = allowedModificationIndSpecified,
                    ChangeTypeCode = ChangeTypeCodeType.Flight,
                    DescText = descText,
                    Fee = new ChangeOrCancelFeeType()
                    {
                        Amount = new AmountType()
                        {
                            CurCode = oIP?.MaxPrice?.Amount?.BaseCurrency ?? baseCurrency,
                            Value = string.IsNullOrEmpty(oIP?.MaxPrice?.Amount?.MonetaryAmount) ? Convert.ToDecimal(string.Format(currencyDecimalFormat, 0)) : Convert.ToDecimal(oIP.MaxPrice?.Amount?.MonetaryAmount)
                        },
                    },
                    JourneyStageCode = oIP!.Text == "NO-SHOW" ? JourneyStageTypeCodeType.NoShow : oIP.Text == "BEFORE-DEPARTURE" ? JourneyStageTypeCodeType.PriorToDeparture : JourneyStageTypeCodeType.AfterDeparture,
                    JourneyStageCodeSpecified = journeyStageCodeSpecified
                };
            }
            else
            {
                return new ChangeRestrictionsType
                {
                    AllowedModificationInd = allowedModificationInd,
                    AllowedModificationIndSpecified = allowedModificationIndSpecified,
                    ChangeTypeCode = ChangeTypeCodeType.Flight,
                    DescText = descText,
                    Fee = null,
                    JourneyStageCodeSpecified = false
                };
            }
        }

        private static bool CheckChangeRestrictionExists(bool allowedModificationInd, List<ChangeRestrictionsType> changeRestrictionsList, ChangeRestrictionsType changeRestrictions)
        {
            return allowedModificationInd ? changeRestrictionsList.Any(item =>
                                                  item.DescText == changeRestrictions?.DescText &&
                                                  item.JourneyStageCode == changeRestrictions?.JourneyStageCode &&
                                                   item.Fee.Amount.CurCode == changeRestrictions.Fee?.Amount?.CurCode &&
                                                  item.Fee.Amount.Value == changeRestrictions.Fee?.Amount?.Value) : changeRestrictionsList.Any(item => item.DescText == changeRestrictions?.DescText);
        }

        private static ChangeRestrictionsType[] GetChangeRestrictFlex(string descText, bool allowedModificationInd, bool allowedModificationIndSpecified, bool journeyStageCodeSpecified, string baseCurrency, string currencyDecimalFormat)
        {
            return new List<ChangeRestrictionsType>{
                    new() {
                AllowedModificationInd = allowedModificationInd,
                AllowedModificationIndSpecified = allowedModificationIndSpecified,
                ChangeTypeCode = ChangeTypeCodeType.Flight,
                DescText = descText,
                Fee = new ChangeOrCancelFeeType()
                {
                    Amount = new AmountType()
                    {
                        CurCode = baseCurrency,
                        Value = Convert.ToDecimal(string.Format(currencyDecimalFormat, 0))
                    }
                },
                JourneyStageCode = JourneyStageTypeCodeType.PriorToDeparture,
                JourneyStageCodeSpecified = journeyStageCodeSpecified
            },
            new() {
                    AllowedModificationInd = allowedModificationInd,
                    AllowedModificationIndSpecified = allowedModificationIndSpecified,
                    ChangeTypeCode = ChangeTypeCodeType.Flight,
                    DescText = descText,
                    Fee = new ChangeOrCancelFeeType()
                    {
                        Amount = new AmountType()
                        {
                            CurCode =  baseCurrency,
                            Value = Convert.ToDecimal(string.Format(currencyDecimalFormat, 0))
                        }
                    },
                    JourneyStageCode = JourneyStageTypeCodeType.AfterDeparture,
                    JourneyStageCodeSpecified = journeyStageCodeSpecified
                }
            }.ToArray();
        }
    }
}
