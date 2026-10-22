using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Common;
using OriginDestination = VAA.EAPI.Domain.Virgin.Models.Products.Flight.OriginDestination;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Reshop
{
    public class ReshopBundleResponseMapper
    {
        public static async Task<OfferResponse> ReshopResponseMapper(search itaRequest, result result, ReshopRequest reshopRequest, ICachedPriceClass _cachedPriceClass, ICachedCabin _cachedCabin, string docId, string offerExpirationLimit, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage, ICachedAirportDetails cachedAirportDetails)
        {
            var reshopResponse = new OfferResponse
            {
                Pos = reshopRequest.Pos,
                Parties = reshopRequest.Parties,
                OrderId = reshopRequest.OrderId,
                FareTypeRequest = new()
                {
                    FareTypeCode = reshopRequest?.FareTypeRequest?.FareTypeCode,
                    FareSubTypeCode = reshopRequest?.FareTypeRequest?.FareSubTypeCode
                },
                ShoppingResponseId = $"{result.session}{Constants.Application.Pipeline}{result.solutionSet}"
            };
            if (result.Items != null && result.Items.Count() > 0)
            {
                DataList dataList = new()
                {
                    OriginDestinationAssociations = new(),
                    FlightSegments = new(),
                    OfferSegments = new(),
                    BaggageAllowances = new(),
                    CustomerDetails = reshopRequest.CustomerDetails,
                    PriceClass = new(),
                    Penalty = new(),
                    Journey = new()
                };
                reshopResponse.Offers = await GetReshopOffers(itaRequest, result, reshopRequest, dataList, _cachedPriceClass, _cachedCabin, docId, offerExpirationLimit, pieceAllowance, cabinBaggage, cachedAirportDetails);
                dataList.OriginDestinationAssociations = dataList.OriginDestinationAssociations.Count > 0 ? dataList.OriginDestinationAssociations : null;
                dataList.FlightSegments = dataList.FlightSegments.Count > 0 ? dataList.FlightSegments : null;
                dataList.OfferSegments = dataList.OfferSegments.Count > 0 ? dataList.OfferSegments : null;
                dataList.BaggageAllowances = dataList.BaggageAllowances.Count > 0 ? dataList.BaggageAllowances : null;
                dataList.PriceClass = dataList.PriceClass.Count > 0 ? dataList.PriceClass : null;
                dataList.Penalty = dataList.Penalty.Count > 0 ? dataList.Penalty : null;
                dataList.Journey = dataList.Journey.Count > 0 ? dataList.Journey : null;
                reshopResponse.DataList = dataList;
            }
            return reshopResponse;
        }

        private static async Task<List<OfferDetails>> GetReshopOffers(search itaRequest, result result, ReshopRequest reshopRequest, DataList dataList, ICachedPriceClass _cachedPriceClass, ICachedCabin _cachedCabin, string docId, string offerExpirationLimit, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage, ICachedAirportDetails cachedAirportDetails)
        {
            List<OfferDetails> reshopOffers = new();

            var requestAccountCode = reshopRequest.Parties?.Participants?.FirstOrDefault()?.AccountCode;
            var bundledSolnSummary = result.Items.FirstOrDefault(x => x.GetType() == typeof(resultBundledSolutionSummary)) as resultBundledSolutionSummary;
            if (bundledSolnSummary?.solution != null && bundledSolnSummary.solution.Any())
            {
                var solutions = reshopRequest.ReshopQueries.FlightSpecific.SearchCriteria.NonStopOnly ? bundledSolnSummary.solution.Where(x => x.itinerary.slice.All(y => y.segment.Length == 1)).ToArray() : bundledSolnSummary.solution;
                var filteredOffers = reshopRequest.OriginDestinationAssociations.Any(x => x.OriginDestination.DepartureTimeRange != null) ? GetTimeFilteredOffers(itaRequest, solutions, reshopRequest) : solutions;
                foreach (var soln in filteredOffers)
                {
                    if (soln.pricing.Any(pricing => pricing.saleFareDifference != null))
                    {
                        var itaTtl = soln?.itinerary?.slice.FirstOrDefault()?.segment?.FirstOrDefault()?.pricing?.FirstOrDefault()?.latestTicketingWithImmediateReservationTime;

                        if (requestAccountCode == null || soln.ext.accountCode)
                        {
                            reshopOffers.Add(new OfferDetails
                            {
                                OfferId = $"{docId}{Constants.Application.Pipeline}{soln.id}",
                                RecordLocatorId = reshopRequest?.BookingReferences?.RLOC,
                                AveragePassengerPrice = PriceMapping.GetAveragePassengerPrice(soln.ext),
                                HighestPassengerPrice = PriceMapping.GetHighestPassengerPrice(soln.ext),
                                SalePrice = PriceMapping.GetSalesPrice(soln),
                                SalePriceDifferences = GetSalesPriceDifferences(soln),
                                Selected = false,
                                UpsellOffer = false,
                                OfferItems = await GetOfferItems(soln, reshopRequest, dataList, _cachedPriceClass, _cachedCabin, docId, pieceAllowance, cabinBaggage, cachedAirportDetails),
                                OfferExpirationTimeLimit = offerExpirationLimit,
                                TicketingTimeLimit = CalculateTTL(itaTtl , soln),
                            });
                        }
                    }
                }
            }
            return reshopOffers.Count > 0 ? reshopOffers : null;
        }

        private static string CalculateTTL(string itaTtl, resultBundledSolutionSummarySolution soln)
        {
            if (!string.IsNullOrEmpty(itaTtl))
            {
                return DateTime.Parse(itaTtl).ToString(Constants.Application.ZuluDateTimeFormat);
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

        private static List<Price> GetSalesPriceDifferences(resultBundledSolutionSummarySolution solution)
        {
            List<Price> saleDifferences = null;
            decimal saleFareDifference = 0;
            decimal saleTaxTotalDifference = 0;
            string baseCurrency = string.Empty;
            foreach (var pricing in solution.pricing)
            {
                baseCurrency = pricing.saleFareDifference?.currency;
                if (pricing.saleFareDifference?.amount != null)
                {
                    saleFareDifference = (decimal)(saleFareDifference + pricing.saleFareDifference?.amount * pricing.paxCount);
                }
                if (pricing.saleTaxTotalDifference?.amount != null)
                {
                    saleTaxTotalDifference = (decimal)(saleTaxTotalDifference + pricing.saleTaxTotalDifference?.amount * pricing.paxCount);
                }
            }
            if (saleFareDifference != 0)
            {
                saleDifferences ??= new();
                saleDifferences.Add(
                new Price
                {
                    Amount = new()
                    {
                        BaseCurrency = baseCurrency,
                        MonetaryAmount = saleFareDifference.ToString()
                    },
                    PriceComponentName = string.Empty,
                    PriceComponentType = Price.PriceComponentTypes.BaseFareEnum
                });
            }
            if (saleTaxTotalDifference != 0)
            {
                saleDifferences ??= new();
                saleDifferences.Add(
                new Price
                {
                    Amount = new()
                    {
                        BaseCurrency = baseCurrency,
                        MonetaryAmount = saleTaxTotalDifference.ToString()
                    },
                    PriceComponentName = string.Empty,
                    PriceComponentType = Price.PriceComponentTypes.TotalTaxEnum
                });
            }
            return saleDifferences;
        }
        private static async Task<List<OfferItem>> GetOfferItems(resultBundledSolutionSummarySolution solution, ReshopRequest reshopRequest, DataList dataList, ICachedPriceClass _cachedPriceClass, ICachedCabin _cachedCabin, string docId, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage, ICachedAirportDetails cachedAirportDetails)
        {
            int offerItemId = 1;
            string currentPTC = string.Empty;
            List<OfferItem> offerItems = new();
            var distinctPricing = solution?.pricing?.GroupBy(x => x.passenger?.FirstOrDefault()?.ptc?.FirstOrDefault()).Select(x => x?.FirstOrDefault());
            foreach (var pricing in distinctPricing)
            {
                currentPTC = pricing.passenger.FirstOrDefault().ptc.FirstOrDefault();
                offerItems.Add(new OfferItem
                {
                    OfferItemId = $"{docId}{Constants.Application.Pipeline}{solution.id}-{offerItemId++}",
                    Services = await GetOfferServices(pricing, solution, reshopRequest, currentPTC, dataList, _cachedPriceClass, _cachedCabin, pieceAllowance, cabinBaggage, cachedAirportDetails),
                    OfferitemPricing = ReshopOfferItemPricingMapper.GetOfferitemPricings(pricing, reshopRequest, currentPTC),
                    TicketDesigCode = pricing.fare?.FirstOrDefault()?.ticketDesignator,
                });
            }
            return offerItems;
        }
        private static async Task<List<OfferService>> GetOfferServices(resultBundledSolutionSummarySolutionPricing tripPricing, resultBundledSolutionSummarySolution solution, ReshopRequest reshopRequest, string Ptc, DataList dataList, ICachedPriceClass _cachedPriceClass, ICachedCabin _cachedCabin, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage, ICachedAirportDetails cachedAirportDetails)
        {
            int serviceId = 1;
            var services = new List<OfferService>();

            List<string> flownFlights = !reshopRequest.IsReshopHoldBooking ? GetFlownFlightsFromCoupon(reshopRequest) : null;
            List<OriginDestination> flownoriginDestinations = !reshopRequest.IsReshopHoldBooking ? GetflownoriginDestinations(reshopRequest) : null;

            var segments = new List<resultBundledSolutionSummarySolutionItinerarySliceSegment>();
            foreach (var itn in solution.itinerary.slice)
            {
                segments.AddRange(itn.segment);
            }
            var allCodeShare = !segments.Any(seg => seg.operationalFlight?.carrier == null);
            foreach (var itinerary in solution.itinerary.slice)
            {
                var point2pointVS = !itinerary.segment.Any(seg => seg.operationalFlight?.carrier != null);

                List<string> offerSegmentId = new();
                string journeyRefId = await AddReshopOriginDestination(tripPricing, itinerary, dataList, offerSegmentId, _cachedCabin, cachedAirportDetails, reshopRequest);
                foreach (var pricing in solution.pricing)
                {
                    if (pricing.passenger?.FirstOrDefault()?.ptc?.FirstOrDefault() == Ptc)
                    {
                        var priceClassRefIds = await ResponseDataListMapper.AddpriceClassDetails(pricing, itinerary, dataList.PriceClass, _cachedPriceClass);
                        var baggageAllowences = await GetBaggageAllowance(pricing, itinerary, dataList, flownFlights, pieceAllowance, cabinBaggage, point2pointVS, allCodeShare);
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
                            ServiceBeneficiaries = PriceMapping.GetServiceBeneficiaries(reshopRequest.CustomerDetails, Ptc),
                            JourneyRefIds = new List<string> { journeyRefId },
                            PenaltyRef = (pricing?.penaltyPrice != null) ? ResponseDataListMapper.AddReshopPenaltyDetails(pricing, dataList?.Penalty, itinerary) : null,
                            PriceClassRefIds = priceClassRefIds,
                            BaggageRefIds = flownoriginDestinations is not null ? flownoriginDestinations.Any(item => item.OriginAirport == itinerary.origin && item.DestinationAirport == itinerary.destination) ? null : baggageAllowences : baggageAllowences// Hiding Baggage in case of Flown scenario
                        });
                    }
                }
            }
            return services;
        }

        private static async Task<List<string>> GetBaggageAllowance(resultBundledSolutionSummarySolutionPricing solutionPricing, resultBundledSolutionSummarySolutionItinerarySlice itinerary, DataList dataList, List<string> flownFlights, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage, bool point2pointVS, bool allCodeShare)
        {
            List<string> baggageRefIds = new();
            var itinerarySeg = itinerary.segment.Select(x => x.flight.carrier + x.flight.number);

            if (solutionPricing.checkedBaggageAllowance != null)
            {
                foreach (var checkedBaggageAllowance in solutionPricing.checkedBaggageAllowance)
                {
                    var checkedSegments = checkedBaggageAllowance.segment.Select(x => x.flight);
                    foreach (var segment in checkedSegments)
                    {
                        if (itinerarySeg.Contains(segment) && (flownFlights is null || !checkedBaggageAllowance.segment.All(q => flownFlights.Contains(q?.flight))))
                        {
                            string baggageRefId = await ResponseDataListMapper.AddBaggageAllowance(checkedBaggageAllowance, null, dataList.BaggageAllowances, point2pointVS, allCodeShare, pieceAllowance, cabinBaggage);

                            if (baggageRefId != null && !baggageRefIds.Contains(baggageRefId))
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

                        string baggageRefId = await ResponseDataListMapper.AddBaggageAllowance(null, filteredCarryOnBaggage.Key, dataList.BaggageAllowances, point2pointVS, allCodeShare, pieceAllowance, cabinBaggage);
                        if (baggageRefId != null && !baggageRefIds.Contains(baggageRefId))
                        {
                            baggageRefIds.Add(baggageRefId);
                        }
                    }
                    else
                    {
                        string baggageRefId = await ResponseDataListMapper.AddBaggageAllowance(null, carryOnBaggageLists.First(), dataList.BaggageAllowances, point2pointVS, allCodeShare, pieceAllowance, cabinBaggage);
                        if (baggageRefId != null && !baggageRefIds.Contains(baggageRefId))
                        {
                            baggageRefIds.Add(baggageRefId);
                        }
                    }
                }
            }
            return baggageRefIds.Count() == 0 ? null : baggageRefIds;
        }

        private static List<string> GetFlownFlightsFromCoupon(ReshopRequest reshopRequest)
        {
            List<string> flownFlights = new();
            foreach (var item in reshopRequest.ReshopOrder.Where(ro => ro.OrderProductType == Constants.Application.Flight))
            {
                foreach (var coupon in item.AccountableDocumentRefs.FirstOrDefault().Coupons)
                {
                    if (Constants.Application.Flownstatus.Contains(coupon.CouponStatus.ToUpper()))
                    {
                        string flownFlight = $"{Constants.Application.VsFlightCode}{reshopRequest.SegmentDetails.FirstOrDefault(t => t.SegmentId == coupon.segmentRefId)?.MarketingFlightNumber.TrimStart('0')}";
                        if (!flownFlights.Contains(flownFlight))
                            flownFlights.Add(flownFlight);
                    }
                }
            }
            return flownFlights;
        }
        private static List<OriginDestination> GetflownoriginDestinations(ReshopRequest reshopRequest)
        {
            List<OriginDestination> flownOriginDestinations = new();
            foreach (var item in reshopRequest.ReshopOrder.Where(ro => ro.OrderProductType == Constants.Application.Flight))
            {
                foreach (var coupon in item.AccountableDocumentRefs.FirstOrDefault().Coupons)
                {
                    if (Constants.Application.Flownstatus.Contains(coupon.CouponStatus.ToUpper()))
                    {
                        OriginDestination originDestination = new();
                        originDestination.DestinationAirport = reshopRequest.SegmentDetails.FirstOrDefault(t => t.SegmentId == coupon.segmentRefId)?.DestinationAirportCode;
                        originDestination.OriginAirport = reshopRequest.SegmentDetails.FirstOrDefault(t => t.SegmentId == coupon.segmentRefId)?.OriginAirportCode;

                        if (!flownOriginDestinations.Contains(originDestination))
                            flownOriginDestinations.Add(originDestination);
                    }
                }
            }
            return flownOriginDestinations;
        }

        private static IEnumerable<resultBundledSolutionSummarySolution> GetTimeFilteredOffers(search itaRequest, IEnumerable<resultBundledSolutionSummarySolution> offers, ReshopRequest reshopRequest)
        {
            List<resultBundledSolutionSummarySolution> timeFilteredOffers = new();
            var journeyId = reshopRequest.ReshopQueries.FlightSpecific.FlightsRequest.Add.Select(x => x.OriginDestinationId);
            int counter = 0;
            foreach (var id in journeyId)
            {
                bool ind = false;
                var Ond = reshopRequest.OriginDestinationAssociations.Where(x => x.OdLabel == id);
                foreach (var slice in itaRequest.inputs.slice)
                {
                    if (Ond.First().OriginDestination.OriginAirport == slice?.origin && Ond.First().OriginDestination.DestinationAirport == slice?.destination)
                    {
                        ind = true;
                        break;
                    }
                }

                if (Ond.Any() && Ond.First().OriginDestination.DepartureTimeRange != null && ind)
                {
                    var seg = Ond.First().OriginDestination;
                    var offer = offers.Select(x => x.itinerary.slice[counter]);
                    timeFilteredOffers = timeFilteredOffers.Any() ? timeFilteredOffers.Where(x => DateTime.Parse(x.itinerary.slice[counter].departure.date + "T" + x.itinerary .slice[counter].departure.time) <= DateTime.Parse(seg.DepartureTimeRange.LatestTime)).ToList() : offers.Where(x => DateTime.Parse(x.itinerary.slice[counter].departure.date + "T" + x.itinerary.slice[counter].departure.time) <= DateTime.Parse(seg.DepartureTimeRange.LatestTime)).ToList();
                }

                counter++;
                if (!timeFilteredOffers.Any()) break;
            }

            return timeFilteredOffers;
        }

        public static async Task<string> AddReshopOriginDestination(resultBundledSolutionSummarySolutionPricing pricing, resultBundledSolutionSummarySolutionItinerarySlice slice, DataList dataList, List<string> offerSegmentIds, ICachedCabin cachedCabin, ICachedAirportDetails cachedAirportDetails, ReshopRequest reshopRequest)
        {
            int lastOriginDestRefID = dataList.OriginDestinationAssociations.Any() ? Convert.ToInt32(dataList.OriginDestinationAssociations.Last().OdLabel[2..]) : 0;
            string originDestRefID = $"{Constants.Application.OriginDestination}{++lastOriginDestRefID}";

            if (!dataList.OriginDestinationAssociations.Any(Od => (Od.OriginDestination.OriginAirport == slice.origin) && (Od.OriginDestination.DestinationAirport == slice.destination) && Od.OriginDestination.DepartureDate == slice.departure.date))
            {
                dataList.OriginDestinationAssociations.Add(new()
                {
                    OdLabel = originDestRefID,
                    OriginDestination = new()
                    {
                        OriginAirport = slice.origin,
                        DestinationAirport = slice.destination,
                        DepartureDate = slice.departure.date
                    },
                    JourneyRefIds = new()
                });
            }
            else
            {
                originDestRefID = dataList.OriginDestinationAssociations.FirstOrDefault(Od => (Od.OriginDestination.OriginAirport == slice.origin) && (Od.OriginDestination.DestinationAirport == slice.destination) && Od.OriginDestination.DepartureDate == slice.departure.date).OdLabel;
            }
            string journeyRefId = await ResponseDataListMapper.AddJourneyAndSegmentDetails(pricing, slice, dataList, originDestRefID, offerSegmentIds, cachedCabin);
            if (!dataList.OriginDestinationAssociations.Any(od => od.JourneyRefIds != null && od.JourneyRefIds.Contains(journeyRefId)))
            {
                dataList.OriginDestinationAssociations.FirstOrDefault(od => od.OdLabel == originDestRefID).JourneyRefIds.Add(journeyRefId);
            }
            return journeyRefId;
        }
    }
}