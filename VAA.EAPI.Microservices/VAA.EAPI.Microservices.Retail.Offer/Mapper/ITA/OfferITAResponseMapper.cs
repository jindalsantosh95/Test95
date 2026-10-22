using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Products.Baggage;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Common;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Reshop;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita
{
    public class OfferITAResponseMapper
    {
        public static async Task<OfferResponse> OfferResponseMapper(result result, OfferRequest offerRequest, ICachedPriceClass cachedPriceClass, ICachedCabin cachedCabin, string docId, string offerExpiryLimit, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage, ICachedAirportDetails cachedAirportDetails)
        {
            var reshopResponse = new OfferResponse
            {
                Pos = offerRequest.Pos,
                Parties = offerRequest.Parties,
                OrderId = offerRequest.OrderId,
                FareTypeRequest = new()
                {
                    FareTypeCode = offerRequest?.FareTypeRequest?.FareTypeCode,
                    FareSubTypeCode = offerRequest?.FareTypeRequest?.FareSubTypeCode
                },
                ShoppingResponseId = $"{result.session}{Constants.Application.Pipeline}{result.solutionSet}",
            };

            if (result.Items is { Length: > 0 })
            {
                DataList dataList = new()
                {
                    OriginDestinationAssociations = new(),
                    FlightSegments = new(),
                    OfferSegments = new(),
                    BaggageAllowances = new(),
                    CustomerDetails = offerRequest.CustomerDetails,
                    PriceClass = new(),
                    Penalty = new(),
                    Journey = new()
                };

                reshopResponse.Offers = await GetOffers(result, offerRequest, dataList, docId, offerExpiryLimit, cachedCabin, cachedPriceClass, pieceAllowance, cabinBaggage, cachedAirportDetails);
                dataList.OriginDestinationAssociations = dataList.OriginDestinationAssociations.Any() ? dataList.OriginDestinationAssociations : null;
                dataList.FlightSegments = dataList.FlightSegments.Any() ? dataList.FlightSegments : null;
                dataList.OfferSegments = dataList.OfferSegments.Any() ? dataList.OfferSegments : null;
                dataList.BaggageAllowances = dataList.BaggageAllowances.Any() ? dataList.BaggageAllowances : null;
                dataList.PriceClass = dataList.PriceClass.Any() ? dataList.PriceClass : null;
                dataList.Journey = dataList.Journey.Any() ? dataList.Journey : null;
                dataList.Penalty = dataList.Penalty.Any() ? dataList.Penalty : null;
                reshopResponse.DataList = dataList;
            }
            return reshopResponse;
        }

        public static OfferResponse CreateStreamingResponse(OfferRequest offerRequest)
        {
            return new OfferResponse
            {
                Pos = offerRequest.Pos,
                Parties = offerRequest.Parties,
                OrderId = offerRequest.OrderId,
                FareTypeRequest = new()
                {
                    FareTypeCode = offerRequest?.FareTypeRequest?.FareTypeCode,
                    FareSubTypeCode = offerRequest?.FareTypeRequest?.FareSubTypeCode
                },
                Offers = new List<OfferDetails>()
            };
        }

        public static DataList CreateStreamingDataList(OfferRequest offerRequest)
        {
            return new DataList
            {
                OriginDestinationAssociations = new(),
                FlightSegments = new(),
                OfferSegments = new(),
                BaggageAllowances = new(),
                CustomerDetails = offerRequest.CustomerDetails,
                PriceClass = new(),
                Penalty = new(),
                Journey = new()
            };
        }

        public static async Task<OfferDetails> MapStreamingSolution(
            resultBundledSolutionSummarySolution solution,
            OfferRequest offerRequest,
            DataList dataList,
            string docId,
            string offerExpiryLimit,
            ICachedCabin cachedCabin,
            ICachedPriceClass cachedPriceClass,
            ICachedPieceAllowance pieceAllowance,
            ICachedCabinBaggage cabinBaggage,
            ICachedAirportDetails cachedAirportDetails)
        {
            var requestAccountCode = offerRequest.Parties.Participants?.FirstOrDefault()?.AccountCode;
            if (!ShouldMapSolution(solution, offerRequest, requestAccountCode))
            {
                return null;
            }

            return await MapSolution(solution, offerRequest, dataList, docId, offerExpiryLimit, cachedCabin, cachedPriceClass, pieceAllowance, cabinBaggage, cachedAirportDetails);
        }

        public static void FinalizeStreamingResponse(OfferResponse response, DataList dataList, result itaResult)
        {
            response.ShoppingResponseId = $"{itaResult.session}{Constants.Application.Pipeline}{itaResult.solutionSet}";
            response.Offers = response.Offers?.Any() == true ? response.Offers : null;
            dataList.OriginDestinationAssociations = dataList.OriginDestinationAssociations.Count > 0 ? dataList.OriginDestinationAssociations : null;
            dataList.FlightSegments = dataList.FlightSegments.Count > 0 ? dataList.FlightSegments : null;
            dataList.OfferSegments = dataList.OfferSegments.Count > 0 ? dataList.OfferSegments : null;
            dataList.BaggageAllowances = dataList.BaggageAllowances.Count > 0 ? dataList.BaggageAllowances : null;
            dataList.PriceClass = dataList.PriceClass.Count > 0 ? dataList.PriceClass : null;
            dataList.Journey = dataList.Journey.Count > 0 ? dataList.Journey : null;
            dataList.Penalty = dataList.Penalty.Count > 0 ? dataList.Penalty : null;
            dataList.ClearLookupIndexes();
            response.DataList = dataList;
        }

        private static async Task<List<OfferDetails>> GetOffers(result result, OfferRequest offerRequest, DataList dataList, string docId, string offerExpiryLimit, ICachedCabin cachedCabin, ICachedPriceClass cachedPriceClass, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage, ICachedAirportDetails cachedAirportDetails)
        {
            List<OfferDetails> reshopOffers = new();
            var requestAccountCode = offerRequest.Parties.Participants?.FirstOrDefault()?.AccountCode;

            if (result.Items is { Length: > 0 })
            {
                var bundledSolnSummary = result.Items.FirstOrDefault(x => x.GetType() == typeof(resultBundledSolutionSummary)) as resultBundledSolutionSummary;

                if (bundledSolnSummary?.solution != null && bundledSolnSummary.solution.Any())
                {
                    foreach (var soln in bundledSolnSummary.solution)
                    {
                        if (!ShouldMapSolution(soln, offerRequest, requestAccountCode))
                        {
                            continue;
                        }

                        reshopOffers.Add(await MapSolution(soln, offerRequest, dataList, docId, offerExpiryLimit, cachedCabin, cachedPriceClass, pieceAllowance, cabinBaggage, cachedAirportDetails));
                    }
                }
            }
            return reshopOffers.Count > 0 ? reshopOffers : null;
        }

        private static async Task<OfferDetails> MapSolution(resultBundledSolutionSummarySolution solution, OfferRequest offerRequest, DataList dataList, string docId, string offerExpiryLimit, ICachedCabin cachedCabin, ICachedPriceClass cachedPriceClass, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage, ICachedAirportDetails cachedAirportDetails)
        {
            var firstSlice = solution.itinerary?.slice is { Length: > 0 } ? solution.itinerary.slice[0] : null;
            var firstSegment = firstSlice?.segment is { Length: > 0 } ? firstSlice.segment[0] : null;
            var firstSegmentPricing = firstSegment?.pricing is { Length: > 0 } ? firstSegment.pricing[0] : null;

            return new OfferDetails
            {
                OfferId = $"{docId}{Constants.Application.Pipeline}{solution.id}",
                SalePrice = PriceMapping.GetSalesPrice(solution),
                Selected = false,
                ItaAccountCode = solution.ext.accountCode,
                UpsellOffer = false,
                OfferItems = await GetOfferItems(solution, offerRequest, dataList, docId, cachedCabin, cachedPriceClass, pieceAllowance, cabinBaggage, cachedAirportDetails),
                OfferExpirationTimeLimit = offerExpiryLimit,
                TicketingTimeLimit = CalculateTTL(firstSegmentPricing?.latestTicketingWithImmediateReservationTime, offerRequest.TtlInfo, firstSegment?.operationalFlight?.carrier ?? string.Empty, solution),
            };
        }

        private static bool ShouldMapSolution(resultBundledSolutionSummarySolution solution, OfferRequest offerRequest, string requestAccountCode)
        {
            if (offerRequest.FlightsearchRequest.NonStopOnly)
            {
                foreach (var slice in solution.itinerary.slice)
                {
                    if (slice.segment.Length != 1)
                    {
                        return false;
                    }
                }
            }

            foreach (var requestedSegment in offerRequest.FlightsearchRequest.SearchOriginDestinations)
            {
                if (requestedSegment?.DepartureTimeRange is null)
                {
                    continue;
                }

                resultBundledSolutionSummarySolutionItinerarySlice matchingSlice = null;
                foreach (var slice in solution.itinerary.slice)
                {
                    if (slice.origin == requestedSegment.Origin && slice.destination == requestedSegment.Destination)
                    {
                        matchingSlice = slice;
                        break;
                    }
                }

                if (matchingSlice is null || DateTime.Parse($"{matchingSlice.departure.date}T{matchingSlice.departure.time}") > DateTime.Parse(requestedSegment.DepartureTimeRange.LatestTime))
                {
                    return false;
                }
            }

            if (!requestAccountCode.IsNullOrWhiteSpace() && !solution.ext.accountCode)
            {
                return false;
            }

            if (requestAccountCode.IsNullOrWhiteSpace() || !solution.ext.accountCode)
            {
                return true;
            }

            var solutionPtcs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var pricing in solution.pricing)
            {
                foreach (var fare in pricing.fare)
                {
                    foreach (var ptc in fare.ptc)
                    {
                        solutionPtcs.Add(ptc);
                    }
                }
            }

            foreach (var customer in offerRequest.CustomerDetails)
            {
                if (!solutionPtcs.Contains(customer.ItaPtc))
                {
                    return false;
                }
            }

            return true;
        }

        private static string CalculateTTL(string itaTtl, IEnumerable<TtlInfo> ttlInfo, string carrier, resultBundledSolutionSummarySolution soln)
        {
            if (!string.IsNullOrEmpty(itaTtl))
            {
                return DateTime.Parse(itaTtl).ToString(Constants.Application.ZuluDateTimeFormat);
            }

            if (!string.IsNullOrEmpty(carrier) && Constants.Application.TtlSupportedCarriers.Contains(carrier, StringComparer.OrdinalIgnoreCase))
            {
                var match = ttlInfo?.FirstOrDefault( x => string.Equals(x.OperatingCarrier, carrier, StringComparison.OrdinalIgnoreCase));

                if (match != null)
                {
                    return match.TtlDate;
                }
            }
            
            var latestTtl = soln?.itinerary?.slice.FirstOrDefault()?.segment?.FirstOrDefault()?.pricing?.FirstOrDefault()?.latestTicketingWithImmediateReservationTime;
            var departureDate = soln?.itinerary?.slice.FirstOrDefault()?.segment?.FirstOrDefault()?.departure?.date;
            var departureTime = soln?.itinerary?.slice.FirstOrDefault()?.segment?.FirstOrDefault()?.departure?.time;

            if (string.IsNullOrEmpty(latestTtl) || string.IsNullOrEmpty(departureDate) || string.IsNullOrEmpty(departureTime))
            {                
                return null; 
            }

            var offset = DateTimeOffset.Parse(latestTtl).Offset;
            var departure = Convert.ToDateTime(departureDate).Add(TimeSpan.Parse(departureTime));

            return TimeLimitMapper.GetTTL(offset, departure).ToString(Constants.Application.ZuluDateTimeFormat);
        }
        

        private static async Task<List<OfferItem>> GetOfferItems(resultBundledSolutionSummarySolution solution, OfferRequest offerRequest, DataList dataList, string docId, ICachedCabin cachedCabin, ICachedPriceClass cachedPriceClass, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage, ICachedAirportDetails cachedAirportDetails)
        {
            int offerItemId = 1;
            var airlineTaxonomies = GetAirlineTaxonomies(offerRequest);
            var offerItems = new List<OfferItem>(solution.pricing.Length);
            foreach (var pricing in solution.pricing)
            {
                string ptc = pricing.passenger[0].ptc[0];
                string currentOfferItemId = $"{docId}{Constants.Application.Pipeline}{solution.id}-{offerItemId++}";
                var services = await GetOfferServices(solution, pricing, offerRequest, ptc, dataList, cachedCabin, cachedPriceClass, pieceAllowance, cabinBaggage, cachedAirportDetails);

                offerItems.Add(new OfferItem
                {
                    AirlineTaxonomies = airlineTaxonomies,
                    OfferItemId = currentOfferItemId,
                    Services = services,
                    OfferitemPricing = OfferItemPricingMapper.GetOfferitemPricings(pricing),
                    TicketDesigCode = pricing.fare is { Length: > 0 } ? pricing.fare[0].ticketDesignator : null,
                });
            }

            return offerItems;
        }

        private static async Task<List<OfferService>> GetOfferServices(resultBundledSolutionSummarySolution solution, resultBundledSolutionSummarySolutionPricing solPricing, OfferRequest offerRequest, string Ptc, DataList dataList, ICachedCabin cachedCabin, ICachedPriceClass cachedPriceClass, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage, ICachedAirportDetails cachedAirportDetails)
        {
            int serviceId = 1;
            var services = new List<OfferService>();
            var allCodeShare = true;
            foreach (var itinerarySlice in solution.itinerary.slice)
            {
                foreach (var segment in itinerarySlice.segment)
                {
                    if (segment.operationalFlight?.carrier == null)
                    {
                        allCodeShare = false;
                        break;
                    }
                }

                if (!allCodeShare)
                {
                    break;
                }
            }

            var serviceBeneficiaries = PriceMapping.GetServiceBeneficiaries(offerRequest.CustomerDetails, Ptc);

            foreach (var slice in solution.itinerary.slice)
            {
                var point2pointVS = !slice.segment.Any(seg => seg.operationalFlight?.carrier != null);
                List<string> offerSegmentId = [];
                string journeyRefId = await ResponseDataListMapper.AddOfferOriginDestination(solPricing, slice, dataList, offerSegmentId, cachedCabin, cachedAirportDetails, offerRequest);
                var priceClassRefIds = await ResponseDataListMapper.AddpriceClassDetails(solPricing, slice, dataList, cachedPriceClass);

                foreach (var pricing in solution.pricing)
                {
                    if (pricing.passenger[0].ptc[0] != Ptc)
                    {
                        continue;
                    }
                    var baggageAllowances = await GetBaggageAllowance(pricing, slice, dataList.BaggageAllowances, pieceAllowance, cabinBaggage, point2pointVS, allCodeShare);
                    services.Add(new OfferService
                    {
                        ServiceId = Constants.Application.Service + serviceId++,
                        Product = new Product
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
                        ServiceBeneficiaries = serviceBeneficiaries,
                        PriceClassRefIds = priceClassRefIds,
                        JourneyRefIds = [journeyRefId],
                        PenaltyRef = ResponseDataListMapper.AddPenaltyDetails(pricing.fare, dataList.Penalty, slice),
                        BaggageRefIds = baggageAllowances
                    });
                }
            }
            return services;
        }

        private static async Task<List<string>> GetBaggageAllowance(resultBundledSolutionSummarySolutionPricing solutionPricing, resultBundledSolutionSummarySolutionItinerarySlice itinerary, List<BaggageAllowances> baggageAllowances, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage, bool point2pointVS, bool allCodeShare)
        {
            List<string> baggageRefIds = new();
            var itinerarySeg = itinerary.segment.Select(x => x.flight.carrier + x.flight.number);

            if (solutionPricing.checkedBaggageAllowance is not null)
            {
                foreach (var checkedBaggageAllowance in solutionPricing.checkedBaggageAllowance)
                {
                    var checkedSegments = checkedBaggageAllowance.segment.Select(x => x.flight);
                    foreach (var segment in checkedSegments)
                    {
                        if (itinerarySeg.Contains(segment))
                        {
                            string baggageRefId = await ResponseDataListMapper.AddBaggageAllowance(checkedBaggageAllowance, null, baggageAllowances, point2pointVS, allCodeShare, pieceAllowance, cabinBaggage);

                            if (baggageRefId is not null && !baggageRefIds.Contains(baggageRefId))
                            {
                                baggageRefIds.Add(baggageRefId);
                            }
                        }
                    }
                }
            }

            if (solutionPricing.carryOnBaggageAllowance is not null)
            {
                var carryOnBaggageLists = solutionPricing.carryOnBaggageAllowance.Where(x => x.freeBaggageAllowance.pieces > 0 && x.segment != null && x.segment.Any(s => itinerarySeg.Contains(s.flight))).OrderBy(x => x.freeBaggageAllowance.pieces);

                if (carryOnBaggageLists?.Any() == true)
                {
                    if (carryOnBaggageLists.Count() > 1 && carryOnBaggageLists.Select(x => x.freeBaggageAllowance.pieces).Distinct().Count() == 1)
                    {
                        var carryOnBagDict = new Dictionary<resultBundledSolutionSummarySolutionPricingCarryOnBaggageAllowance, (int? Kg, int? Lb)>();

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

                        string baggageRefId = await ResponseDataListMapper.AddBaggageAllowance(null, filteredCarryOnBaggage.Key, baggageAllowances, point2pointVS, allCodeShare, pieceAllowance, cabinBaggage);
                        if (baggageRefId != null && !baggageRefIds.Contains(baggageRefId))
                        {
                            baggageRefIds.Add(baggageRefId);
                        }
                    }
                    else
                    {
                        string baggageRefId = await ResponseDataListMapper.AddBaggageAllowance(null, carryOnBaggageLists.First(), baggageAllowances, point2pointVS, allCodeShare, pieceAllowance, cabinBaggage);
                        if (baggageRefId != null && !baggageRefIds.Contains(baggageRefId))
                        {
                            baggageRefIds.Add(baggageRefId);
                        }
                    }
                }
            }

            return baggageRefIds.Count > 0 ? baggageRefIds : null;
        }

        private static IEnumerable<AirlineTaxonomy> GetAirlineTaxonomies(OfferRequest offerRequest)
        {
            if (offerRequest.ServiceProducts is null)
            {
                return null;
            }

            var airlineTaxonomies = new List<AirlineTaxonomy>();
            foreach (var product in offerRequest.ServiceProducts)
            {
                if (product.IncludeInd)
                {
                    airlineTaxonomies.Add(new AirlineTaxonomy { TaxonomyCode = product.ProductCode });
                }
            }

            return airlineTaxonomies.Count > 0 ? airlineTaxonomies : null;
        }
    }
}
