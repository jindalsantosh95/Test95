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
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Common;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Reprice;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita
{
    public class OfferRepriceITAResponseMapper
    {
        public static async Task<OfferResponse> MaptoRepriceResponse(result itaResponse, RepriceRequest repriceRequest, ICachedPriceClass _cachedPriceClass, ICachedCabin _cachedCabin, IMediator mediator, ICachedAmenities _cachedAmenities, OfferResponse selectedOfferResponse, bool priceChange, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage)
        {
            var offerPriceResponse = new OfferResponse
            {
                Pos = repriceRequest.Pos,
                Parties = repriceRequest.Parties,
                OrderId = selectedOfferResponse.OrderId,
                FareTypeRequest = new()
                {
                    FareTypeCode = selectedOfferResponse?.FareTypeRequest?.FareTypeCode,
                    FareSubTypeCode = selectedOfferResponse?.FareTypeRequest?.FareSubTypeCode
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
                offerPriceResponse.Offers = await GetReshopOffers(itaResponse, repriceRequest, dataList, _cachedPriceClass, _cachedCabin, mediator, _cachedAmenities, selectedOfferResponse, priceChange, pieceAllowance, cabinBaggage);
                dataList.OriginDestinationAssociations = dataList.OriginDestinationAssociations.Count > 0 ? dataList.OriginDestinationAssociations : null;
                dataList.FlightSegments = dataList.FlightSegments.Count > 0 ? dataList.FlightSegments : null;
                dataList.BaggageAllowances = dataList.BaggageAllowances.Count > 0 ? dataList.BaggageAllowances : null;
                dataList.PriceClass = dataList.PriceClass.Count > 0 ? dataList.PriceClass : null;
                dataList.Penalty = dataList.Penalty.Count > 0 ? dataList.Penalty : null;
                dataList.Journey = dataList.Journey.Count > 0 ? dataList.Journey : null;
                dataList.OfferSegments = dataList.OfferSegments.Count > 0 ? dataList.OfferSegments : null;
                dataList.ProductComponents = dataList.ProductComponents.Count > 0 ? dataList.ProductComponents : null;
                offerPriceResponse.DataList = dataList;
            }

            var bundledSolutionSummary = itaResponse.Items.FirstOrDefault(x => x.GetType() == typeof(resultTripDetails)) as resultTripDetails;

            if (bundledSolutionSummary != null)
            {
                foreach (var solution in bundledSolutionSummary.solution)
                {
                    foreach (var pricing in solution.pricing)
                    {
                        UpdateDataList(offerPriceResponse.DataList,pricing.bookingInfo);
                    }
                }
            }
            return offerPriceResponse;
        }

        private static void UpdateDataList(DataList dataList, resultTripDetailsSolutionPricingBookingInfo[] bookingInfos)
        {
            foreach (var offerSegment in dataList.OfferSegments)
            {
                var flightSegment = dataList.FlightSegments.FirstOrDefault(f => f.SegmentId == offerSegment.FlightSegmentRefId);

                if (flightSegment != null && offerSegment.PriceClassRefIds == null)
                {
                    var originAirportCode = flightSegment.OriginAirportCode;
                    var destinationAirportCode = flightSegment.DestinationAirportCode;

                    foreach (var bookingInfo in bookingInfos)
                    {
                        if (bookingInfo.segment.origin.Equals(originAirportCode, StringComparison.OrdinalIgnoreCase) &&
                                            bookingInfo.segment.destination.Equals(destinationAirportCode, StringComparison.OrdinalIgnoreCase) &&
                                            bookingInfo.bookingCode.Equals(offerSegment.ClassOfService, StringComparison.OrdinalIgnoreCase))
                        {
                            var fare = bookingInfo.fare.extendedFareCode;

                            var matchingPriceClass = dataList.PriceClass.FirstOrDefault(pc => pc.FareBasisCode == fare);

                            if (matchingPriceClass != null)
                            {
                                offerSegment.PriceClassRefIds = matchingPriceClass.Id;
                            }
                        }
                    }
                }
            }
        }

        private static async Task<List<OfferDetails>> GetReshopOffers(result result, RepriceRequest repriceRequest, DataList dataList, ICachedPriceClass _cachedPriceClass, ICachedCabin _cachedCabin, IMediator mediator, ICachedAmenities _cachedAmenities, OfferResponse selectedOfferResponse, bool priceChange, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage)
        {
            var requestAccountCode = repriceRequest.Parties.Participants?.FirstOrDefault()?.AccountCode;

            List<OfferDetails> reshopOffers = new();
            if (result.Items is { Length: > 0 })
            {
                var tripDetails = result.Items.FirstOrDefault(x => x.GetType() == typeof(resultTripDetails)) as resultTripDetails;
                if (tripDetails?.solution != null && tripDetails.solution.Any())
                {
                    var solutions = repriceRequest.RepriceQueries.SearchCriteria.FareFamilies?.Count == 0 ? tripDetails.solution.Where(x => x.ext.selected == true) : tripDetails.solution;
                    foreach (var soln in solutions)
                    {
                        string upsellOfferId = $"{Guid.NewGuid()}{Constants.Application.Pipeline}{soln.id}";
                        var ttl = soln?.itinerary?.slice.First().segment?.First().pricing?.First().latestTicketingWithImmediateReservationTime;

                        if (requestAccountCode == null || soln.ext.accountCode)
                        {
                            reshopOffers.Add(new OfferDetails
                            {
                                OfferId = GetOfferId(soln, repriceRequest, upsellOfferId, priceChange),
                                RecordLocatorId = selectedOfferResponse.Offers.First().RecordLocatorId,
                                AveragePassengerPrice = GetAveragePassengerPrice(soln.ext),
                                HighestPassengerPrice = GetHighestPassengerPrice(soln.ext),
                                SalePrice = GetSalePrices(soln),
                                SalePriceDifferences = GetSalePriceDifferences(soln),
                                Selected = soln.ext.selected,
                                PriceChanged = soln.ext.selected ? priceChange : false,
                                UpsellOffer = soln.ext.sellup,
                                OfferItems = await GetOfferItems(soln, repriceRequest, dataList, _cachedPriceClass, _cachedCabin, mediator, _cachedAmenities, upsellOfferId, selectedOfferResponse, priceChange, pieceAllowance, cabinBaggage),
                                OfferExpirationTimeLimit = DateTime.UtcNow.AddMinutes(ConfigReader.Get<double>(Constants.Environment.OfferExpiryLimitInMinutes)).ToString(Constants.Application.ZuluDateTimeFormat),
                                TicketingTimeLimit = ttl != null ? ttl : TimeLimitMapper.GetTTL(DateTimeOffset.Parse(soln?.itinerary?.slice.First().segment?.First().pricing?.First().latestTicketingWithImmediateReservationTime).Offset, Convert.ToDateTime(soln?.itinerary?.slice.First().segment?.First().departure.date).Add(TimeSpan.Parse(soln?.itinerary?.slice.First().segment?.First().departure.time))).ToString(Constants.Application.ZuluDateTimeFormat),
                            });
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

        private static async Task<List<OfferItem>> GetOfferItems(resultTripDetailsSolution solution, RepriceRequest repriceRequest, DataList dataList, ICachedPriceClass _cachedPriceClass, ICachedCabin _cachedCabin, IMediator mediator, ICachedAmenities _cachedAmenities, string newOfferId, OfferResponse selectedOfferResponse, bool priceChange, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage)
        {
            int offerItemId = 1;
            List<OfferItem> offerItems = new();
            foreach (var pricing in solution.pricing)
            {
                string ptc = pricing.passenger.FirstOrDefault().ptc.FirstOrDefault();

                offerItems.Add(new OfferItem
                {
                    OfferItemId = GetOfferItemId(solution, repriceRequest, newOfferId, priceChange, offerItemId++),
                    Services = await GetOfferServices(pricing, solution, repriceRequest.CustomerDetails, ptc, dataList, _cachedPriceClass, _cachedCabin, mediator, _cachedAmenities, pieceAllowance, cabinBaggage),
                    OfferitemPricing = RepriceOfferItemPricingMapper.GetOfferItemPricing(pricing, selectedOfferResponse, ptc),
                    TicketDesigCode = pricing.fare.FirstOrDefault().ticketDesignator,
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

        private static async Task<List<OfferService>> GetOfferServices(resultTripDetailsSolutionPricing tripPricing, resultTripDetailsSolution solution, List<Customer> customerDetails, string Ptc, DataList dataList, ICachedPriceClass _cachedPriceClass, ICachedCabin _cachedCabin, IMediator mediator, ICachedAmenities _cachedAmenities, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage)
        {
            int serviceId = 1;
            var services = new List<OfferService>();
            var segments = new List<resultTripDetailsSolutionItinerarySliceSegment>();
            foreach (var itn in solution.itinerary.slice)
            {
                segments.AddRange(itn.segment);
            }
            var point2pointVS = !segments.Any(seg => seg.operationalFlight?.carrier != null);
            var allCodeShare = !segments.Any(seg => seg.operationalFlight?.carrier == null);

            foreach (var itinerary in solution.itinerary.slice)
            {
                List<string> offerSegmentId = new();
                string journeyRefId = await RepriceResponseDataListMapper.AddOriginDestination(tripPricing, itinerary, dataList, offerSegmentId, _cachedCabin);

                foreach (var pricing in solution.pricing)
                {
                    if (pricing.passenger.FirstOrDefault().ptc.FirstOrDefault().Equals(Ptc))
                    {
                        var priceClassRefIds = await RepriceResponseDataListMapper.AddpriceClassDetails(pricing, itinerary, dataList.PriceClass, _cachedPriceClass, _cachedCabin);
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
                            ServiceBeneficiaries = GetServiceBeneficiaries(customerDetails, Ptc),
                            JourneyRefIds = new List<string> { journeyRefId },
                            PenaltyRef = RepriceResponseDataListMapper.AddPenaltyDetails(pricing.fare, dataList.Penalty, itinerary),
                            PriceClassRefIds = priceClassRefIds,
                            BaggageRefIds = await GetBaggageAllowance(pricing, itinerary, dataList, pieceAllowance, cabinBaggage, point2pointVS, allCodeShare)
                        });
                    }
                }
            }
            return services;
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

        private static List<string> GetServiceBeneficiaries(List<Customer> customerDetail, string Ptc)
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
                            if (!string.IsNullOrEmpty(baggageRefId) && !baggageRefIds.Contains(baggageRefId))
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


    }
}
