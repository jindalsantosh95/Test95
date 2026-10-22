using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Baggage;
using VAA.EAPI.NDC.Order.Infrastructure.Model.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Mapper.BrandMS
{
    public static class BrandMSResponseMapper
    {
        public static OfferResponse UpdateOfferDoc(IEnumerable<BrandRS> brandRS, OfferDetails offerDetails, OfferResponse offerDoc, ref int penaltyIndex, ref int baggageCounter, ref int serviceCounter, [Optional] bool isOrderChange)
        {
            if (!brandRS.Any(x => (bool)(x.Errors?.Any())))
            {
                List<ProductComponent> productComponents = new();
                int checkedBagCounter = 1;
                IEnumerable<BaggageAllowances> baggageList = null;
                List<Penalty> penaltyList = new();

                if (!isOrderChange)
                {
                    offerDoc.DataList.Penalty = null;
                }

                offerDoc.DataList.BaggageAllowances = null;

                var fareItems = brandRS
                  .SelectMany(bs => bs.Itineraries)
                  .SelectMany(itinerary => itinerary.Legs.SelectMany(leg => leg.Fares.Select(fare => new
                  {
                      Fare = fare,
                      PassengerTypeCode = itinerary.PassengerTypeCodes?.FirstOrDefault() 
                  }))).OrderBy(x => Array.IndexOf(["ADT", "YTH", "CNN", "INF"], x.PassengerTypeCode));

                foreach (var fareItem in fareItems)
                {
                    var fare = fareItem.Fare;
                    var ptc = fareItem.PassengerTypeCode;

                    foreach (ChangePolicy change in fare.Fees?.Change!)
                    {
                        if (!penaltyList.Any(p => p.Type == Constants.Application.ChangePenaltyType && p.MaxPrice.Amount.MonetaryAmount == change.Fee?.Amount?.ToString() && p.MinPrice.Amount.MonetaryAmount == change.Fee?.Amount?.ToString() && p.Text == change.Description))
                        {
                            penaltyList.Add(new Penalty()
                            {
                                Id = $"PEN{penaltyIndex}",
                                Type = Constants.Application.ChangePenaltyType,
                                MaxPrice = new MaxPrice()
                                {
                                    Amount = new CurrencyAmount()
                                    {
                                        MonetaryAmount = change.Fee?.Amount?.ToString(),
                                        BaseCurrency = change.Fee?.Currency?.ToString(),
                                    }
                                },
                                MinPrice = new MinPrice()
                                {
                                    Amount = new CurrencyAmount()
                                    {
                                        MonetaryAmount = change.Fee?.Amount?.ToString(),
                                        BaseCurrency = change.Fee?.Currency?.ToString(),
                                    }
                                },
                                Text = change.Description
                            });

                            penaltyIndex++;
                        }
                    }


                    foreach (CancelPolicy cancel in fare.Fees?.Cancellation!)
                    {
                        if (!penaltyList.Any(p => p.Type == Constants.Application.CancellationPenaltyType && p.MaxPrice.Amount.MonetaryAmount == cancel.Fee?.Amount?.ToString() && p.MinPrice.Amount.MonetaryAmount == cancel.Fee?.Amount?.ToString() && p.Text == cancel.Description))
                        {
                            penaltyList.Add(new Penalty()
                            {
                                Id = $"PEN{penaltyIndex}",
                                Type = Constants.Application.CancellationPenaltyType,
                                MaxPrice = new MaxPrice()
                                {
                                    Amount = new CurrencyAmount()
                                    {
                                        MonetaryAmount = cancel.Fee?.Amount?.ToString(),
                                        BaseCurrency = cancel.Fee?.Currency?.ToString()
                                    }
                                },
                                MinPrice = new MinPrice()
                                {
                                    Amount = new CurrencyAmount()
                                    {
                                        MonetaryAmount = cancel.Fee?.Amount?.ToString(),
                                        BaseCurrency = cancel.Fee?.Currency?.ToString()
                                    }
                                },
                                Text = cancel.Description
                            });
                            penaltyIndex++;
                        }
                    }

                    if (offerDoc.DataList.Penalty == null)
                        offerDoc.DataList.Penalty = penaltyList;
                    else
                        offerDoc.DataList.Penalty = offerDoc.DataList.Penalty.Concat(penaltyList).Distinct();

                    foreach (Segment segment in fare.Segments)
                    {
                        var point2pointVS = !fare.Segments.Any(x => x.CarryOnBaggage.Any(cb => cb.FreeBags.Carrier != "VS"));

                        var services = from o in offerDoc.Offers
                                        from oi in o.OfferItems
                                        from s in oi.Services
                                        from cust in offerDoc.DataList.CustomerDetails.Where(c => ptc == null || c.Ptc.Is(GetPtc(ptc)))
                                        where s.ServiceBeneficiaries.Contains(cust.CustId)
                                        from osegId in s.ProductSegmentAssociation.OfferSegmentRefIds
                                        join oseg in offerDoc.DataList.OfferSegments
                                        on osegId equals oseg.OfferSegmentId
                                        join fseg in offerDoc.DataList.FlightSegments
                                        on oseg.FlightSegmentRefId equals fseg.SegmentId
                                        from leg in fseg.Legs
                                        where o.OfferId == offerDetails.OfferId && segment.Origin == leg.OriginAirportCode && segment.Destination == leg.DestinationAirportCode
                                        select s;
                            

                        foreach (OfferService service in services)
                        {
                            if (isOrderChange)
                            {
                                service.PenaltyRef = service.PenaltyRef == null ? new PenaltyRef() : service.PenaltyRef;
                                service.PenaltyRef.ChangePenaltyRefIds = service.PenaltyRef.ChangePenaltyRefIds == null ? new() : service.PenaltyRef.ChangePenaltyRefIds;
                                service.PenaltyRef.CancellationPenaltyRefIds = service.PenaltyRef.CancellationPenaltyRefIds == null ? new() : service.PenaltyRef.CancellationPenaltyRefIds;

                                service.PenaltyRef.ChangePenaltyRefIds.AddRange(penaltyList.Where(x => x.Type == Constants.Application.ChangePenaltyType).Select(index => index.Id).Where(id => !service.PenaltyRef.ChangePenaltyRefIds.Contains(id)).ToList());
                                service.PenaltyRef.CancellationPenaltyRefIds.AddRange(penaltyList.Where(x => x.Type == Constants.Application.CancellationPenaltyType).Select(index => index.Id).Where(id => !service.PenaltyRef.CancellationPenaltyRefIds.Contains(id)).ToList());
                            }
                            else
                            {
                                service.PenaltyRef = new()
                                {
                                    CancellationPenaltyRefIds = new(),
                                    ChangePenaltyRefIds = new()
                                };
                                service.PenaltyRef.ChangePenaltyRefIds.AddRange(penaltyList.Where(x => x.Type == Constants.Application.ChangePenaltyType).Select(index => index.Id).ToList());
                                service.PenaltyRef.CancellationPenaltyRefIds.AddRange(penaltyList.Where(x => x.Type == Constants.Application.CancellationPenaltyType).Select(index => index.Id).ToList());
                            }

                            var selectedSegment = (from osegId in service.ProductSegmentAssociation.OfferSegmentRefIds
                                                   join oseg in offerDoc.DataList.OfferSegments
                                                    on osegId equals oseg.OfferSegmentId
                                                   join fseg in offerDoc.DataList.FlightSegments
                                                    on oseg.FlightSegmentRefId equals fseg.SegmentId
                                                   from leg in fseg.Legs
                                                   where segment.Origin == leg.OriginAirportCode && segment.Destination == leg.DestinationAirportCode
                                                   select segment).FirstOrDefault();

                            if (selectedSegment != null)
                            {
                                // Map Brand Name
                                if (service.PriceClassRefIds.Count() > 0)
                                {
                                    foreach (string penalty in service.PriceClassRefIds)
                                    {
                                        offerDoc.DataList.PriceClass.First(p => p.Id == penalty).ProductName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase((fare?.Product ?? string.Empty).ToLower());
                                    }
                                }

                                //Map ServiceDefList
                                if (selectedSegment.Amenities != null)
                                {
                                    var serviceList = new List<ProductComponent>();

                                    var amenities = selectedSegment.Amenities;

                                    var amenityDisplayTexts = new[]
                                    {
                                    amenities?.Aircraft?.DisplayText,
                                    amenities?.Beverage?.DisplayText,
                                    amenities?.Entertainment?.DisplayText,
                                    amenities?.FreshFood?.DisplayText,
                                    amenities?.Layout?.DisplayText,
                                    amenities?.Power?.DisplayText,
                                    amenities?.Seats?.DisplayText,
                                    amenities?.Wifi?.DisplayText
                                };

                                    foreach (var displayText in amenityDisplayTexts.Where(dt => !string.IsNullOrWhiteSpace(dt)))
                                    {
                                        var productComp = productComponents.FirstOrDefault(p => p.Description == displayText);
                                        if (productComp == null)
                                        {
                                            productComp = new ProductComponent()
                                            {
                                                Id = $"{Constants.Application.SV}{serviceCounter++}",
                                                Description = displayText
                                            };
                                            productComponents.Add(productComp);
                                        }

                                        serviceList.Add(productComp);
                                    }

                                    if (serviceList.Count > 0)
                                    {
                                        var fSegNumber = (offerDoc.DataList.FlightSegments.FirstOrDefault(x => x.DestinationAirportCode == selectedSegment.Destination && x.OriginAirportCode == selectedSegment.Origin)?.SegmentId)
                                                         ?? (offerDoc.DataList.FlightSegments.FirstOrDefault(x => x.Legs.Any(l => l.OriginAirportCode == selectedSegment.Origin && l.DestinationAirportCode == selectedSegment.Destination))?.SegmentId);
                                        var offerSeg = offerDoc.DataList.OfferSegments.First(oSeg => oSeg.FlightSegmentRefId == fSegNumber);
                                        offerSeg.ProductComponentRefIds = serviceList.Select(sL => sL.Id).ToList();
                                    }
                                }

                                var baggageRefIds = GetBaggageAllowances(ref checkedBagCounter, ref baggageList, fare, selectedSegment, point2pointVS);

                                service.BaggageRefIds = baggageRefIds;
                                service.ProductSegmentAssociation.OfferSegmentRefIds.ForEach(refId =>
                                {
                                    var legData = offerDoc.DataList.FlightSegments.First(fs => fs.SegmentId == offerDoc.DataList.OfferSegments.First(os => os.OfferSegmentId == refId).FlightSegmentRefId).Legs.Where(l => l.OriginAirportCode == selectedSegment.Origin && l.DestinationAirportCode == selectedSegment.Destination);

                                    foreach (var leg in legData)
                                    {
                                        leg.CarrierAircraftTypeCode = selectedSegment?.Amenities?.Aircraft?.Model;
                                        leg.CarrierAircraftTypeName = selectedSegment?.Amenities?.Aircraft?.DisplayText;
                                    }
                                });
                            }

                        }
                    }
                }

                offerDoc.DataList.BaggageAllowances = baggageList;
                offerDoc.DataList.ProductComponents = productComponents.DistinctBy(x => x.Description);
            }

            return offerDoc;
        }

        private static IEnumerable<string> GetBaggageAllowances(ref int checkedBagCounter, ref IEnumerable<BaggageAllowances> baggageList, Fare fare, Segment selectedSegment, bool point2pointVS)
        {
            IEnumerable<string> baggageRefIds = Enumerable.Empty<string>();

            foreach (BaggagePolicy bag in selectedSegment.CheckedBaggage)
            {
                string baggageRefId = GetBaggage(ref checkedBagCounter, bag, Constants.Application.CheckedBaggage, ref baggageList);
                if (baggageRefId != null && !baggageRefIds.Contains(baggageRefId))
                {
                    baggageRefIds = baggageRefIds.Append(baggageRefId);
                }
            }

            var carryOnBaggagesLists = fare.Segments.SelectMany(x => x.CarryOnBaggage);
            IEnumerable<BaggagePolicy> baggageAllowance = carryOnBaggagesLists;

            var carryOnBaggage = carryOnBaggagesLists.Where(x => x.FreeBags.Pieces > 0).OrderBy(x => x.FreeBags.Pieces);

            if (carryOnBaggage?.Any() == true)
            {
                if (carryOnBaggage.Any() && carryOnBaggage.Count() > 1 && carryOnBaggage.Select(x => x.FreeBags.Pieces).Distinct().Count() == 1)
                {
                    var carryOnBagDict = new Dictionary<BaggagePolicy, (int? Kg, int? Lb)>();

                    foreach (var bag in carryOnBaggage)
                    {
                        carryOnBagDict.Add(bag, (bag.FreeBags.Weight?.Weight?.Metric, bag.FreeBags.Weight?.Weight?.Imperial));
                    }
                    var filteredCarryOnBaggage = carryOnBagDict.OrderBy(x => x.Value.Kg ?? int.MinValue).First();
                    baggageAllowance = new[] { filteredCarryOnBaggage.Key };
                }
                else
                    baggageAllowance = new[] { carryOnBaggage.First() };

                foreach (BaggagePolicy bag in baggageAllowance)
                {
                    string baggageRefId = GetBaggage(ref checkedBagCounter, bag, Constants.Application.CarryOnBaggage, ref baggageList, point2pointVS);
                    if (baggageRefId != null && !baggageRefIds.Contains(baggageRefId))
                    {
                        baggageRefIds = baggageRefIds.Append(baggageRefId);
                    }
                }
            }
            return baggageRefIds.Any() ? baggageRefIds : null;
        }

        private static string GetPtc(string brandPtc)
        {
            return brandPtc switch
            {
                "ADT" => "ADT",
                "YTH" => "GBE",
                "CNN" => "CNN",
                "CHD" => "CHD",
                "INF" => "INF",
                _ => default
            };
        }

        private static string GetBaggage(ref int baggageCounter, BaggagePolicy bag, string baggageAllowanceTypeCode, ref IEnumerable<BaggageAllowances> baggageAllowances, [Optional] bool point2pointVS)
        {
            var bagWeightInKgm = (baggageAllowanceTypeCode == Constants.Application.CheckedBaggage) ? Constants.Application.CheckedBagWeightKgm : null;

            var maximumWeightMeasureMetric = (bag?.FreeBags?.Weight != null) ? Convert.ToString(bag?.FreeBags?.Weight?.Weight?.Metric) : bagWeightInKgm;

            var bagWeightLbr = (baggageAllowanceTypeCode == Constants.Application.CheckedBaggage) ? Constants.Application.CheckedBagWeightLbr : null;

            var maximumWeightMeasureImperial = (bag?.FreeBags?.Weight != null) ? Convert.ToString(bag?.FreeBags?.Weight?.Weight?.Imperial) : bagWeightLbr;

            BaggageAllowances existingBaggage = baggageAllowances?.FirstOrDefault(bagList => bagList.BaggageAllowanceTypeCode == baggageAllowanceTypeCode && bagList.FreeBaggageAllowance.AllowedBagCnt == bag.FreeBags?.Pieces.Value && bagList.FreeBaggageAllowance.WeightAllowance?.First().MaximumWeightMeasure == maximumWeightMeasureMetric && bagList.FreeBaggageAllowance?.WeightAllowance?.ElementAt(1)?.MaximumWeightMeasure == maximumWeightMeasureImperial);

            if (existingBaggage == null)
            {
                existingBaggage = GetBaggageAllowances(bag, baggageCounter, baggageAllowanceTypeCode, point2pointVS);
                baggageAllowances = (baggageAllowances ?? Enumerable.Empty<BaggageAllowances>()).Concat(new[] { existingBaggage });

                baggageCounter++;
            }

            return existingBaggage.BaggageAllowanceId;
        }

        private static BaggageAllowances GetBaggageAllowances(BaggagePolicy bag, int baggageCounter, string baggageAllowanceTypeCode, bool point2pointVS)
        {
            BaggageAllowances baggage = new()
            {
                BaggageAllowanceId = $"{Constants.Application.BAG}{baggageCounter}",
                BaggageAllowanceTypeCode = baggageAllowanceTypeCode,
                FreeBaggageAllowance = new FreeBaggageAllowance()
                {
                    AllowedBagCnt = bag.FreeBags?.Pieces.Value ?? 0,
                    BagDescriptors = (baggageAllowanceTypeCode == "CarryOn") && !point2pointVS ? new List<BagDescriptor>()
                    {
                        new()
                        {
                            BagDescriptorName = Constants.Application.CarryOnBaggageText
                        }
                    } : null,
                    DimensionAllowance = (baggageAllowanceTypeCode == Constants.Application.CheckedBaggage) ? GetCheckedDimensionAllowance(bag) : (baggageAllowanceTypeCode == Constants.Application.CarryOnBaggage) && point2pointVS ? GetCarryOnDimensionAllowance(bag) : null,
                    WeightAllowance = (baggageAllowanceTypeCode == "CarryOn") && bag.FreeBags?.Weight == null ? null : new List<WeightAllowance>()
                    {
                        new()
                        {
                            MaximumWeightMeasure =  (bag.FreeBags?.Weight != null) ? Convert.ToString(bag.FreeBags?.Weight?.Weight?.Metric) : (baggageAllowanceTypeCode == Constants.Application.CheckedBaggage) ? Constants.Application.CheckedBagWeightKgm : Constants.Application.CarryOnBagWeightKgm,
                            WeightUnitOfMeasurement = WeightUnitCodeType.KGM
                        },
                        new()
                        {
                            MaximumWeightMeasure = (bag.FreeBags?.Weight != null) ? Convert.ToString(bag.FreeBags?.Weight?.Weight?.Imperial) : (baggageAllowanceTypeCode == Constants.Application.CheckedBaggage) ? Constants.Application.CheckedBagWeightLbr : Constants.Application.CarryOnBagWeightLbr,
                            WeightUnitOfMeasurement = WeightUnitCodeType.LBR
                        }
                    }
                }
            };

            return baggage;
        }

        private static List<DimensionAllowance> GetCheckedDimensionAllowance(BaggagePolicy bag)
        {
            return new List<DimensionAllowance>()
            {
                new()
                {
                    HeightMeasure =Constants.Application.CheckedBagHeightCmt,
                    LengthMeasure = Constants.Application.CheckedBagLengthCmt,
                    LinearMeasure = (bag.FreeBags?.Size != null) ? Convert.ToString(bag.FreeBags?.Size?.Size?.Metric) : Constants.Application.CheckedBagLinearCmt,
                    WidthMeasure = Constants.Application.CheckedBagWidthCmt,
                    LengthUnitOfMeasurement = LengthUnitCodeType.CMT,
                },
                new()
                {
                    HeightMeasure = Constants.Application.CheckedBagHeightInh,
                    LengthMeasure = Constants.Application.CheckedBagLengthInh,
                    LinearMeasure = (bag.FreeBags?.Size != null) ? Convert.ToString(bag.FreeBags?.Size?.Size?.Imperial) : Constants.Application.CheckedBagLinearInh,
                    WidthMeasure = Constants.Application.CheckedBagWidthInh,
                    LengthUnitOfMeasurement = LengthUnitCodeType.INH,
                }
            };
        }

        private static List<DimensionAllowance> GetCarryOnDimensionAllowance(BaggagePolicy bag)
        {
            return new List<DimensionAllowance>()
            {
                new()
                {
                    HeightMeasure =Constants.Application.CarryOnBagHeightCmt,
                    LengthMeasure = Constants.Application.CarryOnBagLengthCmt,
                    LinearMeasure = (bag.FreeBags?.Size != null) ? Convert.ToString(bag.FreeBags?.Size?.Size?.Metric) : Constants.Application.CarryOnBagLinearCmt,
                    WidthMeasure = Constants.Application.CarryOnBagWidthCmt,
                    LengthUnitOfMeasurement = LengthUnitCodeType.CMT,
                },
                new()
                {
                    HeightMeasure = Constants.Application.CarryOnBagHeightInh,
                    LengthMeasure = Constants.Application.CarryOnBagLengthInh,
                    LinearMeasure = (bag.FreeBags?.Size != null) ? Convert.ToString(bag.FreeBags?.Size?.Size?.Imperial) : Constants.Application.CarryOnBagLinearInh,
                    WidthMeasure = Constants.Application.CarryOnBagWidthInh,
                    LengthUnitOfMeasurement = LengthUnitCodeType.INH,
                }
            };
        }

        public static OrderDocument MapFareComponents(OrderDocument orderDocument, IEnumerable<BrandRS> brandRs, OrderDocument existingOrderDoc)
        {
            if (!brandRs.Any(x => (bool)(x.Errors?.Any())))
            {
                InitializeDataLists(orderDocument);

                int decimalPrecisionCount = 0;

                if (orderDocument.OrderItems.First().PriceDetails != null)
                {
                    decimalPrecisionCount = orderDocument.OrderItems.First().PriceDetails.First().Amount.DecimalPrecisionCount;
                    if (decimalPrecisionCount == 0)
                    {
                        decimalPrecisionCount = orderDocument.OrderItems.First().PriceDetails.First().Amount.MonetaryAmount.Split(".").Last().Length;
                    }
                }

                int checkedBagCounter = 1;
                IEnumerable<BaggageAllowances> baggageList = null;
                var penaltyList = new List<Penalty>();
                int penaltyIndex = 1;
                var serviceDefinitionsList = new List<ServiceDefinition>();
                var fareSegments = brandRs.SelectMany(bs => bs.Itineraries).SelectMany(itinerary => itinerary.Legs.SelectMany(leg => leg.Fares.SelectMany(fare => fare.Segments
                               .Select(segment => new
                               {
                                   fare,
                                   segment,
                                   ptc = itinerary.PassengerTypeCodes?.FirstOrDefault()
                               })))).OrderBy(x => Array.IndexOf(["ADT", "YTH", "CNN", "INF"], x.ptc));

                foreach (var fareSegment in fareSegments)
                {
                    var fare = fareSegment.fare;

                    foreach (ChangePolicy change in fare.Fees?.Change!)
                    {
                        if (!penaltyList.Any(p => p.Type == Constants.Application.ChangePenaltyType && p.MaxPrice.Amount.MonetaryAmount == change.Fee?.Amount?.ToString() && p.MinPrice.Amount.MonetaryAmount == change.Fee?.Amount?.ToString() && p.Text == change.Description))
                        {
                            penaltyList.Add(new Penalty()
                            {
                                Id = $"PEN{penaltyIndex}",
                                Type = Constants.Application.ChangePenaltyType,
                                MaxPrice = new MaxPrice()
                                {
                                    Amount = new CurrencyAmount()
                                    {
                                        MonetaryAmount = change.Fee?.Amount?.ToString(),
                                        BaseCurrency = change.Fee?.Currency?.ToString(),
                                    }
                                },
                                MinPrice = new MinPrice()
                                {
                                    Amount = new CurrencyAmount()
                                    {
                                        MonetaryAmount = change.Fee?.Amount?.ToString(),
                                        BaseCurrency = change.Fee?.Currency?.ToString(),
                                    }
                                },
                                Text = change.Description
                            });

                            penaltyIndex++;
                        }
                    }

                    foreach (CancelPolicy cancel in fare.Fees?.Cancellation!)
                    {
                        if (!penaltyList.Any(p => p.Type == Constants.Application.CancellationPenaltyType && p.MaxPrice.Amount.MonetaryAmount == cancel.Fee?.Amount?.ToString() && p.MinPrice.Amount.MonetaryAmount == cancel.Fee?.Amount?.ToString() && p.Text == cancel.Description))
                        {
                            penaltyList.Add(new Penalty()
                            {
                                Id = $"PEN{penaltyIndex}",
                                Type = Constants.Application.CancellationPenaltyType,
                                MaxPrice = new MaxPrice()
                                {
                                    Amount = new CurrencyAmount()
                                    {
                                        MonetaryAmount = cancel.Fee?.Amount?.ToString(),
                                        BaseCurrency = cancel.Fee?.Currency?.ToString()
                                    }
                                },
                                MinPrice = new MinPrice()
                                {
                                    Amount = new CurrencyAmount()
                                    {
                                        MonetaryAmount = cancel.Fee?.Amount?.ToString(),
                                        BaseCurrency = cancel.Fee?.Currency?.ToString()
                                    }
                                },
                                Text = cancel.Description
                            });
                            penaltyIndex++;
                        }
                    }

                    foreach (Segment segment in fare.Segments)
                    {
                        var point2pointVS = !fare.Segments.Any(x => x.CarryOnBaggage.Any(cb => cb.FreeBags.Carrier != "VS"));

                        var services = GetRelevantServices(orderDocument, segment, fareSegment.ptc);
                        var currentServiceDefinitionsList = new List<ServiceDefinition>();
                        MapAmenityServiceDefinitions(segment, serviceDefinitionsList, currentServiceDefinitionsList);

                        foreach (var service in services)
                        {
                            service.BaggageRefIds = [];
                            service.PenaltyRef = new()
                            {
                                CancellationPenaltyRefIds = [],
                                ChangePenaltyRefIds = []
                            };
                            service.ServiceDefinitionRefIds = [];

                            MapFareBrand(orderDocument, fare, segment, service);

                            service.PenaltyRef = new()
                            {
                                CancellationPenaltyRefIds = new(),
                                ChangePenaltyRefIds = new()
                            };
                            service.PenaltyRef.ChangePenaltyRefIds.AddRange([.. penaltyList.Where(x => x.Type == Constants.Application.ChangePenaltyType).Select(index => index.Id)]);
                            service.PenaltyRef.CancellationPenaltyRefIds.AddRange([.. penaltyList.Where(x => x.Type == Constants.Application.CancellationPenaltyType).Select(index => index.Id)]);

                            var baggageRefIds = GetBaggageAllowances(ref checkedBagCounter, ref baggageList, fare, segment, point2pointVS);

                            service.ServiceDefinitionRefIds = currentServiceDefinitionsList.Select(s => s.ServiceDefinitionId);
                            service.BaggageRefIds = baggageRefIds;
                        }
                    }
                }

                orderDocument.DataList.BaggageAllowances = baggageList;
                orderDocument.DataList.Penalty = penaltyList;
                orderDocument.DataList.ServiceDefinitions = serviceDefinitionsList;
            }
            else
            {
                orderDocument.DataList.BaggageAllowances = existingOrderDoc.DataList.BaggageAllowances;
                orderDocument.DataList.Penalty = existingOrderDoc.DataList.Penalty;
                orderDocument.DataList.ServiceDefinitions = existingOrderDoc.DataList.ServiceDefinitions;

                if (existingOrderDoc.OrderItems.All(x => x.ExistingOrderItemId != null))
                {
                    orderDocument.OrderItems = orderDocument.OrderItems.Select(oi =>
                    {
                        var orderMgmtOrderItem = oi.Services.SelectMany(x => x.ServiceBeneficiaries).Distinct();
                        var existingOrderItem = existingOrderDoc.OrderItems.FirstOrDefault(x => x.Services?.SelectMany(service => service.ServiceBeneficiaries).Any(srv => orderMgmtOrderItem.Contains(srv)) == true);

                        if (existingOrderItem != null)
                        {
                            oi.OrderItemId = existingOrderItem.OrderItemId;
                        }

                        return oi;
                    });
                }

                foreach (var orderItem in orderDocument.OrderItems)
                {
                    var existingOrderItem = existingOrderDoc.OrderItems.First(oi => oi.OrderItemId == orderItem.OrderItemId);
                    foreach (var service in orderItem.Services)
                    {
                        var esitingService = existingOrderItem.Services.First(s => s.ServiceId == service.ServiceId);
                        service.BaggageRefIds = esitingService.BaggageRefIds;
                        service.PenaltyRef = esitingService.PenaltyRef;
                        service.ServiceDefinitionRefIds = esitingService.ServiceDefinitionRefIds;
                    }
                }
            }

            return orderDocument;
        }

        private static void InitializeDataLists(OrderDocument orderDocument)
        {
            orderDocument.DataList.BaggageAllowances = [];
            orderDocument.DataList.Penalty = [];
            orderDocument.DataList.ServiceDefinitions = [];
        }

        private static IEnumerable<OrderService> GetRelevantServices(OrderDocument orderDocument, Segment segment, string ptc)
        {
            IEnumerable<OrderService> services;
            var cust = orderDocument.DataList.CustomerDetails.Where(c => c.ItaPtc.Is(GetPtc(ptc))).Select(c => c.CustId);

            var matchingSegment = orderDocument.DataList.SegmentDetails.FirstOrDefault(seg => seg.OriginAirportCode == segment.Origin && seg.DestinationAirportCode == segment.Destination);
            if (matchingSegment == null)
            {
                return Enumerable.Empty<OrderService>();
            }

            if (orderDocument.DataList.SegmentDetails.Any(x => x.Legs is null))
            {
                services = orderDocument.OrderItems
                                    .SelectMany(oi => oi.Services)
                                    .Where(serv => serv.Product != null && serv.Product.ProductType == Product.ProductTypes.FlightEnum && serv.SegmentRefIds.Contains(matchingSegment.SegmentId) && serv.ServiceBeneficiaries != null && serv.ServiceBeneficiaries.Any(s => !cust.Any() || cust.Contains(s)));
            }
            else
            {
                services = orderDocument.OrderItems
                                   .SelectMany(oi => oi.Services)
                                   .Where(serv => serv.Product != null && serv.Product.ProductType == Product.ProductTypes.FlightEnum && serv.SegmentRefIds.Contains(matchingSegment.SegmentId) && serv.ServiceBeneficiaries != null && serv.ServiceBeneficiaries.Any(s => !cust.Any() || cust.Contains(s)));
            }

            return services;
        }

        private static void MapFareBrand(OrderDocument orderDocument, Fare fare, Segment segment, OrderService service)
        {
            var brandName = fare.Product is null ? null : Constants.Application.FareFamilyDictionary.FirstOrDefault(x => string.Equals(x.Key, fare.Product, StringComparison.OrdinalIgnoreCase)).Key;

            if (brandName is not null)
            {
                service.Product.ProductName = brandName;
                service.Product.ProductOwner = Constants.Application.OwnerAirline;
            }

            foreach (var leg in orderDocument.DataList.SegmentDetails.Where(seg => seg.Legs != null).SelectMany(seg => seg.Legs))
            {
                if (segment.Origin == leg.OriginAirportCode && segment.Destination == leg.DestinationAirportCode)
                {
                    leg.CarrierAircraftTypeCode = segment.Amenities?.Aircraft?.Model;
                    leg.CarrierAircraftTypeName = segment.Amenities?.Aircraft?.DisplayText;
                }
            }
        }

        private static void MapAmenityServiceDefinitions(Segment segment, List<ServiceDefinition> serviceDefinitionsList, List<ServiceDefinition> currentServiceDefinitionList)
        {
            List<string> desc = [
                                segment.Amenities?.Aircraft?.DisplayText,
                                segment.Amenities?.Beverage?.DisplayText,
                                segment.Amenities?.Entertainment?.DisplayText,
                                segment.Amenities?.FreshFood?.DisplayText,
                                segment.Amenities?.Layout?.DisplayText,
                                segment.Amenities?.Power?.DisplayText,
                                segment.Amenities?.Seats?.DisplayText,
                                segment.Amenities?.Wifi?.DisplayText
                              ];

            var serviceDef = serviceDefinitionsList.FirstOrDefault(x => x.Description.SequenceEqual(desc));

            if (serviceDef == null)
            {
                var service = new ServiceDefinition
                {
                    ServiceDefinitionId = $"{Constants.Application.SRV}{serviceDefinitionsList.Count + 1}",
                    Description = desc
                };
                serviceDefinitionsList.Add(service);
                currentServiceDefinitionList.Add(service);
            }
            else
                currentServiceDefinitionList.Add(serviceDef);
        }
    }
}