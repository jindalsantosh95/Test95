using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Products.Baggage;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.AirShoppingRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Helper;
using VAA.EAPI.NDC.Offer.Infrastructure.Mapper;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.FareFamily;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using static VAA.EAPI.Domain.Virgin.Models.Common.Price;
using OriginDestination = VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS.OriginDestination;

namespace VAA.EAPI.NDC.Offer.Mapper.AirShopping
{
    public static class AirshoppingResponseMapper
    {
        public static IATA_AirShoppingRS MapOfferResponse(OfferResponse offerResponse, IGenericDataMapper genericDataMapper, CalendarSearchResponse calendarSearchResponse, IEnumerable<NDCWarning> nDCWarnings,string fareCodeSubType, string fareTypeCode)
        {
            List<PriceClassType> priceClassList = new();
            List<PaxSegmentType> paxSegmentList = new();
            List<DatedMarketingSegmentType> datedMarketingSegmentList = new();
            List<DatedOperatingSegmentType> datedOperatingSegmentList = new();
            List<DatedOperatingLegType> datedOperatingLegList = new();
            List<BaggageAllowanceType> baggageAllowanceList = new();
            List<PaxJourneyType> paxJourneyList = new();
            List<OriginDestType> originDestList = new();
            ServiceId serviceIdCount = new();

            foreach (var ond in offerResponse.DataList.OriginDestinationAssociations)
            {
                originDestList.Add(new OriginDestType
                {

                    DestCode = ond.OriginDestination.DestinationAirport,
                    OriginCode = ond.OriginDestination.OriginAirport,
                    OriginDestID = ond.OdLabel,
                    PaxJourneyRefID = []
                });
            }

            return new()
            {
                Items = new AirShoppingResponseType[1]
                {
                   new()
                   {
                      OffersGroup = new()
                        {
                            CarrierOffers = new CarrierOffersType[1]
                            {
                                new()
                                {
                                    Offer = GetOffers(offerResponse, genericDataMapper, priceClassList, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList,  baggageAllowanceList, paxJourneyList, originDestList, serviceIdCount , fareCodeSubType, fareTypeCode),
                                    PriceCalendar = calendarSearchResponse != null && calendarSearchResponse.Error == null ? GetPriceCalendarOffers(calendarSearchResponse, offerResponse) : null
                                }
                            }
                        },
                      DataLists = new()
                      {
                          BaggageAllowanceList = baggageAllowanceList.ToArray(),
                          PaxList = GetPaxDetails(offerResponse.DataList.CustomerDetails),
                          PaxSegmentList = paxSegmentList.ToArray(),
                          PaxJourneyList = paxJourneyList.ToArray(),
                          PriceClassList = priceClassList.ToArray(),
                          OriginDestList = originDestList.ToArray(),
                          DatedMarketingSegmentList = datedMarketingSegmentList.ToArray(),
                          DatedOperatingSegmentList = datedOperatingSegmentList.ToArray(),
                          DatedOperatingLegList = datedOperatingLegList.ToArray()
                      },
                      Warning = nDCWarnings.Any() ? GetWarnings(nDCWarnings) : null
                   }
                } 
            };
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

        private static NewOfferType[] GetOffers(OfferResponse offerResponse, IGenericDataMapper genericDataMapper, List<PriceClassType> priceClassList, List<PaxSegmentType> paxSegmentList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, List<BaggageAllowanceType> baggageAllowanceList, List<PaxJourneyType> paxJourneyList, List<OriginDestType> originDestList, ServiceId serviceIdCount, string fareCodeSubType, string fareTypeCode)
        {
            return offerResponse.Offers.Select(offer =>
            {
                return new NewOfferType()
                {
                    OfferID = offer.OfferId,
                    OwnerCode = Constants.Application.OWNERCODE,
                    OfferItem = GetOfferItems(offer, offerResponse.DataList, offerResponse.Parties, genericDataMapper, priceClassList, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, paxJourneyList, originDestList, serviceIdCount, fareCodeSubType, fareTypeCode),
                    JourneyOverview = GetJourneyOverview(offer.OfferItems.First(), offerResponse.DataList, genericDataMapper, paxJourneyList, priceClassList, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, originDestList),
                    BaggageAssociations = GetBaggageAssociations(offer, offerResponse.DataList, genericDataMapper, paxJourneyList, baggageAllowanceList, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, originDestList),
                    OfferExpirationTimeLimitDateTime = Convert.ToDateTime(offer.OfferExpirationTimeLimit).ToUniversalTime(),
                    OfferExpirationTimeLimitDateTimeSpecified = true
                };
            }).ToArray();
        }

        private static BaggageAssociationsType[] GetBaggageAssociations(OfferDetails offer, DataList dataList, IGenericDataMapper genericDataMapper, List<PaxJourneyType> paxJourneyList, List<BaggageAllowanceType> baggageAllowanceList, List<PaxSegmentType> paxSegmentList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, List<OriginDestType> originDestList)
        {
            var offerTypeBaggageAllowances = new List<BaggageAssociationsType>();
            var baggageAssociationsById = new Dictionary<string, BaggageAssociationsType>();
            var baggageAllowancesDict = (dataList.BaggageAllowances ?? Enumerable.Empty<BaggageAllowances>()).DistinctBy(bag => bag.BaggageAllowanceId).ToDictionary(bag => bag.BaggageAllowanceId);
            var services = offer.OfferItems.SelectMany(offerItem => offerItem.Services).Where(service => service.BaggageRefIds != null && service.BaggageRefIds.Any());

            foreach (var service in services)
            {
                foreach (var bagID in service.BaggageRefIds)
                {
                    if (baggageAllowancesDict.TryGetValue(bagID, out var baggage))
                    {
                        var baggageAllowanceId = GetBaggageAllowanceId(baggage, baggageAllowanceList);
                        baggageAssociationsById.TryGetValue(baggageAllowanceId, out var existingBaggageAssociation);

                        if (existingBaggageAssociation == null)
                        {
                            var baggageAssociation = new BaggageAssociationsType
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
                            };
                            offerTypeBaggageAllowances.Add(baggageAssociation);
                            baggageAssociationsById.Add(baggageAllowanceId, baggageAssociation);
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

        private static JourneyOverviewType GetJourneyOverview(OfferItem offerItem, DataList dataList, IGenericDataMapper genericDataMapper, List<PaxJourneyType> paxJourneyList, List<PriceClassType> priceClassList, List<PaxSegmentType> paxSegmentList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, List<OriginDestType> originDestList)
        {
            JourneyOverviewType journeyOverviewType = new();
            List<JourneyPriceClassType> journeyPriceClassList = [];

            foreach (var serv in offerItem.Services)
            {
                JourneyPriceClassType journeyPriceClassType = new()
                {
                    PaxJourneyRefID = GetPaxJourneyId(serv, dataList, genericDataMapper, paxJourneyList, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, originDestList),
                    PriceClassRefID = GetPriceClassId(serv, dataList, priceClassList)
                };

                journeyPriceClassList.Add(journeyPriceClassType);
            }

            journeyOverviewType.JourneyPriceClass = [.. journeyPriceClassList];

            return journeyOverviewType;
        }

        private static NewOfferItemType[] GetOfferItems(OfferDetails offer, DataList dataList, EAPI.Domain.Virgin.Models.Common.Parties parties, IGenericDataMapper genericDataMapper, List<PriceClassType> priceClassList, List<PaxSegmentType> paxSegmentList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, List<PaxJourneyType> paxJourneyList, List<OriginDestType> originDestList, ServiceId serviceIdCount ,string fareCodeSubType, string fareTypeCode)
        {
            var offerItemTypes = new List<NewOfferItemType>();
            var accountCode = parties?.Participants?.First().AccountCode;
            var baseCurrency = dataList.Penalty?.FirstOrDefault()?.MaxPrice.Amount.BaseCurrency ?? offer.SalePrice.First().Amount.BaseCurrency;
            var currencyDecimalFormat = DecimalHelper.GetDecimalFormat(dataList.Penalty?.FirstOrDefault()?.MaxPrice.Amount.MonetaryAmount ?? offer.SalePrice.First().Amount.MonetaryAmount);

            foreach (var offerItem in offer.OfferItems)
            {
                var fareDetailTypes = new List<FareDetailType>();
                var offerItemType = new NewOfferItemType()
                {
                    OfferItemID = offerItem.OfferItemId,
                    MandatoryInd = true,
                    MandatoryIndSpecified = true,
                    Price = GetPrice(offerItem.OfferitemPricing.First().SalePrice),
                    PaymentTimeLimit = new PaymentTimeLimitType()
                    {
                        Item = new PaymentTimeLimitDateType()
                        {
                            PaymentTimeLimitDateTime = Convert.ToDateTime(offer.TicketingTimeLimit).ToUniversalTime(),
                            PaymentTimeLimitDateTimeSpecified = true
                        }
                    },
                    Service = GetServices(offerItem.Services, dataList, genericDataMapper, fareDetailTypes, baseCurrency, accountCode, priceClassList, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, paxJourneyList, originDestList, currencyDecimalFormat, serviceIdCount, fareCodeSubType, fareTypeCode),
                    FareDetail = fareDetailTypes.ToArray(),
                    AirlineTaxonomy = offerItem.AirlineTaxonomies != null && offerItem.AirlineTaxonomies.Any() ? GetAirlineTaxonomies(offerItem.AirlineTaxonomies) : null
                };

                offerItemTypes.Add(offerItemType);
            }

            return offerItemTypes.ToArray();
        }

        private static OfferedServiceType[] GetServices(IEnumerable<OfferService> services, DataList dataList, IGenericDataMapper genericDataMapper, List<FareDetailType> fareDetailTypes, string baseCurrency, string accountCode, List<PriceClassType> priceClassList, List<PaxSegmentType> paxSegmentList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, List<PaxJourneyType> paxJourneyList, List<OriginDestType> originDestList, string currencyDecimalFormat, ServiceId serviceIdCount, string fareCodeSubType, string fareTypeCode)
        {
            List<string> journeyIDs = new();
            List<string> paxIDs = new();
            List<OfferedServiceType> OfferedServiceList = new();
            List<FareComponentType> fareComponents = new();

            foreach (var service in services)
            {
                GetFareComponentsV2(dataList, genericDataMapper, priceClassList, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, baseCurrency, service, fareComponents, accountCode, currencyDecimalFormat, fareCodeSubType, fareTypeCode);

                if (paxIDs.Count == 0)
                {
                    paxIDs.AddRange(service.ServiceBeneficiaries);
                }                

                journeyIDs.Add(GetPaxJourneyId(service, dataList, genericDataMapper, paxJourneyList, paxSegmentList, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList, originDestList));
            }

            ///Populate fareDetailTypes section of NDC Offer Item
            fareDetailTypes.Add(new FareDetailType()
            {
                FareComponent = fareComponents.ToArray(),
                PaxRefID = paxIDs.ToArray()
            });

            //Populate Services section of NDC offer response
            GetOfferService(journeyIDs, paxIDs, OfferedServiceList, serviceIdCount);

            return OfferedServiceList.ToArray();
        }

        private static void GetOfferService(IEnumerable<string> journeyIDs, IEnumerable<string> paxIDs, List<OfferedServiceType> OfferedServiceList, ServiceId serviceIdCount)
        {
            OfferedServiceList.Add(new()
            {
                OfferServiceAssociation = new()
                {
                    Item = new PaxJourneyRefType()
                    {
                        PaxJourneyRefID = journeyIDs.Distinct().ToArray()
                    }
                },
                ServiceID = IdGenerator.CreateServiceID(++serviceIdCount.Id),
                PaxRefID = paxIDs.ToArray()
            });
        }

        private static void GetFareComponentsV2(DataList dataList, IGenericDataMapper genericDataMapper, List<PriceClassType> PriceClassList, List<PaxSegmentType> PaxSegmentList, List<DatedMarketingSegmentType> DatedMarketingSegmentList, List<DatedOperatingSegmentType> DatedOperatingSegmentList, List<DatedOperatingLegType> DatedOperatingLegList, string baseCurrency, OfferService service, List<FareComponentType> fareComponents, string accountCode, string currencyDecimalFormat, string fareCodeSubType, string fareTypeCode)
        {
            fareComponents.Add(new()
            {
                CancelRestrictions = GetCancelRestrictionsTypes(service, dataList, baseCurrency, currencyDecimalFormat),
                ChangeRestrictions = GetChangeRestrictionsTypes(service, dataList, baseCurrency, currencyDecimalFormat),
                FareBasisCode = dataList.PriceClass.First(x => x.Id == service.PriceClassRefIds.FirstOrDefault()).FareBasisCode,
                NegotiatedCode = fareTypeCode,
                FareTypeCode =  fareCodeSubType != null ? fareCodeSubType : null,
                PaxSegmentRefID = GetPaxSegmentRefIDs(service.ProductSegmentAssociation.OfferSegmentRefIds, genericDataMapper, dataList, PaxSegmentList, DatedMarketingSegmentList, DatedOperatingSegmentList, DatedOperatingLegList),
                PriceClassRefID = GetPriceClassId(service, dataList, PriceClassList)
            });
        }

        private static PriceType GetPrice(IEnumerable<Price> salePrice)
        {
            var priceDictionary = salePrice.ToDictionary(p => p.PriceComponentType);
            var baseFareAmount = priceDictionary[PriceComponentTypes.BaseFareEnum].Amount;
            var basePriceAmount = priceDictionary[PriceComponentTypes.BasePriceEnum].Amount;
            var totalTaxAmount = priceDictionary[PriceComponentTypes.TotalTaxEnum].Amount;

            return new()
            {
                BaseAmount = new AmountType()
                {
                    CurCode = baseFareAmount.BaseCurrency,
                    Value = Convert.ToDecimal(baseFareAmount.MonetaryAmount)
                },
                TotalAmount = new AmountType()
                {
                    CurCode = basePriceAmount.BaseCurrency,
                    Value = Convert.ToDecimal(basePriceAmount.MonetaryAmount)
                },
                TaxSummary = new List<TaxSummaryType>()
                {
                    new()
                    {
                        TotalTaxAmount = new()
                        {
                            CurCode = totalTaxAmount.BaseCurrency,
                            Value = Convert.ToDecimal(totalTaxAmount.MonetaryAmount)
                        }
                    }
                }.ToArray()
            };
        }

        private static string GetPriceClassId(OfferService service, DataList dataList, List<PriceClassType> priceClassList)
        {
            var priceClass = dataList.PriceClass.First(p => p.Id == service.PriceClassRefIds.FirstOrDefault());
            if (priceClassList.Any(x => x.Name == priceClass.ProductName))
            {
                return priceClassList.First(x => x.Name == priceClass.ProductName).PriceClassID;
            }
            else
            {
                var cabinType = new CabinTypeType
                {
                    CabinTypeCode = priceClass.CabinType?.CabinCode,
                    CabinTypeName = priceClass.CabinType?.CabinName
                };

                var priceClassType = new PriceClassType()
                {
                    PriceClassID = IdGenerator.CreatePriceClassID(priceClassList.Count),
                    Name = priceClass.ProductName,
                    CabinType = [cabinType]
                };

                priceClassList.Add(priceClassType);
                return priceClassType.PriceClassID;
            }
        }

        private static string[] GetPaxSegmentRefIDs(IEnumerable<string> offerSegmentRefIds, IGenericDataMapper genericDataMapper, DataList dataList, List<PaxSegmentType> PaxSegmentList, List<DatedMarketingSegmentType> DatedMarketingSegmentList, List<DatedOperatingSegmentType> DatedOperatingSegmentList, List<DatedOperatingLegType> DatedOperatingLegList)
        {
            List<string> paxSegIDs = new();
            foreach (var oSegID in offerSegmentRefIds)
            {
                var offerSeg = dataList.OfferSegments.First(seg => seg.OfferSegmentId == oSegID);
                var flightSegment = dataList.FlightSegments.First(seg => seg.SegmentId == offerSeg.FlightSegmentRefId);
                paxSegIDs.Add(GetPaxSegmentId(genericDataMapper, offerSeg, flightSegment, PaxSegmentList, DatedMarketingSegmentList, DatedOperatingSegmentList, DatedOperatingLegList));
            }
            return paxSegIDs.ToArray();
        }

        private static string GetPaxSegmentId(IGenericDataMapper genericDataMapper, OfferSegment offerFlightSegment, Segment flightSegment, List<PaxSegmentType> paxSegmentList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList)
        {
            var datedMarketingSegmentId = CreateDatedMarketingSegmentType(flightSegment, genericDataMapper, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList);

            if (paxSegmentList != null)
            {
                var cabinType = offerFlightSegment.CabinType;
                var matchingSegment = paxSegmentList.FirstOrDefault(seg => seg.MarketingCarrierRBD_Code == offerFlightSegment.ClassOfService && seg.CabinTypeAssociationChoice?.Item is CabinTypeType cabin && cabin.CabinTypeName == cabinType.CabinName && cabin.CabinTypeCode == cabinType.CabinCode && seg.DatedMarketingSegmentRefId == datedMarketingSegmentId);

                if (matchingSegment != null)
                {
                    return matchingSegment.PaxSegmentID;
                }
            }

            var newPaxSegment = CreatePaxSegmentType(offerFlightSegment, paxSegmentList, datedMarketingSegmentId);
            paxSegmentList?.Add(newPaxSegment);

            return newPaxSegment.PaxSegmentID;
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
                var datedMarketingSegmentType = new DatedMarketingSegmentType();

                datedMarketingSegmentType.Arrival = new TransportPointType
                {
                    IATA_LocationCode = flightSegment.DestinationAirportCode,
                    AircraftScheduledDateTime = scheduledArrivalTime,
                    AircraftScheduledDateTimeSpecified = true
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
            var flightDuration = TimeSpan.Parse(flightSegment.FlightDuration, CultureInfo.InvariantCulture);

            var datedOperatingSegmentType = new DatedOperatingSegmentType
            {
                DatedOperatingSegmentId = IdGenerator.CreateDatedOperatingSegmentID(count),
                CarrierName = string.IsNullOrEmpty(flightSegment.OperatingCarrier) ? genericDataMapper.GetOperatingCarrierNameByCode(flightSegment.MarketingCarrier) : genericDataMapper.GetOperatingCarrierNameByCode(flightSegment.OperatingCarrier),
                CarrierDesigCode = string.IsNullOrEmpty(flightSegment.OperatingCarrier) ? flightSegment.MarketingCarrier : flightSegment.OperatingCarrier,
                OperatingCarrierFlightNumberText = string.IsNullOrEmpty(flightSegment.OperatingFlightNumber) ? flightSegment.MarketingFlightNumber : flightSegment.OperatingFlightNumber,
                Duration = $"{Constants.Application.PT}{(flightDuration.Days * 24) + flightDuration.Hours}{Constants.Application.H}{flightDuration.Minutes}{Constants.Application.M}{flightDuration.Seconds}{Constants.Application.S}",
                DatedOperatingLegRefID = flightSegment.Legs.Select(legRef => CreateDatedOperatingLegType(flightSegment?.AirCraft, legRef, datedOperatingLegList)).ToArray(),
                SecureFlightInd = Constants.Application.SecureFlightDefaultValue,
                SecureFlightIndSpecified = Constants.Application.SecureFlightDefaultValue
            };

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
            var penalty = GetPenalties(offerService, dataList);

            var fareBasisCode = dataList.PriceClass.First(pC => pC.Id == offerService.PriceClassRefIds.FirstOrDefault()).FareBasisCode;
            var flightsegments = GetFlightSegments(offerService, dataList);

            var originBoundAirport = flightsegments.First().OriginAirportCode;
            var destinationBoundAirport = flightsegments.Last().DestinationAirportCode;
            var boundDetails = $"{originBoundAirport}-{destinationBoundAirport}";

            if (penalty.Any())
            {
                var nonRefundable = penalty.Any(pen => pen.Text == "BEFORE-DEPARTURE") && penalty.Any(pen => pen.Text == "AFTER-DEPARTURE");

                foreach (var oIP in penalty)
                {
                    if (oIP?.MaxPrice != null)
                    {
                        boundDetails = GetBoundDetails(fareBasisCode, nonRefundable, originBoundAirport, destinationBoundAirport);
                        var allowedModificationInd = !nonRefundable;

                        AddCancelRestriction(cancelRestrictions, boundDetails, oIP, allowedModificationInd);
                    }
                    else if (!string.IsNullOrEmpty(fareBasisCode))
                    {
                        boundDetails = GetPenaltyBoundDetails(fareBasisCode, originBoundAirport, destinationBoundAirport);
                        var allowedModificationInd = false;

                        AddCancelRestriction(cancelRestrictions, boundDetails, oIP, allowedModificationInd);
                    }
                }
            }
            else
            {
                AddNonPenaltyCancelRestrictions(offerService, dataList, baseCurrency, currencyDecimalFormat, cancelRestrictions, originBoundAirport, destinationBoundAirport);
            }

            return cancelRestrictions.ToArray();
        }

        private static void AddCancelRestriction(List<CancelRestrictionsType> cancelRestrictions, string boundDetails, Penalty oIP, bool allowedModificationInd)
        {
            var cancelRestriction = GetCancelRestrict(oIP, boundDetails, allowedModificationInd, true, true);
            if (!CheckCancelRestrictionExists(cancelRestrictions, cancelRestriction))
            {
                cancelRestrictions.Add(cancelRestriction);
            }
        }

        private static void AddNonPenaltyCancelRestrictions(OfferService offerService, DataList dataList, string baseCurrency, string currencyDecimalFormat, List<CancelRestrictionsType> cancelRestrictions, string originBoundAirport, string destinationBoundAirport)
        {
            var priceClassRef = offerService?.PriceClassRefIds.First();
            foreach (var pc in dataList?.PriceClass!)
            {
                if (pc?.Id == priceClassRef && pc!.ProductName.Contains("Flex"))
                {
                    string boundOriginDest = $"{originBoundAirport}-{destinationBoundAirport}";
                    CancelRestrictionsType[] cancelRestriction = GetCancelRestrictFlex(boundOriginDest, true, true, true, baseCurrency, currencyDecimalFormat);
                    cancelRestrictions.AddRange(cancelRestriction);
                }
            }
        }

        private static string GetPenaltyBoundDetails(string fareBasisCode, string originBoundAirport, string destinationBoundAirport)
        {
            if (fareBasisCode!.StartsWith('Y') || fareBasisCode.StartsWith('W') || fareBasisCode.StartsWith('J'))
            {
                return $"{Constants.Application.PenaltyFreeCANCELLATIONS} - {originBoundAirport}-{destinationBoundAirport}";
            }

            return $"{Constants.Application.PenaltyNotFreeCANCELLATIONS} - {originBoundAirport}-{destinationBoundAirport}";
        }

        private static string GetBoundDetails(string fareBasisCode, bool nonRefundable, string originBoundAirport, string destinationBoundAirport)
        {
            if (fareBasisCode!.StartsWith('Y') || fareBasisCode.StartsWith('W') || fareBasisCode.StartsWith('J'))
            {
                return $"{Constants.Application.PenaltyFreeCANCELLATIONS} - {originBoundAirport}-{destinationBoundAirport}";
            }
            if (nonRefundable)
            {
                return $"{Constants.Application.PenaltyNotFreeCANCELLATIONS} - {originBoundAirport}-{destinationBoundAirport}";
            }

            return $"{originBoundAirport}-{destinationBoundAirport}";
        }

        private static IEnumerable<Segment> GetFlightSegments(OfferService offerService, DataList dataList)
        {
            return offerService.ProductSegmentAssociation.OfferSegmentRefIds.Select(x => dataList.FlightSegments.First(z => z.SegmentId == dataList.OfferSegments.First(y => y.OfferSegmentId == x).FlightSegmentRefId));
        }

        private static IEnumerable<Penalty> GetPenalties(OfferService offerService, DataList dataList)
        {
            return offerService?.PenaltyRef?.ChangePenaltyRefIds?.Select(penRefId => dataList.Penalty.First(pen => pen.Id == penRefId)) ?? [];
        }

        private static CancelRestrictionsType GetCancelRestrict(Penalty oIP, string boundDetails, bool allowedModificationInd, bool allowedModificationIndSpecified, bool journeyStageCodeSpecified)
        {
            if (allowedModificationInd)
            {
                return new CancelRestrictionsType
                {
                    AllowedModificationInd = allowedModificationInd,
                    AllowedModificationIndSpecified = allowedModificationIndSpecified,
                    DescText = boundDetails,
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
                    DescText = boundDetails,
                    Fee = null,
                    JourneyStageCodeSpecified = false
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
                new CancelRestrictionsType
                {
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
                new CancelRestrictionsType
                {
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

        private static ChangeRestrictionsType[] GetChangeRestrictionsTypes(OfferService offerService, DataList dataList, string baseCurrency, string currencyDecimalFormat)
        {
            var changeRestrictions = new List<ChangeRestrictionsType>();
            var penaltyList = offerService?.PenaltyRef?.ChangePenaltyRefIds?.Select(panRefId => dataList.Penalty.First(pen => pen.Id == panRefId)).ToList() ?? new List<Penalty>();

            var fareBasisCode = dataList.PriceClass.First(pen => pen.Id == offerService.PriceClassRefIds.FirstOrDefault()).FareBasisCode;
            var flightSegments = offerService.ProductSegmentAssociation.OfferSegmentRefIds.Select(oSeg => dataList.FlightSegments.First(fSeg => fSeg.SegmentId == dataList.OfferSegments.First(y => y.OfferSegmentId == oSeg).FlightSegmentRefId)).ToList();

            var originBoundAirport = flightSegments.First().OriginAirportCode;
            var destinationBoundAirport = flightSegments.Last().DestinationAirportCode;

            if (penaltyList.Count > 0)
            {
                ProcessPenalties(penaltyList, fareBasisCode, originBoundAirport, destinationBoundAirport, changeRestrictions, baseCurrency, currencyDecimalFormat);
            }
            else
            {
                AddFlexChangeRestrictions(offerService, dataList, baseCurrency, currencyDecimalFormat, changeRestrictions, originBoundAirport, destinationBoundAirport);
            }

            return changeRestrictions.ToArray();
        }

        private static void ProcessPenalties(List<Penalty> penaltyList, string fareBasisCode, string originBoundAirport, string destinationBoundAirport, List<ChangeRestrictionsType> changeRestrictions, string baseCurrency, string currencyDecimalFormat)
        {
            foreach (var oIP in penaltyList)
            {
                string boundDetails = $"{originBoundAirport}-{destinationBoundAirport}";
                var allowedModificationInd = oIP?.MaxPrice != null;

                if (fareBasisCode.StartsWith('Y') || fareBasisCode.StartsWith('W') || fareBasisCode.StartsWith('J'))
                {
                    boundDetails = $"{Constants.Application.PenaltyFreeCHANGES}-{originBoundAirport}-{destinationBoundAirport}";
                }

                var changeRestriction = GetChangeRestrict(oIP, boundDetails, allowedModificationInd, true, true, baseCurrency, currencyDecimalFormat);
                if (!CheckChangeRestrictionExists(allowedModificationInd, changeRestrictions, changeRestriction))
                {
                    changeRestrictions.Add(changeRestriction);
                }
            }
        }

        private static void AddFlexChangeRestrictions(OfferService offerService, DataList dataList, string baseCurrency, string currencyDecimalFormat, List<ChangeRestrictionsType> changeRestrictions, string originBoundAirport, string destinationBoundAirport)
        {
            var priceClassRef = offerService?.PriceClassRefIds.First();
            foreach (var pc in dataList?.PriceClass!)
            {
                if (pc?.Id == priceClassRef && pc.ProductName.Contains("Flex"))
                {
                    var boundOriginDest = $"{originBoundAirport}-{destinationBoundAirport}";
                    var changeRestriction = GetChangeRestrictFlex(boundOriginDest, true, true, true, baseCurrency, currencyDecimalFormat);
                    changeRestrictions.AddRange(changeRestriction);
                }
            }
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
                    new ChangeRestrictionsType
            {
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
            new ChangeRestrictionsType
                {
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

        private static PriceCalendarType[] GetPriceCalendarOffers(CalendarSearchResponse flexiDateSearchResponse, OfferResponse offerResponse)
        {
            var priceCalendarOfferList = new List<PriceCalendarType>();
            if (flexiDateSearchResponse != null)
            {
                var cells = flexiDateSearchResponse.FlexibleDateCalendar.DepartureDetails.SelectMany(d => d.FlexibleDatePriceCalendarCell);
                foreach (var cell in cells)
                {
                    var priceCalendarOffer = new PriceCalendarType
                    {
                        PriceCalendarDate = GetPriceCalendarDates(cell.OriginDestinations, offerResponse.DataList.OriginDestinationAssociations),
                        PricedPTC = GetPricedPTC(cell.PriceItems, offerResponse.DataList.CustomerDetails),
                        TotalPriceAmount = new AmountType() { CurCode = cell.SalePrice.First(sale => sale.PriceComponentType.Value == PriceComponentTypes.BasePriceEnum).Amount.BaseCurrency, Value = Convert.ToDecimal(cell.SalePrice.First(sale => sale.PriceComponentType.Value == PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount) }
                    };
                    priceCalendarOfferList.Add(priceCalendarOffer);
                }
                return priceCalendarOfferList.ToArray();
            }
            else
            {
                return null;
            }
        }

        private static PriceCalendarDateType[] GetPriceCalendarDates(IEnumerable<OriginDestination> originDest, IEnumerable<OriginDestinationAssociation> originDestinationAssociations)
        {
            return originDest.Select(ond => new PriceCalendarDateType()
            {
                Date = Convert.ToDateTime(ond.DepartureDate),
                OriginDestRefID = originDestinationAssociations.First(org => org.OriginDestination.DestinationAirport == ond.DestinationAirport && org.OriginDestination.OriginAirport == ond.OriginAirport).OdLabel
            }).ToArray();
        }

        private static PTC_OfferParametersType[] GetPricedPTC(IEnumerable<PriceItems> priceItems, IEnumerable<Customer> customerDetails)
        {
            List<PTC_OfferParametersType> pricedPTC = new();
            PTC_OfferParametersType pTCPrices = new()
            {
                PaxRefID = priceItems.Select(price => customerDetails.First(customer => customer.Ptc == price.Ptc).CustId).ToArray()
            };
            pricedPTC.Add(pTCPrices);
            return pricedPTC.ToArray();
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
                    if (baggage?.FreeBaggageAllowance?.DimensionAllowance != null)
                    {
                        baggageAllowance.MaximumDimensionAllowance = baggage?.FreeBaggageAllowance?.DimensionAllowance!.Select(da =>
                        new DimensionAllowanceType
                        {
                            LengthUnitOfMeasurement = (Domain.RequestResponseModels.AirShoppingRS.LengthUnitCodeType)da.LengthUnitOfMeasurement,
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
                    if (baggage?.FreeBaggageAllowance?.WeightAllowance != null)
                    {
                        baggageAllowance.WeightAllowance = baggage?.FreeBaggageAllowance?.WeightAllowance!.Select(wa => new WeightAllowanceType
                        {
                            MaximumWeightMeasure = Convert.ToDecimal(wa.MaximumWeightMeasure),
                            WeightUnitOfMeasurement = (Domain.RequestResponseModels.AirShoppingRS.WeightUnitCodeType)wa.WeightUnitOfMeasurement,
                            MaximumWeightMeasureSpecified = true,
                            TotalMaximumWeightMeasureSpecified = true,
                            TotalMaximumWeightMeasure = baggageAllowance.PieceAllowance.TotalQty * Convert.ToDecimal(wa.MaximumWeightMeasure)
                        }).ToArray();
                    }

                    if(baggage?.FreeBaggageAllowance?.BagDescriptors?.FirstOrDefault()?.BagDescriptorName.Contains(Constants.Application.CarryOnBaggageText) == true)
                    {
                        baggageAllowance.DescText = new[] { baggage?.FreeBaggageAllowance?.BagDescriptors?.FirstOrDefault()?.BagDescriptorName };
                    }
                }

                if (baggageAllowance.TypeCode == "Checked")
                {
                    if (baggage?.FreeBaggageAllowance?.DimensionAllowance != null)
                    {
                        baggageAllowance.MaximumDimensionAllowance = baggage?.FreeBaggageAllowance?.DimensionAllowance!.Select(da =>
                        new DimensionAllowanceType
                        {
                            LengthUnitOfMeasurement = (Domain.RequestResponseModels.AirShoppingRS.LengthUnitCodeType)da.LengthUnitOfMeasurement,
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

                    if (baggage?.FreeBaggageAllowance?.WeightAllowance != null)
                    {
                        List<WeightAllowanceType> weightAllowances = new();
                        var weightAllowanceInKGM = baggage?.FreeBaggageAllowance?.WeightAllowance!.FirstOrDefault(wa => wa.WeightUnitOfMeasurement == EAPI.Domain.Virgin.Models.Products.Baggage.WeightUnitCodeType.KGM);
                        var weightAllowanceInLBR = baggage?.FreeBaggageAllowance?.WeightAllowance!.FirstOrDefault(wa => wa.WeightUnitOfMeasurement == EAPI.Domain.Virgin.Models.Products.Baggage.WeightUnitCodeType.LBR);

                        if (weightAllowanceInKGM != null)
                        {
                            weightAllowances.Add(new WeightAllowanceType
                            {
                                MaximumWeightMeasure = Convert.ToDecimal(weightAllowanceInKGM.MaximumWeightMeasure),
                                WeightUnitOfMeasurement = (Domain.RequestResponseModels.AirShoppingRS.WeightUnitCodeType)weightAllowanceInKGM.WeightUnitOfMeasurement,
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
                                WeightUnitOfMeasurement = (Domain.RequestResponseModels.AirShoppingRS.WeightUnitCodeType)weightAllowanceInLBR.WeightUnitOfMeasurement,
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
                                                && bal?.PieceAllowance?.TotalQty == baggage.FreeBaggageAllowance?.AllowedBagCnt
                                                && bal?.MaximumDimensionAllowance?.FirstOrDefault()?.LengthMeasure == Convert.ToDecimal(baggage.FreeBaggageAllowance?.DimensionAllowance?.FirstOrDefault()?.LengthMeasure)
                                                && bal?.MaximumDimensionAllowance?.FirstOrDefault()?.LengthUnitOfMeasurement.ToString() == baggage.FreeBaggageAllowance?.DimensionAllowance?.FirstOrDefault()?.LengthUnitOfMeasurement.ToString()
                                                && bal?.WeightAllowance?.FirstOrDefault()?.MaximumWeightMeasure == Convert.ToDecimal(baggage.FreeBaggageAllowance?.WeightAllowance?.FirstOrDefault()?.MaximumWeightMeasure)
                                                && bal?.WeightAllowance?.FirstOrDefault()?.WeightUnitOfMeasurement.ToString() == baggage.FreeBaggageAllowance?.WeightAllowance?.FirstOrDefault()?.WeightUnitOfMeasurement.ToString());


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

        private static PaxType[] GetPaxDetails(IEnumerable<Customer> customers)
        {
            var paxList = customers.Select(customer => new PaxType()
            {
                PaxID = customer.CustId,
                PTC = customer.Ptc == Constants.Application.CNNPTCCode ? Constants.Application.ChildPTCCode : customer.Ptc,
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

        private static string GetPaxJourneyId(OfferService offerService, DataList dataList, IGenericDataMapper genericDataMapper, List<PaxJourneyType> paxJourneyList, List<PaxSegmentType> paxSegmentList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList, List<OriginDestType> originDestList)
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

        private static AirlineTaxonomyType[] GetAirlineTaxonomies(IEnumerable<AirlineTaxonomy> airlineTaxonomies)
        {
            return airlineTaxonomies.Select(taxonomy => new AirlineTaxonomyType { TaxonomyCode = taxonomy.TaxonomyCode }).ToArray();
        }
    }
}
