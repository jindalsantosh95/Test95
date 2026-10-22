using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Common;
using Environment = VAA.EAPI.Microservices.Retail.Offer.Constants.Environment;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Reprice
{
    public class RepriceResponseMapper
    {
        public static async Task<OfferResponse> MaptoRepriceResponse(result itaResponse, RepriceRequest repriceRequest, ICachedPriceClass _cachedPriceClass, ICachedCabin _cachedCabin, IMediator mediator, ICachedAmenities _cachedAmenities, OfferResponse reshopSelectedOfferResponse, bool priceChange, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage)
        {
            var reshopResponse = new OfferResponse
            {
                Pos = repriceRequest.Pos,
                Parties = repriceRequest.Parties,
                OrderId = reshopSelectedOfferResponse.OrderId,
                FareTypeRequest = new()
                {
                    FareTypeCode = reshopSelectedOfferResponse?.FareTypeRequest?.FareTypeCode,
                    FareSubTypeCode = reshopSelectedOfferResponse?.FareTypeRequest?.FareSubTypeCode
                },
                ShoppingResponseId = $"{itaResponse.session}|{itaResponse.solutionSet}",
            };
            if (itaResponse.Items is { Length: > 0 })
            {
                DataList dataList = new()
                {
                    OriginDestinationAssociations = new(),
                    FlightSegments = new(),
                    BaggageAllowances = new(),
                    CustomerDetails = repriceRequest.CustomerDetails,
                    PriceClass = new(),
                    Penalty = new(),
                    Journey = new(),
                    OfferSegments = new(),
                    ProductComponents = new()
                };
                reshopResponse.Offers = await GetReshopOffers(itaResponse, repriceRequest, dataList, _cachedPriceClass, _cachedCabin, mediator, _cachedAmenities, reshopSelectedOfferResponse, priceChange, pieceAllowance, cabinBaggage);
                dataList.OriginDestinationAssociations = dataList.OriginDestinationAssociations.Count > 0 ? dataList.OriginDestinationAssociations : null;
                dataList.FlightSegments = dataList.FlightSegments.Count > 0 ? dataList.FlightSegments : null;
                dataList.BaggageAllowances = dataList.BaggageAllowances.Count > 0 ? dataList.BaggageAllowances : null;
                dataList.PriceClass = dataList.PriceClass.Count > 0 ? dataList.PriceClass : null;
                dataList.Penalty = dataList.Penalty.Count > 0 ? dataList.Penalty : null;
                dataList.Journey = dataList.Journey.Count > 0 ? dataList.Journey : null;
                dataList.OfferSegments = dataList.OfferSegments.Count > 0 ? dataList.OfferSegments : null;
                dataList.ProductComponents = dataList.ProductComponents.Count > 0 ? dataList.ProductComponents : null;
                reshopResponse.DataList = dataList;
            }
            return reshopResponse;
        }

        private static async Task<List<OfferDetails>> GetReshopOffers(result result, RepriceRequest repriceRequest, DataList dataList, ICachedPriceClass _cachedPriceClass, ICachedCabin _cachedCabin, IMediator mediator, ICachedAmenities _cachedAmenities, OfferResponse reshopSelectedOfferResponse, bool priceChange, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage)
        {
            List<OfferDetails> reshopOffers = new();

            var requestAccountCode = repriceRequest.Parties.Participants?.FirstOrDefault()?.AccountCode;

            if (result.Items is { Length: > 0 })
            {
                var tripDetails = result.Items.FirstOrDefault(x => x.GetType() == typeof(resultTripDetails)) as resultTripDetails;
                if (tripDetails?.solution != null && tripDetails.solution.Any())
                {
                    foreach (var soln in tripDetails.solution.OrderByDescending(x => x.ext.selected))
                    {
                        if (!soln.pricing.Any(x => x.reissueInfo.First().reissueAction.Equals(Constants.Application.NewTicket)))
                        {
                            string upsellOfferId = $"{Guid.NewGuid()}{Constants.Application.Pipeline}{soln.id}";
                            var ttl = soln?.itinerary?.slice.First().segment?.First().pricing?.First().latestTicketingWithImmediateReservationTime;

                            if (requestAccountCode == null || soln.ext.accountCode)
                            {
                                reshopOffers.Add(new OfferDetails
                                {
                                    OrderItemServices = reshopSelectedOfferResponse?.Offers?.FirstOrDefault().OrderItemServices,
                                    OfferId = GetOfferId(soln, repriceRequest, upsellOfferId, priceChange),
                                    RecordLocatorId = reshopSelectedOfferResponse.Offers.First().RecordLocatorId,
                                    AveragePassengerPrice = GetAveragePassengerPrice(soln.ext),
                                    HighestPassengerPrice = GetHighestPassengerPrice(soln.ext),
                                    SalePrice = GetSalePrices(soln),
                                    SalePriceDifferences = GetSalePriceDifferences(soln),
                                    Selected = soln.ext.selected,
                                    UpsellOffer = soln.ext.sellup,
                                    OfferItems = await GetOfferItems(soln, repriceRequest, dataList, _cachedPriceClass, _cachedCabin, mediator, _cachedAmenities, upsellOfferId, reshopSelectedOfferResponse, priceChange, pieceAllowance, cabinBaggage),
                                    OfferExpirationTimeLimit = DateTime.UtcNow.AddMinutes(ConfigReader.Get<double>(Environment.OfferExpiryLimitInMinutes)).ToString(Constants.Application.ZuluDateTimeFormat),
                                    TicketingTimeLimit = ttl != null ? ttl : TimeLimitMapper.GetTTL(DateTimeOffset.Parse(soln?.itinerary?.slice.First().segment?.First().pricing?.First().latestTicketingWithImmediateReservationTime).Offset, Convert.ToDateTime(soln?.itinerary?.slice.First().segment?.First().departure.date).Add(TimeSpan.Parse(soln?.itinerary?.slice.First().segment?.First().departure.time))).ToString(Constants.Application.ZuluDateTimeFormat),
                                });
                            }                              
                        }
                    }
                }
            }
            return reshopOffers.Count > 0 ? reshopOffers : null;
        }

        private static string GetOfferId(resultTripDetailsSolution solution, RepriceRequest repriceRequest, string upsellOfferId, bool priceChange)
        {
            if (solution.ext.selected)
            {
                if (priceChange)
                {
                    return $"{repriceRequest.PricedOffer.SelectedOffer.OfferRefID.Split("|").FirstOrDefault()}{Constants.Application.Pipeline}{solution.id}";
                }
                return repriceRequest.PricedOffer.SelectedOffer.OfferRefID;
            }

            return upsellOfferId;
        }

        private static async Task<List<OfferItem>> GetOfferItems(resultTripDetailsSolution solution, RepriceRequest repriceRequest, DataList dataList, ICachedPriceClass _cachedPriceClass, ICachedCabin _cachedCabin, IMediator mediator, ICachedAmenities _cachedAmenities, string newOfferId, OfferResponse reshopSelectedOfferResponse, bool priceChange, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage)
        {
            int offerItemId = 1;
            List<OfferItem> offerItems = new();
            foreach (var pricing in solution.pricing)
            {
                string ptc = pricing.passenger.FirstOrDefault().ptc.FirstOrDefault();
                offerItems.Add(new OfferItem
                {
                    OfferItemId = GetOfferItemId(solution, repriceRequest, newOfferId, priceChange, offerItemId++),
                    Services = await GetOfferServices(pricing, solution, repriceRequest, ptc, dataList, _cachedPriceClass, _cachedCabin, mediator, _cachedAmenities, pieceAllowance, cabinBaggage),
                    OfferitemPricing = RepriceOfferItemPricingMapper.GetOfferItemPricing(pricing, reshopSelectedOfferResponse, ptc),
                    TicketDesigCode = pricing.fare?.FirstOrDefault()?.ticketDesignator,
                });
            }
            return offerItems;
        }

        private static string GetOfferItemId(resultTripDetailsSolution solution, RepriceRequest repriceRequest, string upsellOfferId, bool priceChange, int offerItemId)
        {
            if (solution.ext.selected)
            {
                if (priceChange)
                {
                    return $"{repriceRequest.PricedOffer.SelectedOffer.OfferRefID.Split("|").FirstOrDefault()}{Constants.Application.Pipeline}{solution.id}-{offerItemId}";
                }
                return $"{repriceRequest.PricedOffer.SelectedOffer.OfferRefID}-{offerItemId}";
            }

            return $"{upsellOfferId}-{offerItemId}";
        }

        private static async Task<List<OfferService>> GetOfferServices(resultTripDetailsSolutionPricing tripPricing, resultTripDetailsSolution solution, RepriceRequest repriceRequest, string Ptc, DataList dataList, ICachedPriceClass _cachedPriceClass, ICachedCabin _cachedCabin, IMediator mediator, ICachedAmenities _cachedAmenities, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage)
        {
            int serviceId = 1;
            var services = new List<OfferService>();
            var segments = new List<resultTripDetailsSolutionItinerarySliceSegment>();

            IEnumerable<string> flownFlights = !repriceRequest.IsReshopHoldBooking ? GetFlownFlightsFromCoupon(repriceRequest) : null;
            IEnumerable<OriginDestination> flownoriginDestinations = !repriceRequest.IsReshopHoldBooking ? GetflownoriginDestinations(repriceRequest) : null;

            foreach (var itn in solution.itinerary.slice)
            {
                segments.AddRange(itn.segment);
            }
            var allCodeShare = !segments.Any(seg => seg.operationalFlight?.carrier == null);

            foreach (var itinerary in solution.itinerary.slice)
            {
                var point2pointVS = !itinerary.segment.Any(seg => seg.operationalFlight?.carrier != null);

                List<string> offerSegmentId = new();
                string journeyRefId = await RepriceResponseDataListMapper.AddOriginDestination(tripPricing, itinerary, dataList, offerSegmentId, _cachedCabin);

                foreach (var pricing in solution.pricing)
                {
                    if (pricing.passenger.FirstOrDefault().ptc.FirstOrDefault().Equals(Ptc))
                    {
                        var priceClassRefIds = await RepriceResponseDataListMapper.AddpriceClassDetails(pricing, itinerary, dataList.PriceClass, _cachedPriceClass, _cachedCabin);
                        var baggageAllowances = await GetBaggageAllowance(pricing, itinerary, dataList, pieceAllowance, cabinBaggage, point2pointVS, allCodeShare);
                        services.Add(new OfferService
                        {
                            ServiceId = $"{Constants.Application.Service}{serviceId++}",
                            Product = new()
                            {
                                ProductOwner = Constants.Application.VsFlightCode,
                                ProductCode = string.Empty,
                                ProductName = string.Empty,
                                ProductType = Product.ProductTypes.FlightEnum
                            },
                            ProductSegmentAssociation = new()
                            {
                                OfferSegmentRefIds = offerSegmentId
                            },
                            ServiceBeneficiaries = GetServiceBeneficiaries(repriceRequest.CustomerDetails, Ptc),
                            JourneyRefIds = new List<string> { journeyRefId },
                            PenaltyRef = (pricing.penaltyPrice != null && pricing.penaltyPrice.Any(x => x.amount != 0)) ? RepriceResponseDataListMapper.AddQuotePenaltyDetails(pricing, dataList.Penalty) : null,
                            PriceClassRefIds = priceClassRefIds,
                            BaggageRefIds = flownoriginDestinations is not null ? flownoriginDestinations.Any(item => item.OriginAirport == itinerary.origin && item.DestinationAirport == itinerary.destination) ? null : baggageAllowances : baggageAllowances// Hiding Baggage in case of Flown scenario
                        });
                    }
                }
            }
            return services;
        }

        private static IEnumerable<string> GetFlownFlightsFromCoupon(RepriceRequest repriceRequest)
        {
            List<string> flownFlights = new();
            var coupons = repriceRequest.AccountableDocumentRefs?.FirstOrDefault()?.Coupons;
            if (coupons == null) return Enumerable.Empty<string>();

            foreach (var coupon in coupons)
            {
                if (Constants.Application.Flownstatus.Contains(coupon.CouponStatus.ToUpper()))
                {
                    string flownFlight = $"{Constants.Application.VsFlightCode}{repriceRequest.SegmentDetails.FirstOrDefault(t => t.SegmentId == coupon.SegmentRefId)?.MarketingFlightNumber.TrimStart('0')}";
                    if (!flownFlights.Contains(flownFlight))
                        flownFlights.Add(flownFlight);
                }
            }
            return flownFlights;
        }

        private static IEnumerable<OriginDestination> GetflownoriginDestinations(RepriceRequest repriceRequest)
        {
            List<OriginDestination> flownOriginDestinations = new();
            var coupons = repriceRequest.AccountableDocumentRefs?.FirstOrDefault()?.Coupons;
            if (coupons == null) return Enumerable.Empty<OriginDestination>();

            foreach (var coupon in coupons)
            {
                if (Constants.Application.Flownstatus.Contains(coupon.CouponStatus.ToUpper()))
                {
                    OriginDestination originDestination = new();
                    originDestination.DestinationAirport = repriceRequest.SegmentDetails.FirstOrDefault(t => t.SegmentId == coupon.SegmentRefId)?.DestinationAirportCode;
                    originDestination.OriginAirport = repriceRequest.SegmentDetails.FirstOrDefault(t => t.SegmentId == coupon.SegmentRefId)?.OriginAirportCode;

                    if (!flownOriginDestinations.Contains(originDestination))
                        flownOriginDestinations.Add(originDestination);
                }
            }
            return flownOriginDestinations;
        }

        private static List<string> GetServiceBeneficiaries(IEnumerable<Customer> customerDetail, string Ptc)
        {
            List<string> serviceBeneficiaries = new();
            var customerDetails = customerDetail.Where(c => (c.ItaPtc ?? c.Ptc).Equals(Ptc));
            foreach (var c in customerDetails)
            {
                serviceBeneficiaries.Add(c.CustId);
            }

            return serviceBeneficiaries;
        }

        private static async Task<List<string>> GetBaggageAllowance(resultTripDetailsSolutionPricing solutionPricing, resultTripDetailsSolutionItinerarySlice itinerary, DataList dataList, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage, bool point2pointVS, bool allCodeShare)
        {
            List<string> baggageRefIds = new();
            var itinerarySeg = itinerary.segment.Select(x => x.flight.carrier + x.flight.number);
            if (solutionPricing.checkedBaggageAllowance is { Length: > 0 })
            {
                foreach (var checkedBaggageAllowance in solutionPricing.checkedBaggageAllowance)
                {
                    var checkedSegments = checkedBaggageAllowance.segment.Select(x => x.flight);
                    foreach (var segment in checkedSegments)
                    {
                        if (itinerarySeg.Contains(segment))
                        {
                            string baggageRefId = await RepriceResponseDataListMapper.AddBaggageAllowance(checkedBaggageAllowance, null, dataList.BaggageAllowances, point2pointVS, allCodeShare, pieceAllowance, cabinBaggage);
                            if (!baggageRefIds.Contains(baggageRefId))
                            {
                                baggageRefIds.Add(baggageRefId);
                            }
                        }
                    }
                }
            }
            if (solutionPricing.carryOnBaggageAllowance is { Length: > 0 })
            {
                var carryOnBaggageLists = solutionPricing.carryOnBaggageAllowance.Where(x => x.freeBaggageAllowance.pieces > 0 && x.segment != null && x.segment.Any(s => itinerarySeg.Contains(s.flight))).OrderBy(x => x.freeBaggageAllowance.pieces);

                if (carryOnBaggageLists?.Any() == true)
                {
                    if (carryOnBaggageLists.Count() > 1 && carryOnBaggageLists.Select(x => x.freeBaggageAllowance.pieces).Distinct().Count() == 1)
                    {
                        var carryOnBagDict = new Dictionary<resultTripDetailsSolutionPricingCarryOnBaggageAllowance, (int? Kg, int? Lb)>();

                        foreach (var carryOnAllowance in carryOnBaggageLists)
                        {
                            int? kg = null;
                            int? lb = null;

                            if (carryOnAllowance.freeBaggageAllowance.bagDescriptor != null && carryOnAllowance.freeBaggageAllowance.bagDescriptor.Any(x => x.description != null))
                            {
                                foreach (var bagDesc in carryOnAllowance.freeBaggageAllowance.bagDescriptor)
                                {
                                    foreach (var desc in bagDesc.description ?? Array.Empty<string>())
                                    {
                                        var parts = desc.Replace("Up to", "").Split('/');

                                        foreach (var part in parts)
                                        {
                                            var tokens = part.Trim().Split(' ');

                                            if (tokens.Length == 2)
                                            {
                                                if (tokens[1].Is("KG"))
                                                    kg = tokens[0].Parse<int>();

                                                if (tokens[1].Is("LB"))
                                                    lb = tokens[0].Parse<int>();
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if ((carryOnAllowance.freeBaggageAllowance.bagDescriptor == null || carryOnAllowance.freeBaggageAllowance.bagDescriptor.Any(x => x.description == null)) && carryOnAllowance.freeBaggageAllowance.kilos > 0)
                                {
                                    kg = carryOnAllowance.freeBaggageAllowance.kilos;
                                }
                            }

                            carryOnBagDict[carryOnAllowance] = (kg, lb);
                        }
                        var filteredCarryOnBaggage = carryOnBagDict.OrderBy(x => x.Value.Kg ?? int.MinValue).First();

                        string baggageRefId = await RepriceResponseDataListMapper.AddBaggageAllowance(null, filteredCarryOnBaggage.Key, dataList.BaggageAllowances, point2pointVS, allCodeShare, pieceAllowance, cabinBaggage);
                        if (baggageRefId != null && !baggageRefIds.Contains(baggageRefId))
                        {
                            baggageRefIds.Add(baggageRefId);
                        }
                    }
                    else
                    {
                        string baggageRefId = await RepriceResponseDataListMapper.AddBaggageAllowance(null, carryOnBaggageLists.First(), dataList.BaggageAllowances, point2pointVS, allCodeShare, pieceAllowance, cabinBaggage);
                        if (baggageRefId != null && !baggageRefIds.Contains(baggageRefId))
                        {
                            baggageRefIds.Add(baggageRefId);
                        }
                    }
                }
            }
            return baggageRefIds.Count() == 0 ? null : baggageRefIds;
        }

        private static AveragePassengerPrice GetAveragePassengerPrice(resultTripDetailsSolutionExt ext)
        {
            return new AveragePassengerPrice
            {
                Amount = new()
                {
                    BaseCurrency = ext?.averagePassengerPrice?.currency,
                    MonetaryAmount = ext?.averagePassengerPrice?.amount.ToString()
                }
            };
        }

        private static HighestPassengerPrice GetHighestPassengerPrice(resultTripDetailsSolutionExt ext)
        {
            return new HighestPassengerPrice
            {
                Amount = new()
                {
                    BaseCurrency = ext?.highestPassengerPrice?.currency,
                    MonetaryAmount = ext?.highestPassengerPrice?.amount.ToString()
                }
            };
        }

        private static List<Price> GetSalePrices(resultTripDetailsSolution solution)
        {
            List<Price> salePrices = new()
            {
                new Price
                {
                    Amount = new()
                    {
                        BaseCurrency = solution.saleTotal.currency,
                        MonetaryAmount = solution.saleTotal.amount.ToString()
                    },
                    PriceComponentName = string.Empty,
                    PriceComponentType = Price.PriceComponentTypes.BasePriceEnum
                },
                new Price
                {
                    Amount = new()
                    {
                        BaseCurrency = solution.displayFareTotal.currency,
                        MonetaryAmount = solution.displayFareTotal.amount.ToString()
                    },
                    PriceComponentName = string.Empty,
                    PriceComponentType = Price.PriceComponentTypes.BaseFareEnum
                },
                new Price
                {
                    Amount = new()
                    {
                        BaseCurrency = solution.saleTaxTotal.currency,
                        MonetaryAmount = solution.saleTaxTotal.amount.ToString()
                    },
                    PriceComponentName = string.Empty,
                    PriceComponentType = Price.PriceComponentTypes.TotalTaxEnum
                }
            };

            return salePrices;
        }

        private static List<Price> GetSalePriceDifferences(resultTripDetailsSolution solution)
        {
            List<Price> saleDifferences = null;
            decimal saleFareDifference = 0;
            string baseCurrency = null;
            decimal saleFareDifferencetotaltax = 0;
            foreach (var pricing in solution.pricing)
            {
                baseCurrency = pricing?.saleFareDifference?.currency;
                if (pricing?.saleFareDifference?.amount != null)
                {
                    saleFareDifference = (decimal)(saleFareDifference + pricing?.saleFareDifference?.amount * pricing.paxCount);
                }
                var totaltaxdifference = pricing?.ext?.reshopTaxTotal?.Sum(i => i.totalSalePriceDifference?.amount);
                saleFareDifferencetotaltax = (decimal)(saleFareDifferencetotaltax + totaltaxdifference.Value * pricing?.paxCount);
            }
            if (saleFareDifference != 0)
            {
                saleDifferences ??= new();
                saleDifferences.Add(new Price
                {
                    Amount = new()
                    {
                        BaseCurrency = baseCurrency,
                        MonetaryAmount = Convert.ToString(saleFareDifference)
                    },
                    PriceComponentName = string.Empty,
                    PriceComponentType = Price.PriceComponentTypes.BaseFareEnum
                });
                saleDifferences.Add(new Price
                {
                    Amount = new()
                    {
                        BaseCurrency = baseCurrency,
                        MonetaryAmount = Convert.ToString(saleFareDifferencetotaltax)
                    },
                    PriceComponentName = string.Empty,
                    PriceComponentType = Price.PriceComponentTypes.TotalTaxEnum
                });
            }
            return saleDifferences;
        }
    }
}
