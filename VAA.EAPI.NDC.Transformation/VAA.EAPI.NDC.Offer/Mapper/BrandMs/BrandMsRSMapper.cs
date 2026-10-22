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
using VAA.EAPI.NDC.Offer.Infrastructure.Model.BrandMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;

namespace VAA.EAPI.NDC.Offer.Mapper.BrandMs
{
    public class BrandMsRSMapper
    {
        public static OfferResponse UpdateOfferPrice(BrandRS brandResponse, OfferDetails reshopOffer, OfferResponse reshopResponse, ref int penaltyIndex, ref int checkedBagCounter, ref int serviceCounter, ref IEnumerable<BaggageAllowances> baggageList)
        {
            List<Penalty> penaltyList = new();
            List<ProductComponent> productComponents = new();

            foreach (OfferService service in reshopOffer.OfferItems.SelectMany(offerItem => offerItem.Services))
            {
                service.BaggageRefIds = [];
                service.PenaltyRef = new PenaltyRef();
                service.PenaltyRef.ChangePenaltyRefIds = [];
                service.PenaltyRef.CancellationPenaltyRefIds = [];

                foreach (string segmentRefId in service.ProductSegmentAssociation.OfferSegmentRefIds)
                {
                    var offerSeg = reshopResponse.DataList.OfferSegments.First(oSeg => oSeg.OfferSegmentId == segmentRefId);
                    var segmentList = reshopResponse.DataList.FlightSegments.First(fSeg => fSeg.SegmentId == offerSeg.FlightSegmentRefId);
                    var fares = brandResponse.Itineraries.SelectMany(x => x.Legs).SelectMany(x => x.Fares);

                    foreach (var fare in fares)
                    {
                        var selectedSegment = fare.Segments?.FirstOrDefault(segment => segment.FlightNumber.ToString() == segmentList.MarketingFlightNumber && Convert.ToDateTime(segment.FlightDate).Date == Convert.ToDateTime(segmentList.ScheduledDepartureTime.LocalDateTime).Date);
                        var point2pointVS = !fare.Segments.Any(x => x.CarryOnBaggage.Any(cb => cb.FreeBags.Carrier != "VS"));

                        if (selectedSegment != null)
                        {
                            penaltyList = new();

                            foreach (ChangePolicy change in fare.Fees.Change)
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

                            service.PenaltyRef.ChangePenaltyRefIds.AddRange(penaltyList.Select(index => index.Id).ToList());
                            reshopResponse.DataList.Penalty = reshopResponse.DataList.Penalty.Concat(penaltyList);

                            penaltyList = new();

                            foreach (CancelPolicy cancel in fare.Fees?.Cancellation)
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

                            service.PenaltyRef.CancellationPenaltyRefIds.AddRange(penaltyList.Select(index => index.Id).ToList());
                            reshopResponse.DataList.Penalty = reshopResponse.DataList.Penalty.Concat(penaltyList);

                            if (service.PriceClassRefIds.Any())
                            {
                                foreach (string priceClassId in service.PriceClassRefIds)
                                {
                                    reshopResponse.DataList.PriceClass.First(p => p.Id == priceClassId).ProductName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase((fare?.Product ?? string.Empty).ToLower());

                                }
                            }

                            if (selectedSegment.Amenities != null)
                            {
                                var serviceList = new List<ProductComponent>();

                                if (selectedSegment.Amenities.Aircraft != null)
                                    serviceList.Add(new()
                                    {
                                        Id = $"{Constants.Application.SV}{serviceCounter++}",
                                        Description = selectedSegment.Amenities.Aircraft?.DisplayText
                                    });

                                if (selectedSegment.Amenities.Beverage != null)
                                    serviceList.Add(new()
                                    {
                                        Id = $"{Constants.Application.SV}{serviceCounter++}",
                                        Description = selectedSegment.Amenities.Beverage?.DisplayText
                                    });

                                if (selectedSegment.Amenities.Entertainment != null)
                                    serviceList.Add(new()
                                    {
                                        Id = $"{Constants.Application.SV}{serviceCounter++}",
                                        Description = selectedSegment.Amenities.Entertainment?.DisplayText
                                    });

                                if (selectedSegment.Amenities.FreshFood != null)
                                    serviceList.Add(new()
                                    {
                                        Id = $"{Constants.Application.SV}{serviceCounter++}",
                                        Description = selectedSegment.Amenities.FreshFood?.DisplayText
                                    });

                                if (selectedSegment.Amenities.Layout != null)
                                    serviceList.Add(new()
                                    {
                                        Id = $"{Constants.Application.SV}{serviceCounter++}",
                                        Description = selectedSegment.Amenities.Layout?.DisplayText
                                    });

                                if (selectedSegment.Amenities.Power != null)
                                    serviceList.Add(new()
                                    {
                                        Id = $"{Constants.Application.SV}{serviceCounter++}",
                                        Description = selectedSegment.Amenities.Power?.DisplayText
                                    });

                                if (selectedSegment.Amenities.Seats != null)
                                    serviceList.Add(new()
                                    {
                                        Id = $"{Constants.Application.SV}{serviceCounter++}",
                                        Description = selectedSegment.Amenities.Seats?.DisplayText
                                    });

                                if (selectedSegment.Amenities.Wifi != null)
                                    serviceList.Add(new()
                                    {
                                        Id = $"{Constants.Application.SV}{serviceCounter++}",
                                        Description = selectedSegment.Amenities.Wifi?.DisplayText
                                    });

                                productComponents.AddRange(serviceList);
                                offerSeg.ProductComponentRefIds = serviceList.Select(sL => sL.Id).ToList();
                            }

                            var baggageRefIds = GetBaggageAllowances(ref checkedBagCounter, ref baggageList, fare, selectedSegment, point2pointVS);

                            service.BaggageRefIds = baggageRefIds;
                            foreach (var refId in service.ProductSegmentAssociation.OfferSegmentRefIds)
                            {
                                var legData = reshopResponse.DataList.FlightSegments.First(fs => fs.SegmentId == reshopResponse.DataList.OfferSegments.First(os => os.OfferSegmentId == refId).FlightSegmentRefId).Legs.Where(l => l.OriginAirportCode == selectedSegment.Origin && l.DestinationAirportCode == selectedSegment.Destination).ToList();

                                foreach (var legInfo in legData)
                                {
                                    legInfo.CarrierAircraftTypeCode = selectedSegment?.Amenities?.Aircraft?.Model;
                                    legInfo.CarrierAircraftTypeName = selectedSegment?.Amenities?.Aircraft?.DisplayText;
                                }
                            }
                        }
                    }
                }
            }

            reshopResponse.DataList.BaggageAllowances = baggageList;
            reshopResponse.DataList.ProductComponents = reshopResponse.DataList.ProductComponents.Concat(productComponents);
            return reshopResponse;
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

        public static OfferResponse UpdateOfferPricePTC( IEnumerable<BrandRS> brandMSvalue,OfferDetails brandMSkey,OfferResponse reshopResponse,ref int penaltyIndex,ref int checkedBagCounter,ref int serviceCounter, ref IEnumerable<BaggageAllowances> baggageList)
        {
            var productComponents = new List<ProductComponent>();
            var penaltyList = new List<Penalty>();

            foreach (var offerItem in brandMSkey.OfferItems)
            {
                foreach (var service in offerItem.Services)
                {
                    string ptc = service.ServiceBeneficiaries.First();

                    var brandItineraries = brandMSvalue.SelectMany(b => b.Itineraries).Where(i => i.PassengerTypeCodes.Any(p => p.Substring(4) == ptc)).ToList();

                    service.BaggageRefIds = [];

                    service.PenaltyRef = new PenaltyRef
                    {
                        ChangePenaltyRefIds = [],
                        CancellationPenaltyRefIds = []
                    };

                    foreach (string segmentRefId in service.ProductSegmentAssociation.OfferSegmentRefIds)
                    {
                        var offerSegment = reshopResponse.DataList.OfferSegments.First(oSeg => oSeg.OfferSegmentId == segmentRefId);
                        var segmentList = reshopResponse.DataList.FlightSegments.First(fSeg => fSeg.SegmentId == offerSegment.FlightSegmentRefId);

                        foreach (var itinerary in brandItineraries)
                        {
                            var fares = itinerary.Legs.SelectMany(l => l.Fares);

                            foreach (var fare in fares)
                            {
                                var segment = fare.Segments?.FirstOrDefault(segment => segment.FlightNumber.ToString() == segmentList.MarketingFlightNumber && Convert.ToDateTime(segment.FlightDate).Date == Convert.ToDateTime(segmentList.ScheduledDepartureTime.LocalDateTime).Date);
                                var point2pointVS = !fare.Segments.Any(x => x.CarryOnBaggage.Any(cb => cb.FreeBags.Carrier != "VS"));

                                if (segment != null)
                                {
                                    penaltyList = new List<Penalty>();

                                    MapFeeChange(penaltyList, fare, ref penaltyIndex);
                                    service.PenaltyRef.ChangePenaltyRefIds.AddRange(penaltyList.Select(p => p.Id));

                                    reshopResponse.DataList.Penalty = reshopResponse.DataList.Penalty.Concat(penaltyList);

                                    penaltyList = new List<Penalty>();

                                    MapFeeCancellation(penaltyList, fare, ref penaltyIndex);

                                    service.PenaltyRef.CancellationPenaltyRefIds.AddRange(penaltyList.Select(p => p.Id));

                                    reshopResponse.DataList.Penalty = reshopResponse.DataList.Penalty.Concat(penaltyList);

                                    if (service.PriceClassRefIds.Any())
                                    {
                                        var priceClass = reshopResponse.DataList.PriceClass.First(p => p.Id == service.PriceClassRefIds.First());

                                        priceClass.ProductName = fare?.Product ?? string.Empty;
                                    }

                                    if (segment.Amenities != null)
                                    {
                                        var serviceList = MapServiceList(segment, serviceCounter);

                                        productComponents.AddRange(serviceList);

                                        var offerSegments = reshopResponse.DataList.OfferSegments.Where(o => service.ProductSegmentAssociation.OfferSegmentRefIds.Contains(o.OfferSegmentId));

                                        foreach (var offerSeg in offerSegments)
                                        {
                                            offerSeg.ProductComponentRefIds = serviceList.Select(s => s.Id);
                                        }
                                    }
                                    var baggageRefIds = GetBaggageAllowances(ref checkedBagCounter, ref baggageList, fare, segment, point2pointVS);

                                    service.BaggageRefIds = baggageRefIds;
                                    MapAircraft(segment, reshopResponse);
                                }
                            }
                        }
                    }
                    
                }
             }
            reshopResponse.DataList.BaggageAllowances = baggageList;
            reshopResponse.DataList.ProductComponents = reshopResponse.DataList.ProductComponents.Concat(productComponents);

            return reshopResponse;
        }

        private static void MapAircraft(Segment segment, OfferResponse reshopResponse)
        {
            var flightLegs = reshopResponse.DataList.FlightSegments.SelectMany(f => f.Legs ?? []);

            foreach (var leg in flightLegs)
            {
                if (leg.OriginAirportCode == segment.Origin &&
                    leg.DestinationAirportCode == segment.Destination)
                {
                    leg.CarrierAircraftTypeCode = segment?.Amenities?.Aircraft?.Model;
                    leg.CarrierAircraftTypeName = segment?.Amenities?.Aircraft?.DisplayText;
                }
            }
        }
        
        public static void MapFeeCancellation(List<Penalty> penaltyList, Fare fare, ref int penaltyIndex)
        {
            foreach (CancelPolicy cancel in fare.Fees?.Cancellation)
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
        }

        public static void MapFeeChange(List<Penalty> penaltyList, Fare fare, ref int penaltyIndex)
        {
            foreach (ChangePolicy change in fare.Fees?.Change)
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
        }

        public static List<ProductComponent> MapServiceList(Segment selectedSegment, int serviceCounter)
        {
            var serviceList = new List<ProductComponent>();

            if (selectedSegment.Amenities.Aircraft != null)
                serviceList.Add(new()
                {
                    Id = $"{Constants.Application.SV}{serviceCounter++}",
                    Description = selectedSegment.Amenities.Aircraft?.DisplayText
                });
            if (selectedSegment.Amenities.Beverage != null)
                serviceList.Add(new()
                {
                    Id = $"{Constants.Application.SV}{serviceCounter++}",
                    Description = selectedSegment.Amenities.Beverage?.DisplayText
                });

            if (selectedSegment.Amenities.Entertainment != null)
                serviceList.Add(new()
                {
                    Id = $"{Constants.Application.SV}{serviceCounter++}",
                    Description = selectedSegment.Amenities.Entertainment?.DisplayText
                });

            if (selectedSegment.Amenities.FreshFood != null)
                serviceList.Add(new()
                {
                    Id = $"{Constants.Application.SV}{serviceCounter++}",
                    Description = selectedSegment.Amenities.FreshFood?.DisplayText
                });

            if (selectedSegment.Amenities.Layout != null)
                serviceList.Add(new()
                {
                    Id = $"{Constants.Application.SV}{serviceCounter++}",
                    Description = selectedSegment.Amenities.Layout?.DisplayText
                });

            if (selectedSegment.Amenities.Power != null)
                serviceList.Add(new()
                {
                    Id = $"{Constants.Application.SV}{serviceCounter++}",
                    Description = selectedSegment.Amenities.Power?.DisplayText
                });

            if (selectedSegment.Amenities.Seats != null)
                serviceList.Add(new()
                {
                    Id = $"{Constants.Application.SV}{serviceCounter++}",
                    Description = selectedSegment.Amenities.Seats?.DisplayText
                });

            if (selectedSegment.Amenities.Wifi != null)
                serviceList.Add(new()
                {
                    Id = $"{Constants.Application.SV}{serviceCounter++}",
                    Description = selectedSegment.Amenities.Wifi?.DisplayText
                });

            return serviceList;
        }

        public static OfferResponse UpdateOrderQuote(BrandRS brandResponse, OfferDetails reshopOffer, OfferResponse reshopResponse, ref int penaltyIndex, ref int checkedBagCounter, ref int serviceCounter, ref IEnumerable<BaggageAllowances> baggageList)
        {
            IEnumerable<ProductComponent> productComponents = Enumerable.Empty<ProductComponent>();
            IEnumerable<Penalty> penaltyList = [];
            List<string> existingBaggageRefIds = new List<string>();

            var fares = brandResponse.Itineraries.SelectMany(x => x.Legs).SelectMany(x => x.Fares);

            foreach (OfferService service in reshopOffer.OfferItems.SelectMany(offerItem => offerItem.Services))
            {
                service.BaggageRefIds = [];
                service.PenaltyRef = new PenaltyRef
                {
                    ChangePenaltyRefIds = [],
                    CancellationPenaltyRefIds = []
                };

                foreach (string segmentRefId in service.ProductSegmentAssociation.OfferSegmentRefIds)
                {
                    var offerSeg = reshopResponse.DataList.OfferSegments.First(oSeg => oSeg.OfferSegmentId == segmentRefId);
                    var segmentList = reshopResponse.DataList.FlightSegments.First(fSeg => fSeg.SegmentId == offerSeg.FlightSegmentRefId);

                    foreach (var fare in fares)
                    {
                        var selectedSegment = fare.Segments?.FirstOrDefault(segment => segment.FlightNumber.ToString() == segmentList.MarketingFlightNumber && Convert.ToDateTime(segment.FlightDate).Date == Convert.ToDateTime(segmentList.ScheduledDepartureTime.LocalDateTime).Date);
                        var point2pointVS = !fare.Segments.Any(x => x.CarryOnBaggage.Any(cb => cb.FreeBags.Carrier != "VS"));

                        if (selectedSegment != null)
                        {
                            foreach (ChangePolicy change in fare.Fees.Change)
                            {
                                penaltyList = penaltyList.Append(new Penalty()
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

                            service.PenaltyRef.ChangePenaltyRefIds?.AddRange(penaltyList.Select(index => index.Id));
                            reshopResponse.DataList.Penalty = reshopResponse.DataList.Penalty == null ? penaltyList : reshopResponse.DataList.Penalty.Concat(penaltyList);
                            penaltyList = Enumerable.Empty<Penalty>();

                            foreach (CancelPolicy cancel in fare.Fees.Cancellation)
                            {
                                penaltyList = penaltyList.Append(new Penalty()
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
                                    Text = cancel.Headline.Split(":").First()
                                });
                                penaltyIndex++;


                            }
                            service.PenaltyRef.CancellationPenaltyRefIds?.AddRange(penaltyList.Select(index => index.Id));
                            reshopResponse.DataList.Penalty = reshopResponse.DataList.Penalty.Concat(penaltyList);

                            foreach (string priceClassRefId in service.PriceClassRefIds)
                            {
                                reshopResponse.DataList.PriceClass.First(p => p.Id == priceClassRefId).ProductName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase((fare?.Product ?? string.Empty).ToLower());
                            }

                            if (selectedSegment.Amenities != null)
                            {
                                IEnumerable<ProductComponent> serviceList = new List<ProductComponent>()
                            {
                                    (selectedSegment.Amenities.Aircraft == null) ? null : new()
                                    {
                                        Id = $"{Constants.Application.SV}{serviceCounter++}",
                                        Description = selectedSegment.Amenities.Aircraft?.DisplayText
                                    },
                                    (selectedSegment.Amenities.Beverage == null) ? null : new()
                                    {
                                        Id = $"{Constants.Application.SV}{serviceCounter++}",
                                        Description = selectedSegment.Amenities.Beverage?.DisplayText
                                    },
                                    (selectedSegment.Amenities.Entertainment == null) ? null : new()
                                    {
                                        Id = $"{Constants.Application.SV}{serviceCounter++}",
                                        Description = selectedSegment.Amenities.Entertainment?.DisplayText
                                    },
                                    (selectedSegment.Amenities.FreshFood == null) ? null : new()
                                    {
                                        Id = $"{Constants.Application.SV}{serviceCounter++}",
                                        Description = selectedSegment.Amenities.FreshFood?.DisplayText
                                    },
                                    (selectedSegment.Amenities.Layout == null) ? null : new()
                                    {
                                         Id = $"{Constants.Application.SV}{serviceCounter++}",
                                         Description = selectedSegment.Amenities.Layout?.DisplayText
                                    },
                                    (selectedSegment.Amenities.Power == null) ? null : new()
                                    {
                                         Id = $"{Constants.Application.SV}{serviceCounter++}",
                                         Description = selectedSegment.Amenities.Power?.DisplayText
                                    },
                                    (selectedSegment.Amenities.Seats == null) ? null : new()
                                    {
                                         Id = $"{Constants.Application.SV}{serviceCounter++}",
                                         Description = selectedSegment.Amenities.Seats?.DisplayText
                                    },
                                    (selectedSegment.Amenities.Wifi == null) ? null : new()
                                    {
                                         Id = $"{Constants.Application.SV}{serviceCounter++}",
                                         Description = selectedSegment.Amenities.Wifi?.DisplayText
                                    }
                                };

                                serviceList = serviceList.Where(x => x != null);
                                productComponents = productComponents.Concat(serviceList);
                                offerSeg.ProductComponentRefIds = serviceList?.Select(sL => sL.Id);
                            }

                            var baggageRefIds = GetBaggageAllowances(ref checkedBagCounter, ref baggageList, fare, selectedSegment, point2pointVS);

                            service.BaggageRefIds = baggageRefIds;
                            reshopResponse.DataList.FlightSegments = MapAirCraftDetails(reshopResponse.DataList.FlightSegments, selectedSegment);
                        }
                    }
                }
            }

            reshopResponse.DataList.BaggageAllowances = baggageList;
            reshopResponse.DataList.ProductComponents = reshopResponse.DataList.ProductComponents.Concat(productComponents);
            return reshopResponse;
        }

        public static OfferResponse UpdateOrderQuotePTC(IEnumerable<BrandRS> brandMSvalue, OfferDetails brandMSkey, OfferResponse reshopResponse, ref int penaltyIndex, ref int checkedBagCounter, ref int serviceCounter, ref IEnumerable<BaggageAllowances> baggageList)
        {
            var productComponents = new List<ProductComponent>();
            var penaltyList = new List<Penalty>();
            List<string> existingBaggageRefIds = new List<string>();

            var fareSegments = brandMSvalue.SelectMany(bs => bs.Itineraries).SelectMany(itinerary => itinerary.Legs.SelectMany(leg => leg.Fares.SelectMany(fare => fare.Segments
                                                   .Select(segment => new
                                                   {
                                                       fare,
                                                       segment,
                                                       ptc = itinerary.PassengerTypeCodes?.FirstOrDefault()
                                                   }))));

            foreach (var fareSegment in fareSegments)
            {
                var fare = fareSegment.fare;
                var ptc = fareSegment.ptc.Split(new[] { '_' }, 2)[0];
                var cust = reshopResponse.DataList.CustomerDetails.Where(c => c.ItaPtc.Is(GetPtc(ptc))).Select(c => c.CustId);

                foreach (OfferItem offerItem in brandMSkey.OfferItems)
                {
                    foreach(OfferService service in offerItem.Services)
                    {
                        bool relevantService = service.ServiceBeneficiaries.Any(serv => !cust.Any() || cust.Any(c => c.Contains(serv)));
                        if (relevantService) 
                        {
                            service.BaggageRefIds = [];
                            service.PenaltyRef = new PenaltyRef
                            {
                                ChangePenaltyRefIds = [],
                                CancellationPenaltyRefIds = []
                            };
                            foreach (string segmentRefId in service.ProductSegmentAssociation.OfferSegmentRefIds)
                            {
                                var offerSeg = reshopResponse.DataList.OfferSegments.First(oSeg => oSeg.OfferSegmentId == segmentRefId);
                                var segmentList = reshopResponse.DataList.FlightSegments.First(fSeg => fSeg.SegmentId == offerSeg.FlightSegmentRefId);

                                var selectedSegment = fare.Segments.FirstOrDefault(segment => segment.FlightNumber.ToString() == segmentList.MarketingFlightNumber && Convert.ToDateTime(segment.FlightDate).Date == Convert.ToDateTime(segmentList.ScheduledDepartureTime.LocalDateTime).Date);
                                var point2pointVS = !fare.Segments.Any(x => x.CarryOnBaggage.Any(cb => cb.FreeBags.Carrier != "VS"));

                                if (selectedSegment != null)
                                {
                                    penaltyList = new List<Penalty>();
                                    MapFeeChange(penaltyList, fare, ref penaltyIndex);
                                    service.PenaltyRef.ChangePenaltyRefIds?.AddRange(penaltyList.Select(index => index.Id));
                                    reshopResponse.DataList.Penalty = reshopResponse.DataList.Penalty == null ? penaltyList : reshopResponse.DataList.Penalty.Concat(penaltyList);
                                    penaltyList = new List<Penalty>();

                                    MapFeeCancellation(penaltyList, fare, ref penaltyIndex);
                                    service.PenaltyRef.CancellationPenaltyRefIds?.AddRange(penaltyList.Select(index => index.Id));
                                    reshopResponse.DataList.Penalty = reshopResponse.DataList.Penalty.Concat(penaltyList);

                                    foreach (string priceClassRefId in service.PriceClassRefIds)
                                    {
                                        reshopResponse.DataList.PriceClass.First(p => p.Id == priceClassRefId).ProductName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase((fare?.Product ?? string.Empty).ToLower());
                                    }

                                    if (selectedSegment.Amenities != null)
                                    {
                                        var serviceList = MapServiceList(selectedSegment, serviceCounter);
                                        productComponents.AddRange(serviceList);
                                        offerSeg.ProductComponentRefIds = serviceList.Select(s => s.Id);
                                    }
                                    var baggageRefIds = GetBaggageAllowances(ref checkedBagCounter, ref baggageList, fare, selectedSegment, point2pointVS);
                                    service.BaggageRefIds = baggageRefIds;
                                    reshopResponse.DataList.FlightSegments = MapAirCraftDetails(reshopResponse.DataList.FlightSegments, selectedSegment);
                                }
                            }
                        }

                    }
                }
            }
            
            reshopResponse.DataList.BaggageAllowances = baggageList;
            reshopResponse.DataList.ProductComponents = reshopResponse.DataList.ProductComponents.Concat(productComponents);

            return reshopResponse;
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
            var baggageAllowanceId = "";
            
            if (baggageAllowanceTypeCode == "CarryOn")
            {
                baggageAllowanceId = $"{Constants.Application.CAB}{baggageCounter}";
            }
            else
            {
                baggageAllowanceId = $"{Constants.Application.BAG}{baggageCounter}";
            }

            BaggageAllowances baggage = new BaggageAllowances()
            {
                BaggageAllowanceId = baggageAllowanceId,
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
                    DimensionAllowance = (baggageAllowanceTypeCode == Constants.Application.CheckedBaggage) ? GetCheckedDimensionAllowance(bag) : (baggageAllowanceTypeCode == Constants.Application.CarryOnBag) && point2pointVS ? GetCarryOnDimensionAllowance(bag) : null,
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
                    LinearMeasure = (bag.FreeBags?.Size != null) ? Convert.ToString(bag?.FreeBags?.Size?.Size?.Metric) : Constants.Application.CheckedBagLinearCmt,
                    WidthMeasure = Constants.Application.CheckedBagWidthCmt,
                    LengthUnitOfMeasurement = LengthUnitCodeType.CMT,
                },
                new()
                {
                    HeightMeasure = Constants.Application.CheckedBagHeightInh,
                    LengthMeasure = Constants.Application.CheckedBagLengthInh,
                    LinearMeasure = (bag.FreeBags?.Size != null) ? Convert.ToString(bag?.FreeBags?.Size?.Size?.Imperial) : Constants.Application.CheckedBagLinearInh,
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
                    LinearMeasure = (bag?.FreeBags?.Size != null) ? Convert.ToString(bag?.FreeBags?.Size?.Size?.Metric) : Constants.Application.CarryOnBagLinearCmt,
                    WidthMeasure = Constants.Application.CarryOnBagWidthCmt,
                    LengthUnitOfMeasurement = LengthUnitCodeType.CMT,
                },
                new()
                {
                    HeightMeasure = Constants.Application.CarryOnBagHeightInh,
                    LengthMeasure = Constants.Application.CarryOnBagLengthInh,
                    LinearMeasure = (bag?.FreeBags?.Size != null) ? Convert.ToString(bag?.FreeBags?.Size?.Size?.Imperial) : Constants.Application.CarryOnBagLinearInh,
                    WidthMeasure = Constants.Application.CarryOnBagWidthInh,
                    LengthUnitOfMeasurement = LengthUnitCodeType.INH,
                }
            };
        }

        private static IEnumerable<EAPI.Domain.Virgin.Models.Products.Flight.Segment> MapAirCraftDetails(IEnumerable<EAPI.Domain.Virgin.Models.Products.Flight.Segment> flightSegments, Segment brandMsLegSegment)
        {
            if (flightSegments.Select(f => f.Legs).Any(l => l != null))
            {
                foreach (var segment in flightSegments)
                {
                    if (segment.Legs.Count() > 0)
                    {
                        foreach (var leg in segment.Legs)
                        {
                            leg.CarrierAircraftTypeCode = brandMsLegSegment.Amenities.Aircraft?.Model;
                            leg.CarrierAircraftTypeName = brandMsLegSegment.Amenities.Aircraft?.DisplayText;
                        }
                    }
                }
            }
            return flightSegments;
        }

        public static OrderDocument MapBrandMSResponseUsingSabreOrderDoc(OrderDocument orderDocument, IEnumerable<BrandRS> brandRS, OrderDocument existingOrderDoc)
        {
            if (!brandRS.Any(x => (bool)(x.Errors?.Any())))
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
                var fareSegments = brandRS.SelectMany(bs => bs.Itineraries).SelectMany(itinerary => itinerary.Legs.SelectMany(leg => leg.Fares.SelectMany(fare => fare.Segments
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

        private static void MapFareBrand(OrderDocument orderDocument, Fare fare, Segment segment, OrderService service)
        {
            var brandName = fare.Product is null ? null : Constants.Application.FareFamilyDictionary.FirstOrDefault(x => string.Equals(x.Key, fare.Product, StringComparison.OrdinalIgnoreCase)).Key;

            // Safely retrieve PriceClassRefIds and map to a PriceClass
            var priceClassRefIds = service.PriceClassRefIds?.FirstOrDefault();

            if (!string.IsNullOrEmpty(priceClassRefIds) && orderDocument.DataList.PriceClass != null)
            {
                var priceClass = orderDocument.DataList.PriceClass.FirstOrDefault(pc => priceClassRefIds.Contains(pc?.Id));

                if (priceClass != null)
                {
                    priceClass.ProductName = brandName;
                }
            }

            if (brandName is not null && service?.Product != null)
            {
                service.Product.ProductName = brandName;
                service.Product.ProductOwner = Constants.Application.OwnerCode;
            }

            foreach (var leg in orderDocument?.DataList?.SegmentDetails?.Where(seg => seg.Legs != null).SelectMany(seg => seg.Legs))
            {
                if (segment?.Origin == leg.OriginAirportCode && segment.Destination == leg.DestinationAirportCode)
                {
                    leg.CarrierAircraftTypeCode = segment?.Amenities?.Aircraft?.Model;
                    leg.CarrierAircraftTypeName = segment?.Amenities?.Aircraft?.DisplayText;
                }
            }
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

    }
}