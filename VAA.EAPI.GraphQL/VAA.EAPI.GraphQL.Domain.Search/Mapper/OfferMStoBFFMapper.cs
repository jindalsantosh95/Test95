using System.Web;
using System.Xml;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Search.Helpers;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.CalendarSearch;
using VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.FlexiDateSearch;

namespace VAA.EAPI.GraphQL.Domain.Search.Mapper
{
    public class OfferMStoBFFMapper
    {
        private static readonly string[] fareFamilies = new[]
        {
            "ECONOMY-LIGHT",
            "ECON",
            "ECONOMY-DELIGHT",
            "COMFORT-PLUS-PREMIUM-ECON",
            "BUS-FIRST"
        };

        private static readonly string[] awardFareFamilies = new[]
        {
            "AWARD-ECONOMY",
            "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY",
            "AWARD-BUSINESS-FIRST"
        };

        private static readonly Dictionary<string, string[]> fareFamilyOrBetter = new Dictionary<string, string[]>
        {
            { "ECONOMY-LIGHT", new string[] { "ECONOMY-LIGHT", "ECON", "ECON-CLASSIC-FLEXIBLE", "ECONOMY-DELIGHT", "ECON-DELIGHT-FLEXIBLE", "COMFORT-PLUS-PREMIUM-ECON", "COMFORT-PLUS-PREMIUM-FLEXIBLE", "BUS-FIRST", "BUS-FIRST-FLEXIBLE" } },
            { "ECON", new string[] { "ECON", "ECON-CLASSIC-FLEXIBLE", "ECONOMY-DELIGHT", "ECON-DELIGHT-FLEXIBLE", "COMFORT-PLUS-PREMIUM-ECON", "COMFORT-PLUS-PREMIUM-FLEXIBLE", "BUS-FIRST", "BUS-FIRST-FLEXIBLE" } },
            { "ECONOMY-DELIGHT", new string[] { "ECONOMY-DELIGHT", "ECON-DELIGHT-FLEXIBLE", "COMFORT-PLUS-PREMIUM-ECON", "COMFORT-PLUS-PREMIUM-FLEXIBLE", "BUS-FIRST", "BUS-FIRST-FLEXIBLE" } },
            { "COMFORT-PLUS-PREMIUM-ECON", new string[] {"COMFORT-PLUS-PREMIUM-ECON", "COMFORT-PLUS-PREMIUM-FLEXIBLE", "BUS-FIRST", "BUS-FIRST-FLEXIBLE" } },
            { "BUS-FIRST", new string[]{ "BUS-FIRST", "BUS-FIRST-FLEXIBLE" } },
            { "AWARD-ECONOMY", new string[]{ "AWARD-ECONOMY", "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY", "AWARD-BUSINESS-FIRST" } },
            { "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY", new string[]{ "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY", "AWARD-BUSINESS-FIRST" } },
            { "AWARD-BUSINESS-FIRST", new string[]{ "AWARD-BUSINESS-FIRST" } }
        };

        private static readonly Dictionary<string, string> cabinDisplayNames = new Dictionary<string, string>
        {
            { "ECONOMY-LIGHT", "Economy Light" },
            { "ECON", "Economy Classic" },
            { "ECONOMY-DELIGHT", "Economy Delight" },
            { "COMFORT-PLUS-PREMIUM-ECON", "Premium" },
            { "BUS-FIRST", "Upper Class" },
            { "AWARD-ECONOMY", "Economy Classic"},
            { "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY", "Premium"},
            { "AWARD-BUSINESS-FIRST", "Upper Class"}
        };

        // Maps from specific fare family to the one that we use to represent the group of fare families on the filters (e.g. there is one filter option to cover both BUS-FIRST and BUS-FIRST-FLEXIBLE).
        private static readonly Dictionary<string, string> fareFamilyForFilter = new Dictionary<string, string>
        {
            { "ECONOMY-LIGHT", "ECONOMY-LIGHT" },
            { "ECON", "ECON" },
            { "ECON-CLASSIC-FLEXIBLE", "ECON" },
            { "ECONOMY-DELIGHT", "ECONOMY-DELIGHT" },
            { "ECON-DELIGHT-FLEXIBLE", "ECONOMY-DELIGHT" },
            { "COMFORT-PLUS-PREMIUM-ECON", "COMFORT-PLUS-PREMIUM-ECON" },
            { "COMFORT-PLUS-PREMIUM-FLEXIBLE", "COMFORT-PLUS-PREMIUM-ECON" },
            { "BUS-FIRST", "BUS-FIRST" },
            { "BUS-FIRST-FLEXIBLE", "BUS-FIRST" },
            { "AWARD-ECONOMY", "AWARD-ECONOMY"},
            { "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY", "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY"},
            { "AWARD-BUSINESS-FIRST", "AWARD-BUSINESS-FIRST"}
        };

        private static readonly Dictionary<string, string> FareFamilyandFlex = new Dictionary<string, string>
        {
            { "ECONOMY-LIGHT", "ECONOMY-LIGHT"},
            { "ECON", "ECON-CLASSIC-FLEXIBLE" },
            { "ECONOMY-DELIGHT", "ECON-DELIGHT-FLEXIBLE" },
            { "COMFORT-PLUS-PREMIUM-ECON", "COMFORT-PLUS-PREMIUM-FLEXIBLE" },
            { "BUS-FIRST", "BUS-FIRST-FLEXIBLE" },
            { "AWARD-ECONOMY", "AWARD-ECONOMY"},
            { "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY", "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY"},
            { "AWARD-BUSINESS-FIRST", "AWARD-BUSINESS-FIRST"}
        };

        public static SearchResults Map(FlightSearchResponse response, IEnumerable<SearchOriginDestination>? searchOriginDestinations, int totalOnDs, int currentSlice, Guid basketID, string passengerCounts, string offerID, string selectedSort, FlightOfferRequestFilters selectedFilters, IEnumerable<AirportInfo> airportInfo, IEnumerable<AircraftInfo> aircraftInfo, IEnumerable<AirlineInfo> airlineInfo, IEnumerable<CabinInfo> cabinInfo)
        {
            var requestDate = searchOriginDestinations.First().DepartureDate;
            return new()
            {
                PriceGrid = GetFlexiDateResponse(searchOriginDestinations, response.FlexiDateSearchResponse, airportInfo),
                Result = GetResult(searchOriginDestinations, response.FlightOfferResponse, totalOnDs, currentSlice, basketID, passengerCounts, offerID, selectedSort, selectedFilters, airportInfo, airlineInfo, aircraftInfo, cabinInfo),
                Calendar = GetCalendar(response.CalendarSearchResponse, requestDate),
                Links = GetSelfLink(basketID.ToString(), totalOnDs, currentSlice, offerID)
            };
        }

        private static PriceGrid? GetFlexiDateResponse(IEnumerable<SearchOriginDestination> searchOriginDestinations, FlexiDateSearchResponse? response, IEnumerable<AirportInfo> airportInfo)
        {
            if (response?.FlexibleDateCalendar?.DepartureDetails != null && !response.FlexibleDateCalendar.DepartureDetails.Any(d => d.DepartureDate == null))
            {
                var x = new PriceGrid()
                {
                    Returning = response.FlexibleDateCalendar.ColumnText.Select(x => x.Parse<DateOnly>()),
                    Departures = GetDepartures(response.FlexibleDateCalendar.DepartureDetails, response.isAwardSearch),
                    Criteria = searchOriginDestinations.OrderBy(sod => sod.DepartureDate).Select(ond => new Criteria(ond, airportInfo))
                };
                return x;
            }
            else
                return null;
        }

        private static IEnumerable<PriceGridDeparture> GetDepartures(IEnumerable<DepartureDetails> response, bool isAwardSearch)
        {
            return response.Select(departure => new PriceGridDeparture()
            {
                departing = DateOnly.FromDateTime(departure.DepartureDate.Value.Date),
                prices = departure.FlexibleDatePriceCalendarCell.Any()
                    ? GetPriceGridPrices(departure.FlexibleDatePriceCalendarCell, isAwardSearch)
                    : new PriceGridPrice[7]
            });
        }

        private static IEnumerable<PriceGridPrice?> GetPriceGridPrices(IEnumerable<FlexibleDatePriceCalendar> response, bool isAwardSearch)
        {
            return response.Select(price => price.Availability
                ? new PriceGridPrice()
                {
                    minPrice = price.LowestPriceOfferIndicator,
                    price = new()
                    {
                        Amount = Convert.ToDecimal(price.SalePrice.First(price => price.PriceComponentType == Models.FlightOffer.Price.PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount),
                        Currency = price.SalePrice.First(price => price.PriceComponentType == Models.FlightOffer.Price.PriceComponentTypes.BasePriceEnum).Amount.BaseCurrency,
                        AwardPoints = isAwardSearch ? price.SalePrice.First(price => price.PriceComponentType == Models.FlightOffer.Price.PriceComponentTypes.PointsEnum).PointsAmount.PointsCount.ToString() : null
                    }
                }
                : null
            );
        }

        private static Result GetResult(IEnumerable<SearchOriginDestination> searchOriginDestinations, FlightOfferResponse? response, int totalOnDs, int currentSlice, Guid basketID, string passengerCounts, string offerID, string selectedSort, FlightOfferRequestFilters selectedFilter, IEnumerable<AirportInfo> airportInfo, IEnumerable<AirlineInfo> airlineInfo, IEnumerable<AircraftInfo> aircraftInfo, IEnumerable<CabinInfo> cabinInfo)
        {
            if (response == null) return null;

            selectedFilter ??= new FlightOfferRequestFilters();
            if (selectedFilter.Cabins == null || fareFamilyOrBetter.ContainsKey(selectedFilter.Cabins) == false)
            {
                var outboundDominantFare = response.Offers?.FirstOrDefault(o => o.Selected)?.OfferItems.FirstOrDefault()?.Services.FirstOrDefault()?.FareDetails?.DominantFareProduct;
                var outboundFareFamily = cabinInfo.FirstOrDefault(x => x.BrandID == outboundDominantFare)?.FareFamily;
                if (outboundFareFamily != null && fareFamilyForFilter.ContainsKey(outboundFareFamily))
                {
                    selectedFilter.Cabins = fareFamilyForFilter[outboundFareFamily];
                }
                else
                {
                    selectedFilter.Cabins = response.IsAwardSearchResult ? awardFareFamilies.First() : fareFamilies.First();
                }
            }
            string[] selectedFareFamilies = fareFamilyOrBetter[selectedFilter.Cabins];

            var ond = searchOriginDestinations.OrderBy(sod => sod.DepartureDate).ElementAt(currentSlice);

            Result result = new()
            {
                Slices = new()
                {
                    current = currentSlice,
                    total = totalOnDs
                },
                Slice = new()
                {
                    FlightsAndFares = GetFlightsAndFares(response, totalOnDs, currentSlice, basketID.ToString(), passengerCounts, selectedFareFamilies, cabinInfo, aircraftInfo, airlineInfo, airportInfo),
                    Id = currentSlice == 0 || !response.Offers.Any(o => o.Selected) ? null : basketID.ToString(),
                    FareId = currentSlice == 0 || !response.Offers.Any(o => o.Selected) ? null : response.Offers.First(o => o.Selected).OfferId
                },
                TripSummary = GetTripSummary(response, currentSlice, cabinInfo),
                Criteria = new Criteria(ond, airportInfo)
            };

            if (currentSlice > 0)
                result.Links = GetSelectandNextSelfLink(basketID.ToString(), currentSlice, totalOnDs, offerID);

            result.Slice.FlightsAndFares = FilterNotOfferdFlights(result);

            result.FlightSortLinks = GetFlightSortLinks(selectedSort, selectedFilter, totalOnDs, currentSlice, basketID.ToString(), offerID, response.IsAwardSearchResult);

            if (basketID != null)
            {
                var baseUrl = GetBaseUrl(totalOnDs, currentSlice, basketID.ToString(), offerID);

                result.FlightSearchFilters = GetFlightSearchFilters(result.Slice.FlightsAndFares, selectedSort, selectedFilter, baseUrl, response.IsAwardSearchResult);
                result.BasketId = basketID;
            }

            if (!selectedSort.IsNullOrWhiteSpace())
                result.Slice.FlightsAndFares = SortData(result.Slice.FlightsAndFares, selectedSort);

            var offerFareFamilies = response.IsAwardSearchResult ? awardFareFamilies : fareFamilies;

            foreach (var ff in result.Slice.FlightsAndFares)
                foreach (var fareFamily in offerFareFamilies)
                    if (!ff.Fares.Select(x => x.FareFamilyType).Contains(fareFamily))
                    {
                        ff.Fares = ff.Fares.Append(new SliceFare()
                        {
                            FareFamilyType = fareFamily,
                            Links = Enumerable.Empty<Link>(),
                            Availability = Constants.Application.NOT_OFFERED
                        });
                    }

            result.Slice.FareFamilyTypes = result.Slice.FlightsAndFares.SelectMany(x => x.Fares.Select(f => f.FareFamilyType)).Distinct();

            result.FlightSearchFilters.Total = result.Slice.FlightsAndFares.Count(); // Count before filtering
            result.Slice.FlightsAndFares = FilterData(result.Slice.FlightsAndFares, selectedFilter);
            result.FlightSearchFilters.Count = result.Slice.FlightsAndFares.Count(); // Count after filtering

            return result;
        }

        private static IEnumerable<SliceFlightsAndFares> FilterNotOfferdFlights(Result result)
        {
            return result.Slice.FlightsAndFares.Except(result.Slice.FlightsAndFares.Where(f => f.Fares.All(x => x.Availability == Constants.Application.NOT_OFFERED)));
        }

        private static IEnumerable<SliceFlightsAndFares> SortData(IEnumerable<SliceFlightsAndFares> flights, string selectedSort)
        {
            return selectedSort switch
            {
                "PRICE" => flights.Select(f =>
                {
                    f.Fares = f.Fares.OrderByDescending(o => o.Price?.AmountIncludingTax.HasValue).ThenBy(o => o.Price?.AmountIncludingTax);
                    return f;
                }).OrderBy(x => x.Fares.First()?.Price?.AmountIncludingTax),

                "POINTS" => flights.Select(f =>
                {
                    f.Fares = f.Fares.OrderByDescending(o => Convert.ToInt32(!o.Price?.AwardPoints.IsNullOrEmpty())).ThenBy(o => Convert.ToInt32(o.Price?.AwardPoints));
                    return f;
                }).OrderBy(x => Convert.ToInt32(x.Fares.First()?.Price?.AwardPoints)),

                "DEPARTURE_TIME" => flights.OrderBy(o => o.Flight.Departure.Value).ThenBy(o => XmlConvert.ToTimeSpan(o.Flight.Duration)),

                "DURATION" => flights.OrderBy(o => XmlConvert.ToTimeSpan(o.Flight.Duration)).ThenBy(o => o.Flight.Departure.Value),
                _ => flights,
            };
        }

        private static IEnumerable<SliceFlightsAndFares> FilterData(IEnumerable<SliceFlightsAndFares> flights, Models.FlightOffer.FlightOfferRequestFilters filters)
        {
            if (filters?.Airlines != null && filters.Airlines != Constants.Application.FILTER_PLACEHOLDER)
                flights = FilterAirlines(flights, filters.Airlines.Split(Constants.Application.FILTER_SEPARATOR));

            if (filters?.DepartureTimes != null && filters.DepartureTimes != Constants.Application.FILTER_PLACEHOLDER)
                flights = FilterDepartureTimes(flights, filters.DepartureTimes.Split(Constants.Application.FILTER_SEPARATOR));

            if (filters?.Connections != null && filters.Connections != Constants.Application.FILTER_PLACEHOLDER)
                flights = FilterConnections(flights, filters.Connections.Split(Constants.Application.FILTER_SEPARATOR));

            if (filters?.Cabins != null && filters.Cabins != Constants.Application.FILTER_PLACEHOLDER && ConfigReader.Try<bool>("EnableCabinFilters"))
                flights = FilterCabins(flights, filters.Cabins.Split(Constants.Application.FILTER_SEPARATOR));

            return flights;
        }

        private static IEnumerable<SliceFlightsAndFares> FilterAirlines(IEnumerable<SliceFlightsAndFares> flights, string[] airlineCodes)
        {
            return flights.Where(faf => !airlineCodes.Any() || faf.Flight.Segments.Any(s => airlineCodes.Contains(s.operatingAirline.code)));
        }

        // Returns the start of the sort URL, to which the selected sort and filter options can be added.
        private static string GetBaseUrl(int totalOnDs, int currentSlice, string basketID, string offerID)
        {
            var sliceData = $"slice{totalOnDs}.{currentSlice}";
            var fareId = offerID.IsNullOrEmpty() ? sliceData : $"{sliceData}|{offerID}";
            return $"{PostbackUrl.Get()}/{basketID}/sort/{fareId}";
        }

        private static string GetFilterPath(string? airlinesFilter, string? departureTimesFilter, string? connectionsFilter, string? cabinssFilter)
        {
            if (airlinesFilter.IsNullOrEmpty()) airlinesFilter = Constants.Application.FILTER_PLACEHOLDER;
            if (departureTimesFilter.IsNullOrEmpty()) departureTimesFilter = Constants.Application.FILTER_PLACEHOLDER;
            if (connectionsFilter.IsNullOrEmpty()) connectionsFilter = Constants.Application.FILTER_PLACEHOLDER;
            if (cabinssFilter.IsNullOrEmpty()) cabinssFilter = Constants.Application.FILTER_PLACEHOLDER;

            return $"{airlinesFilter}/{departureTimesFilter}/{connectionsFilter}/{HttpUtility.UrlEncode(cabinssFilter)}";
        }

        private static FlightSortLinks GetFlightSortLinks(string selectedSort, Models.FlightOffer.FlightOfferRequestFilters selectedFilters, int totalOnDs, int currentSlice, string basketID, string offerID, bool isAwardSearch)
        {
            return new FlightSortLinks()
            {
                SelectedSort = selectedSort.IsNullOrWhiteSpace() ? Constants.Application.AvailableSortFields.First().Key : selectedSort.ToUpper(),
                AvailableSortFields = Constants.Application.AvailableSortFields.Where(x => !(isAwardSearch && x.Key == "PRICE") &&
                                        !(!isAwardSearch && x.Key == "POINTS")).Select(x => x.Key),
                Links = GetSortLinks(selectedFilters, totalOnDs, currentSlice, basketID, offerID, isAwardSearch)
            };
        }

        private static IEnumerable<SortLink> GetSortLinks(Models.FlightOffer.FlightOfferRequestFilters selectedFilters, int totalOnDs, int currentSlice, string basketID, string offerID, bool isAwardSearch)
        {
            IEnumerable<SortLink> links = Enumerable.Empty<SortLink>();

            if (!basketID.IsNullOrEmpty())
            {
                var baseUrl = GetBaseUrl(totalOnDs, currentSlice, basketID, offerID);
                var filtersPath = GetFilterPath(selectedFilters?.Airlines, selectedFilters?.DepartureTimes, selectedFilters?.Connections, selectedFilters?.Cabins);

                links = Constants.Application.AvailableSortFields
                         .Where(sortType => !((isAwardSearch && sortType.Key == "PRICE") || (!isAwardSearch && sortType.Key == "POINTS")))
                         .Select(sortType => new SortLink
                         {
                             Href = $"{baseUrl}/{sortType.Key}/{filtersPath}",
                             Name = sortType.Key,
                             Rel = sortType.Value
                         });
            }
            return links;
        }

        private static FilterCollection GetFlightSearchFilters(IEnumerable<SliceFlightsAndFares> sliceFlightsAndFares, string selectedSort, Models.FlightOffer.FlightOfferRequestFilters selectedFilters, string baseUrl, bool isAwardSearch)
        {
            return new FilterCollection()
            {
                Filters = new Filter[]
                {
                    GetAirlineFilters(sliceFlightsAndFares, selectedSort, selectedFilters, baseUrl),
                    GetDepartureTimesFilters(sliceFlightsAndFares, selectedSort, selectedFilters, baseUrl),
                    GetConnectionsFilters(sliceFlightsAndFares, selectedSort, selectedFilters, baseUrl),
                    GetCabinFilters(sliceFlightsAndFares, selectedSort, selectedFilters, baseUrl),
                },
                Links = new Link[]
                {
                    new()
                    {
                        // Non-standard Rel. Used as HTML ID/Name by front end.
                        Rel = "clear_filters",
                        Href = $"{baseUrl}/{selectedSort ?? Constants.Application.DEFAULT_SORT}/{GetFilterPath(null, null, null, null)}/"
                    }
                }
            };
        }


        private static Filter GetCabinFilters(IEnumerable<SliceFlightsAndFares> sliceFlightsAndFares, string selectedSort, Models.FlightOffer.FlightOfferRequestFilters selectedFilters, string baseUrl)
        {
            return new Filter
            {
                Name = Constants.Application.FILTER_CABIN,
                Enabled = ConfigReader.Try<bool>("EnableCabinFilters"),
                Values = GetCabinFilterValues(sliceFlightsAndFares, selectedSort, selectedFilters, baseUrl)
            };
        }

        private static IEnumerable<FilterValue> GetCabinFilterValues(IEnumerable<SliceFlightsAndFares> sliceFlightsAndFares, string selectedSort, Models.FlightOffer.FlightOfferRequestFilters selectedFilters, string baseUrl)
        {
            var flightsAfterApplyingOtherFilters = FilterData(sliceFlightsAndFares, new Models.FlightOffer.FlightOfferRequestFilters { Airlines = selectedFilters?.Airlines, DepartureTimes = selectedFilters?.DepartureTimes, Connections = selectedFilters?.Connections });

            var availableFlightsOnly = sliceFlightsAndFares.SelectMany(x => x.Fares.Where(o => o.Availability != Constants.Application.NOT_OFFERED));
            var searchResponseFF = availableFlightsOnly?.Select(f => f.FareFamilyType)?.Distinct();
            var currency = availableFlightsOnly?.FirstOrDefault()?.Price?.Currency;

            var filterValues = fareFamilyOrBetter.Where(x =>
            {
                var flights = FilterCabins(sliceFlightsAndFares, fareFamilyOrBetter[x.Key]);
                return flights.Any() && (searchResponseFF.Contains(x.Key) || searchResponseFF.Contains(FareFamilyandFlex[x.Key]));
            }).Select(x =>
            {
                var flights = FilterCabins(sliceFlightsAndFares, fareFamilyOrBetter[x.Key]);

                return new FilterValue
                {
                    DisplayName = cabinDisplayNames[x.Key],
                    Code = x.Key,
                    Count = flights.Count(),
                    MinPrice = flights.SelectMany(f => f.Fares).Where(fr => (fr.FareFamilyType == x.Key || fr.FareFamilyType == FareFamilyandFlex[x.Key]) && fr.Availability != Constants.Application.NOT_OFFERED).Min(p => p?.Price?.AmountIncludingTax) ?? int.MaxValue,
                    Currency = currency,
                    SelectedSort = selectedSort.IsNullOrEmpty() ? Constants.Application.DEFAULT_SORT : selectedSort,
                    SelectedFilters = selectedFilters,
                    BaseUrl = baseUrl,
                    FilterType = Constants.Application.FILTER_CABIN,
                    Values = new FilterValueDetails() { Cabin = x.Key }
                };
            });
            return (IEnumerable<FilterValue>)filterValues.Where(x => x != null);
        }

        private static IEnumerable<SliceFlightsAndFares> FilterCabins(IEnumerable<SliceFlightsAndFares> flights, string[] cabins) => flights.Where(x => !cabins.Any() || x.Fares.Any(f => cabins.Contains(f.FareFamilyType)));


        private static Filter GetAirlineFilters(IEnumerable<SliceFlightsAndFares> sliceFlightsAndFares, string selectedSort, Models.FlightOffer.FlightOfferRequestFilters selectedFilters, string baseUrl)
        {
            return new Filter
            {
                Name = Constants.Application.FILTER_AIRLINE_NAME,
                Enabled = ConfigReader.Try<bool>("EnableAirlineFilters"),
                Values = GetAirlineFilterValues(sliceFlightsAndFares, selectedSort, selectedFilters, baseUrl)
            };
        }

        private static IEnumerable<FilterValue> GetAirlineFilterValues(IEnumerable<SliceFlightsAndFares> sliceFlightsAndFares, string selectedSort, Models.FlightOffer.FlightOfferRequestFilters selectedFilters, string baseUrl)
        {
            var flightsAfterApplyingOtherFilters = FilterData(sliceFlightsAndFares, new Models.FlightOffer.FlightOfferRequestFilters { DepartureTimes = selectedFilters?.DepartureTimes, Connections = selectedFilters?.Connections, Cabins = selectedFilters?.Cabins });

            var allAirlines = flightsAfterApplyingOtherFilters
                .SelectMany(faf => faf.Flight.Segments)
                .Select(s => s.operatingAirline)
                .DistinctBy(a => a.code)
                .OrderByDescending(a => a.code == Constants.Application.FILTER_AIRLINE_NAME_LIST_FIRST)
                .ThenBy(a => a.name);

            var filters = new List<FilterValue>();
            foreach (var airline in allAirlines)
            {
                var airlineFlights = FilterAirlines(flightsAfterApplyingOtherFilters, new string[] { airline.code });
                filters.Add(new FilterValue
                {
                    DisplayName = airline.name,
                    Code = airline.code,
                    Count = airlineFlights.Count(),
                    MinPrice = airlineFlights.Min(a => a.Fares.Min(f => f.Price?.AmountIncludingTax ?? int.MaxValue)),
                    Currency = airlineFlights.SelectMany(af => af?.Fares?.Where(f => f?.Availability != Constants.Application.NOT_OFFERED))?.FirstOrDefault()?.Price?.Currency,
                    SelectedSort = selectedSort.IsNullOrEmpty() ? Constants.Application.DEFAULT_SORT : selectedSort,
                    SelectedFilters = selectedFilters,
                    BaseUrl = baseUrl,
                    FilterType = Constants.Application.FILTER_AIRLINE_NAME,
                    Values = GetAirlineFilterValueDetails(airline, flightsAfterApplyingOtherFilters)
                });
            }
            return filters;
        }

        private static FilterValueDetails GetAirlineFilterValueDetails(SegmentAirline airline, IEnumerable<SliceFlightsAndFares> sliceFlightsAndFares)
        {
            return new FilterValueDetails
            {
                Airline = airline.code
            };
        }


        private static Filter GetDepartureTimesFilters(IEnumerable<SliceFlightsAndFares> sliceFlightsAndFares, string selectedSort, Models.FlightOffer.FlightOfferRequestFilters selectedFilters, string baseUrl)
        {
            return new Filter
            {
                Name = Constants.Application.FILTER_DEPARTURE_TIME,
                Enabled = ConfigReader.Try<bool>("EnableDepartureTimesFilters"),
                Values = GetDepartureTimeFilterValues(sliceFlightsAndFares, selectedSort, selectedFilters, baseUrl)
            };
        }

        private static IEnumerable<FilterValue> GetDepartureTimeFilterValues(IEnumerable<SliceFlightsAndFares> sliceFlightsAndFares, string selectedSort, Models.FlightOffer.FlightOfferRequestFilters selectedFilters, string baseUrl)
        {
            var flightsAfterApplyingOtherFilters = FilterData(sliceFlightsAndFares, new Models.FlightOffer.FlightOfferRequestFilters { Airlines = selectedFilters?.Airlines, Connections = selectedFilters?.Connections, Cabins = selectedFilters?.Cabins });

            var earlyMorningFlights = EarlyMorningFlights(flightsAfterApplyingOtherFilters);
            var morningFlights = MorningFlights(flightsAfterApplyingOtherFilters);
            var afternoonFlights = AfternoonFlights(flightsAfterApplyingOtherFilters);
            var eveningFlights = EveningFlights(flightsAfterApplyingOtherFilters);
            var nightFlights = NightFlights(flightsAfterApplyingOtherFilters);

            var filters = new List<FilterValue>();
            if (earlyMorningFlights.Any())
            {
                filters.Add(new FilterValue
                {
                    DisplayName = "04:00 - 06:59",
                    Code = Constants.Application.FILTER_EARLY_MORNING,
                    Count = earlyMorningFlights.Count(),
                    MinPrice = earlyMorningFlights.Min(a => a.Fares.Min(f => f.Price?.AmountIncludingTax ?? int.MaxValue)),
                    Currency = earlyMorningFlights.SelectMany(af => af?.Fares?.Where(f => f?.Availability != Constants.Application.NOT_OFFERED))?.FirstOrDefault()?.Price?.Currency,
                    SelectedSort = selectedSort.IsNullOrEmpty() ? Constants.Application.DEFAULT_SORT : selectedSort,
                    SelectedFilters = selectedFilters,
                    BaseUrl = baseUrl,
                    FilterType = Constants.Application.FILTER_DEPARTURE_TIME,
                    Values = new FilterValueDetails
                    {
                        DepartureTime = Constants.Application.FILTER_EARLY_MORNING
                    }
                });
            }
            if (morningFlights.Any())
            {
                filters.Add(new FilterValue
                {
                    DisplayName = "07:00 - 11:59",
                    Code = Constants.Application.FILTER_MORNING,
                    Count = morningFlights.Count(),
                    MinPrice = morningFlights.Min(a => a.Fares.Min(f => f.Price?.AmountIncludingTax ?? int.MaxValue)),
                    Currency = morningFlights.SelectMany(af => af?.Fares?.Where(f => f?.Availability != Constants.Application.NOT_OFFERED))?.FirstOrDefault()?.Price?.Currency,
                    SelectedSort = selectedSort.IsNullOrEmpty() ? Constants.Application.DEFAULT_SORT : selectedSort,
                    SelectedFilters = selectedFilters,
                    BaseUrl = baseUrl,
                    FilterType = Constants.Application.FILTER_DEPARTURE_TIME,
                    Values = new FilterValueDetails
                    {
                        DepartureTime = Constants.Application.FILTER_MORNING
                    }
                });
            }
            if (afternoonFlights.Any())
            {
                filters.Add(new FilterValue
                {
                    DisplayName = "12:00 - 16:59",
                    Code = Constants.Application.FILTER_AFTERNOON,
                    Count = afternoonFlights.Count(),
                    MinPrice = afternoonFlights.Min(a => a.Fares.Min(f => f.Price?.AmountIncludingTax ?? int.MaxValue)),
                    Currency = afternoonFlights.SelectMany(af => af?.Fares?.Where(f => f?.Availability != Constants.Application.NOT_OFFERED))?.FirstOrDefault()?.Price?.Currency,
                    SelectedSort = selectedSort.IsNullOrEmpty() ? Constants.Application.DEFAULT_SORT : selectedSort,
                    SelectedFilters = selectedFilters,
                    BaseUrl = baseUrl,
                    FilterType = Constants.Application.FILTER_DEPARTURE_TIME,
                    Values = new FilterValueDetails
                    {
                        DepartureTime = Constants.Application.FILTER_AFTERNOON
                    }
                });
            }
            if (eveningFlights.Any())
            {
                filters.Add(new FilterValue
                {
                    DisplayName = "17:00 - 19:59",
                    Code = Constants.Application.FILTER_EVENING,
                    Count = eveningFlights.Count(),
                    MinPrice = eveningFlights.Min(a => a.Fares.Min(f => f.Price?.AmountIncludingTax ?? int.MaxValue)),
                    Currency = eveningFlights.SelectMany(af => af?.Fares?.Where(f => f?.Availability != Constants.Application.NOT_OFFERED))?.FirstOrDefault()?.Price?.Currency,
                    SelectedSort = selectedSort.IsNullOrEmpty() ? Constants.Application.DEFAULT_SORT : selectedSort,
                    SelectedFilters = selectedFilters,
                    BaseUrl = baseUrl,
                    FilterType = Constants.Application.FILTER_DEPARTURE_TIME,
                    Values = new FilterValueDetails
                    {
                        DepartureTime = Constants.Application.FILTER_EVENING
                    }
                });
            }
            if (nightFlights.Any())
            {
                filters.Add(new FilterValue
                {
                    DisplayName = "20:00 - 03:59",
                    Code = Constants.Application.FILTER_NIGHT,
                    Count = nightFlights.Count(),
                    MinPrice = nightFlights.Min(a => a.Fares.Min(f => f.Price?.AmountIncludingTax ?? int.MaxValue)),
                    Currency = nightFlights.SelectMany(af => af?.Fares?.Where(f => f?.Availability != Constants.Application.NOT_OFFERED))?.FirstOrDefault()?.Price?.Currency,
                    SelectedSort = selectedSort.IsNullOrEmpty() ? Constants.Application.DEFAULT_SORT : selectedSort,
                    SelectedFilters = selectedFilters,
                    BaseUrl = baseUrl,
                    FilterType = Constants.Application.FILTER_DEPARTURE_TIME,
                    Values = new FilterValueDetails
                    {
                        DepartureTime = Constants.Application.FILTER_NIGHT
                    }
                });
            }
            return filters;
        }

        private static IEnumerable<SliceFlightsAndFares> FilterDepartureTimes(IEnumerable<SliceFlightsAndFares> flights, string[] departureTimesCodes)
        {
            if (departureTimesCodes == null || departureTimesCodes.Length == 0)
            {
                return flights;
            }

            var filteredFlights = new List<SliceFlightsAndFares>();
            if (departureTimesCodes.Contains(Constants.Application.FILTER_EARLY_MORNING))
            {
                filteredFlights.AddRange(EarlyMorningFlights(flights));
            }
            if (departureTimesCodes.Contains(Constants.Application.FILTER_MORNING))
            {
                filteredFlights.AddRange(MorningFlights(flights));
            }
            if (departureTimesCodes.Contains(Constants.Application.FILTER_AFTERNOON))
            {
                filteredFlights.AddRange(AfternoonFlights(flights));
            }
            if (departureTimesCodes.Contains(Constants.Application.FILTER_EVENING))
            {
                filteredFlights.AddRange(EveningFlights(flights));
            }
            if (departureTimesCodes.Contains(Constants.Application.FILTER_NIGHT))
            {
                filteredFlights.AddRange(NightFlights(flights));
            }
            return filteredFlights;
        }

        private static IEnumerable<SliceFlightsAndFares> EarlyMorningFlights(IEnumerable<SliceFlightsAndFares> sliceFlightsAndFares)
            => sliceFlightsAndFares.Where(f => f.Flight.Departure == null || (f.Flight.Departure.Value.Hour >= 4 && f.Flight.Departure.Value.Hour < 7));

        private static IEnumerable<SliceFlightsAndFares> MorningFlights(IEnumerable<SliceFlightsAndFares> sliceFlightsAndFares)
            => sliceFlightsAndFares.Where(f => f.Flight.Departure == null || (f.Flight.Departure.Value.Hour >= 7 && f.Flight.Departure.Value.Hour < 12));

        private static IEnumerable<SliceFlightsAndFares> AfternoonFlights(IEnumerable<SliceFlightsAndFares> sliceFlightsAndFares)
            => sliceFlightsAndFares.Where(f => f.Flight.Departure == null || (f.Flight.Departure.Value.Hour >= 12 && f.Flight.Departure.Value.Hour < 17));

        private static IEnumerable<SliceFlightsAndFares> EveningFlights(IEnumerable<SliceFlightsAndFares> sliceFlightsAndFares)
            => sliceFlightsAndFares.Where(f => f.Flight.Departure == null || (f.Flight.Departure.Value.Hour >= 17 && f.Flight.Departure.Value.Hour < 20));

        private static IEnumerable<SliceFlightsAndFares> NightFlights(IEnumerable<SliceFlightsAndFares> sliceFlightsAndFares)
            => sliceFlightsAndFares.Where(f => f.Flight.Departure == null || f.Flight.Departure.Value.Hour >= 20 || f.Flight.Departure.Value.Hour < 4);


        private static Filter GetConnectionsFilters(IEnumerable<SliceFlightsAndFares> sliceFlightsAndFares, string selectedSort, Models.FlightOffer.FlightOfferRequestFilters selectedFilters, string baseUrl)
        {
            return new Filter
            {
                Name = Constants.Application.FILTER_CONNECTIONS,
                Enabled = ConfigReader.Try<bool>("EnableConnectionsFilters"),
                Values = GetConnectionsFilterValues(sliceFlightsAndFares, selectedSort, selectedFilters, baseUrl)
            };
        }

        private static IEnumerable<SliceFlightsAndFares> FilterConnections(IEnumerable<SliceFlightsAndFares> flights, string[] connectionsCodes)
        {
            // Keep these Where clauses in sync with the function below that calculates counts

            // Direct
            if (connectionsCodes.Contains("0") == false)
            {
                flights = flights.Where(faf => (faf.Flight.Segments.Count() == 1 && faf.Flight.Segments.First().stopCount == 0) == false);
            }
            // Direct with stop
            if (connectionsCodes.Contains("S") == false)
            {
                flights = flights.Where(faf => (faf.Flight.Segments.Count() == 1 && faf.Flight.Segments.First().stopCount > 0) == false);
            }
            // One stop
            if (connectionsCodes.Contains("1") == false)
            {
                flights = flights.Where(faf => (faf.Flight.Segments.Count() == 2) == false);
            }
            // 2+ stops
            if (connectionsCodes.Contains("2") == false)
            {
                flights = flights.Where(faf => (faf.Flight.Segments.Count() > 2) == false);
            }

            return flights;
        }

        private static IEnumerable<FilterValue> GetConnectionsFilterValues(IEnumerable<SliceFlightsAndFares> sliceFlightsAndFares, string selectedSort, Models.FlightOffer.FlightOfferRequestFilters selectedFilters, string baseUrl)
        {
            var flightsAfterApplyingOtherFilters = FilterData(sliceFlightsAndFares, new Models.FlightOffer.FlightOfferRequestFilters { Airlines = selectedFilters?.Airlines, DepartureTimes = selectedFilters?.DepartureTimes, Cabins = selectedFilters?.Cabins });

            // Keep these Where clauses in sync with the function above that filters the data
            var directFlights = flightsAfterApplyingOtherFilters
                .Where(faf => faf.Flight.Segments.Count() == 1 && faf.Flight.Segments.First().stopCount == 0);
            var directWithStopFlights = flightsAfterApplyingOtherFilters
                .Where(faf => faf.Flight.Segments.Count() == 1 && faf.Flight.Segments.First().stopCount > 0);
            var oneStopFlights = flightsAfterApplyingOtherFilters
                .Where(faf => faf.Flight.Segments.Count() == 2);
            var multiStopFlights = flightsAfterApplyingOtherFilters
                .Where(faf => faf.Flight.Segments.Count() > 2);

            var filters = new List<FilterValue>();
            if (directFlights.Any())
            {
                filters.Add(new FilterValue
                {
                    DisplayName = "Direct",
                    Code = "0",
                    Values = new FilterValueDetails
                    {
                        Connections = "0"
                    },
                    Count = directFlights.Count(),
                    MinPrice = directFlights.Min(f => f.Fares.Min(f => f.Price?.AmountIncludingTax ?? int.MaxValue)),
                    Currency = directFlights.SelectMany(af => af?.Fares?.Where(f => f?.Availability != Constants.Application.NOT_OFFERED))?.FirstOrDefault()?.Price?.Currency,
                    SelectedSort = selectedSort.IsNullOrEmpty() ? Constants.Application.DEFAULT_SORT : selectedSort,
                    SelectedFilters = selectedFilters,
                    BaseUrl = baseUrl,
                    FilterType = Constants.Application.FILTER_CONNECTIONS,
                });
            }
            if (directWithStopFlights.Any())
            {
                filters.Add(new FilterValue
                {
                    DisplayName = "Direct with stop",
                    Code = "S",
                    Values = new FilterValueDetails
                    {
                        Connections = "S"
                    },
                    Count = directWithStopFlights.Count(),
                    MinPrice = directWithStopFlights.Min(f => f.Fares.Min(f => f.Price?.AmountIncludingTax ?? int.MaxValue)),
                    Currency = directWithStopFlights.SelectMany(af => af?.Fares?.Where(f => f?.Availability != Constants.Application.NOT_OFFERED))?.FirstOrDefault()?.Price?.Currency,
                    SelectedSort = selectedSort.IsNullOrEmpty() ? Constants.Application.DEFAULT_SORT : selectedSort,
                    SelectedFilters = selectedFilters,
                    BaseUrl = baseUrl,
                    FilterType = Constants.Application.FILTER_CONNECTIONS,
                });
            }
            if (oneStopFlights.Any())
            {
                filters.Add(new FilterValue
                {
                    DisplayName = "1 Stop",
                    Code = "1",
                    Values = new FilterValueDetails
                    {
                        Connections = "1"
                    },
                    Count = oneStopFlights.Count(),
                    MinPrice = oneStopFlights.Min(f => f.Fares.Min(f => f.Price?.AmountIncludingTax ?? int.MaxValue)),
                    Currency = oneStopFlights.SelectMany(af => af?.Fares?.Where(f => f?.Availability != Constants.Application.NOT_OFFERED))?.FirstOrDefault()?.Price?.Currency,
                    SelectedSort = selectedSort.IsNullOrEmpty() ? Constants.Application.DEFAULT_SORT : selectedSort,
                    SelectedFilters = selectedFilters,
                    BaseUrl = baseUrl,
                    FilterType = Constants.Application.FILTER_CONNECTIONS,
                });
            }
            if (multiStopFlights.Any())
            {
                filters.Add(new FilterValue
                {
                    DisplayName = "2+ Stops",
                    Code = "2",
                    Values = new FilterValueDetails
                    {
                        Connections = "2"
                    },
                    Count = multiStopFlights.Count(),
                    MinPrice = multiStopFlights.Min(f => f.Fares.Min(f => f.Price?.AmountIncludingTax ?? int.MaxValue)),
                    Currency = multiStopFlights.SelectMany(af => af?.Fares?.Where(f => f?.Availability != Constants.Application.NOT_OFFERED))?.FirstOrDefault()?.Price?.Currency,
                    SelectedSort = selectedSort.IsNullOrEmpty() ? Constants.Application.DEFAULT_SORT : selectedSort,
                    SelectedFilters = selectedFilters,
                    BaseUrl = baseUrl,
                    FilterType = Constants.Application.FILTER_CONNECTIONS,
                });
            }

            return filters;
        }


        private static IEnumerable<Link> GetSelectandNextSelfLink(string basketID, int currentSlice, int totalOnDs, string offerID)
        {
            if (basketID != null)
            {
                return new Link[]
                {
                    new Link()
                    {
                        Href = $"{Helpers.PostbackUrl.Get()}/search/{basketID}/slice{totalOnDs}.{currentSlice}/{offerID}",
                        Rel = "self"
                    }
                };

            }
            return Enumerable.Empty<Link>();
        }

        private static TripSummary GetTripSummary(Models.FlightOffer.FlightOfferResponse flightOfferResponse, int currentSlice, IEnumerable<CabinInfo> cabinInfo)
        {
            if (flightOfferResponse != null)
            {
                var outboundSelectedOffer = flightOfferResponse.Offers.FirstOrDefault(o => o.Selected);

                return new()
                {
                    Currency = flightOfferResponse.Offers.FirstOrDefault(offer => offer.OfferId != null)?.SalePrice.FirstOrDefault().Amount.BaseCurrency,
                    SliceDetails = currentSlice == 0 || outboundSelectedOffer == null ? Enumerable.Empty<SliceDetails>() : GetTripSummarySliceDetails(outboundSelectedOffer, currentSlice, cabinInfo),
                    TotalAwardPoints = outboundSelectedOffer?.SalePrice?.FirstOrDefault(p => p.PriceComponentType.Equals(Models.FlightOffer.Price.PriceComponentTypes.PointsEnum))?.PointsAmount?.PointsCount.ToString()
                };
            }

            return new();
        }

        private static IEnumerable<SliceDetails> GetTripSummarySliceDetails(Models.FlightOffer.OfferDetails outboundSelectedOffer, int currentSlice, IEnumerable<CabinInfo> cabinInfo)
        {
            var dominantCabin = cabinInfo.FirstOrDefault(x => x.BrandID == outboundSelectedOffer.OfferItems.FirstOrDefault().Services.FirstOrDefault().FareDetails.DominantFareProduct);
            var awardPoints = outboundSelectedOffer.SalePrice.FirstOrDefault(p => p.PriceComponentType.Equals(Models.FlightOffer.Price.PriceComponentTypes.PointsEnum))?.PointsAmount?.PointsCount.ToString();

            return new SliceDetails[]
            {
                new SliceDetails()
                {
                    selectedCabin = awardPoints.IsNullOrEmpty()?   dominantCabin?.FareFamily : dominantCabin?.AwardFareFamily,
                    selectedPrice = Convert.ToDecimal(outboundSelectedOffer.SalePrice.FirstOrDefault(p=>p.PriceComponentType.Equals(Models.FlightOffer.Price.PriceComponentTypes.BasePriceEnum))?.Amount.MonetaryAmount),
                    sliceNumber = currentSlice-1,
                    selectedAwardPoints = awardPoints
                }
            };
        }

        private static Models.BFFResponse.Calendar? GetCalendar(CalendarSearchResponse? response, string requestDate)
        {
            if (response?.Calendar != null)
            {
                Models.BFFResponse.Calendar calendar = new();
                calendar.FromPrices = response.Calendar.FromPrices.Select(cfp => (Models.BFFResponse.CalendarFromPrice)cfp);

                if (calendar.FromPrices.Any())
                {
                    DateOnly requestedDate = DateOnly.Parse(requestDate);
                    calendar.From = calendar?.FromPrices.First().FromDate > requestedDate.AddDays(-3) ? requestedDate.AddDays(-3) : calendar?.FromPrices?.First().FromDate;
                    calendar.To = calendar?.FromPrices.Last().FromDate < requestedDate.AddDays(3) ? requestedDate.AddDays(3) : calendar?.FromPrices?.Last().FromDate;
                }

                return calendar;
            }
            return null;
        }

        private static IEnumerable<Link> GetSelfLink(string basketID, int totalOnDs, int currentSlice, string offerID)
        {
            if (basketID != null)
            {
                if (currentSlice == 0)
                {
                    return new Link[]
                    {
                    new Link()
                        {
                            Href = $"{Helpers.PostbackUrl.Get()}/search/slice{totalOnDs}.{currentSlice}/{basketID}",
                            Rel = "self"
                        }
                    };
                }
                else
                {
                    return new Link[]
                    {
                    new Link()
                        {
                            Href = $"{Helpers.PostbackUrl.Get()}/search/{basketID}/slice{totalOnDs}.{currentSlice}/{offerID}",
                            Rel = "self"
                        }
                    };
                }
            }
            return Enumerable.Empty<Link>();
        }

        private static IEnumerable<SliceFlightsAndFares> GetFlightsAndFares(FlightOfferResponse response, int totalOnDs, int currentSlice, string basketID, string passengerCounts, string[] selectedFareFamilies, IEnumerable<CabinInfo> cabinInfo, IEnumerable<AircraftInfo> aircraftInfo, IEnumerable<AirlineInfo> airlineInfo, IEnumerable<AirportInfo> airportInfo)
        {
            return response.DataList.Journey.Select(x =>
            {
                var offerDetails = response.Offers.Where(o => o.OfferItems.Any(oi => oi.Services.First().JourneyRefIds.Contains(x.JourneyId)));
                var segments = response.DataList.FlightSegments.Where(fs => x.FlightSegmentRefIds.Contains(fs.SegmentId));
                var offerId = offerDetails.Any(o => !o.OfferId.IsNullOrEmpty()) ? offerDetails.Where(x => !x.OfferId.IsNullOrEmpty()).FirstOrDefault().OfferId : string.Empty;

                return new SliceFlightsAndFares()
                {
                    Flight = GetFlight(segments, aircraftInfo, airlineInfo, airportInfo, x.Duration, offerId, passengerCounts),
                    Fares = GetFares(offerDetails, totalOnDs, currentSlice, basketID, passengerCounts, selectedFareFamilies, cabinInfo, response.IsAwardSearchResult)
                };
            })
            .Where(x => x.Fares.Any());
        }

        private static string GetDuration(string flightDuration)
        {
            var duration = flightDuration.Split(':');

            var hours = duration[0] == "00" ? string.Empty : $"{duration[0].TrimStart('0')}H";
            var minutes = duration[1] == "00" ? string.Empty : $"{duration[1].TrimStart('0')}M";

            return $"PT{hours}{minutes}";
        }

        private static SliceFlight GetFlight(IEnumerable<Models.FlightOffer.Segment> segments, IEnumerable<AircraftInfo> aircraftInfo, IEnumerable<AirlineInfo> airlineInfo, IEnumerable<AirportInfo> airportInfo, string flightDuration, string offerId, string passengerCounts)
        {
            return new()
            {
                Duration = GetDuration(flightDuration),
                Segments = GetFlightSegments(segments, aircraftInfo, airlineInfo, airportInfo),
                Links = GetCarbonFootprintLinks(segments, offerId, passengerCounts)
            };
        }

        private static IEnumerable<Link> GetCarbonFootprintLinks(IEnumerable<Models.FlightOffer.Segment> segments, string offerId, string passengerCounts)
        {
            if (!offerId.IsNullOrEmpty() && segments.Any(x => x.OperatingCarrier == "VS"))
            {
                return new Link[]
                {
                    new Link()
                    {
                        Href = $"{Helpers.PostbackUrl.Get()}/carbonindex/{offerId}/{passengerCounts}",
                        Rel = "carbonIndex"
                    }
                };

            }
            return Enumerable.Empty<Link>();
        }

        private static IEnumerable<SliceFare> GetFares(IEnumerable<Models.FlightOffer.OfferDetails> offers, int totalOnDs, int currentSlice, string basketID, string passengerCounts, string[] selectedFareFamilies, IEnumerable<CabinInfo> cabinInfo, bool isAwardSearchResults)
        {
            return offers.Where(x => !x.Selected).Select(offer => GetSliceFare(offer, totalOnDs, currentSlice, basketID, passengerCounts, selectedFareFamilies, cabinInfo, isAwardSearchResults));
        }

        private static SliceFare GetSliceFare(OfferDetails offer, int totalOnDs, int currentSlice, string basketID, string passengerCounts, string[] selectedFareFamilies, IEnumerable<CabinInfo> cabinInfo, bool isAwardSearch)
        {
            var fareDetails = offer.OfferItems.First().Services.First().FareDetails!;
            var dominantCabin = cabinInfo.FirstOrDefault(x => x.BrandID == fareDetails.DominantFareProduct);
            var fareFamily = isAwardSearch ? dominantCabin?.AwardFareFamily : dominantCabin?.FareFamily;
            return new SliceFare()
            {
                Availability = offer.Availability,
                AvailableSeatCount = offer.AvailableSeatCount,
                Price = offer.OfferId.IsNullOrEmpty() ? null : (PriceWithTax?)(offer.SalePrice, isAwardSearch),
                PromoCodeApplied = offer.PromoCodeApplied,
                IsSaverFare = offer.IsSaverFare,
                FareFamilyType = fareFamily ?? fareDetails?.DominantFareProduct,
                CabinSelected = selectedFareFamilies.Contains(dominantCabin?.FareFamily),
                FareSegments = fareDetails?.FareSegments?.Select(x => GetSliceFareSegment(x, cabinInfo)),
                FareId = fareDetails?.FareBasisCode,
                Id = offer.OfferId,
                Links = GetLinks(totalOnDs, currentSlice, basketID, offer.OfferId, fareDetails?.FareBasisCode, passengerCounts)
            };
        }

        private static SliceFareSegment GetSliceFareSegment(Models.FlightOffer.FareSegment fareSegment, IEnumerable<CabinInfo> cabinInfo)
        {
            return new SliceFareSegment()
            {
                bookingClass = fareSegment.BookingClass,
                cabinName = cabinInfo.FirstOrDefault(seg => seg.BrandID == fareSegment.CabinName)?.BrandName ?? fareSegment.CabinName,
                isDominantLeg = fareSegment.IsDominantLeg,
                isSaverFare = fareSegment.IsSaverFare
            };
        }

        private static IEnumerable<Link> GetLinks(int totalOnDs, int currentSlice, string basketID, string offerID, string fareBasisCode, string passengerCounts)
        {
            var urlPrefix = $"{Helpers.PostbackUrl.Get()}";

            Link summaryFareLink;
            Link searchSummaryFareLink = null;

            if (offerID != null)
            {
                if (totalOnDs - currentSlice == 1)
                {
                    summaryFareLink = new()
                    {
                        Href = $"{urlPrefix}/{basketID}/summary/slice{totalOnDs}.{currentSlice + 1}/{offerID}",
                        Rel = "summary"
                    };
                    searchSummaryFareLink = new()
                    {
                        Href = $"{urlPrefix}/summary/{basketID}/slice{totalOnDs}.{currentSlice + 1}|{offerID}",
                        Rel = "searchsummary"
                    };
                }
                else
                {
                    summaryFareLink = new()
                    {
                        Href = $"{urlPrefix}/search/{basketID}/slice{totalOnDs}.{currentSlice + 1}/{offerID}",
                        Rel = "fare"
                    };
                }

                Link cabinLink = new()
                {
                    Href = $"{urlPrefix}/{offerID}/cabin/{fareBasisCode}",
                    Rel = "cabin"
                };

                Link dynamicCabinLink = new()
                {
                    Href = $"{urlPrefix}/{passengerCounts}/{offerID}/dynamicCabin",
                    Rel = "dynamicCabin"
                };

                return searchSummaryFareLink == null
                    ? new Link[] { summaryFareLink, cabinLink, dynamicCabinLink }
                    : new Link[] { summaryFareLink, searchSummaryFareLink, cabinLink, dynamicCabinLink };
            }

            return Enumerable.Empty<Link>();
        }

        private static IEnumerable<Models.BFFResponse.Segment> GetFlightSegments(IEnumerable<Models.FlightOffer.Segment> fs, IEnumerable<AircraftInfo> aircraftInfo, IEnumerable<AirlineInfo> airlineInfo, IEnumerable<AirportInfo> airportInfo)
        {
            return fs.Select(x => new Models.BFFResponse.Segment()
            {
                metal = [new FlightSearchMetal(x.AirCraft, aircraftInfo)],
                airline = (SegmentAirline)(x.MarketingCarrier, airlineInfo),
                flightNumber = $"{x.MarketingCarrier}{x.MarketingFlightNumber}",
                operatingAirline = (SegmentAirline)(x.OperatingCarrier, airlineInfo),
                operatingFlightNumber = $"{x.OperatingCarrier}{x.OperatingFlightNumber}",
                origin = (Airport)(x.OriginAirportCode, airportInfo),
                destination = (Airport)(x.DestinationAirportCode, airportInfo),
                departure = Common.Helper.DateTimeHelper.ParseLocalDateTime(x.ScheduledDepartureTime.LocalDateTime),
                arrival = Common.Helper.DateTimeHelper.ParseLocalDateTime(x.ScheduledArrivalTime.LocalDateTime),
                duration = GetDuration(x.FlightDuration),
                legs = GetLegs(x.Legs, airportInfo, airlineInfo),
                stopCount = x.ConnectionOrStopOver.Parse<int>(),
                connection = x.ConnectionDuration.IsNullOrEmpty() ? null : GetDuration(x.ConnectionDuration),
                pendingGovtApproval = x.PendingGovtApproval
            });
        }

        private static IEnumerable<Leg> GetLegs(IEnumerable<Legs> legs, IEnumerable<AirportInfo> airportInfo, IEnumerable<AirlineInfo> airlineInfo)
        {
            return legs.Select(leg => new Leg()
            {
                origin = (Airport)(leg.OriginAirportCode, airportInfo),
                destination = (Airport)(leg.DestinationAirportCode, airportInfo),
                operatingAirline = string.IsNullOrWhiteSpace(leg.OperatingCarrier) ? null : (SegmentAirline)(leg.OperatingCarrier, airlineInfo),
                marketingAirline = string.IsNullOrWhiteSpace(leg.MarketingCarrier) ? null : (SegmentAirline)(leg.MarketingCarrier, airlineInfo),
                departure = Common.Helper.DateTimeHelper.ParseLocalDateTime(leg.ScheduledDepartureTime.LocalDateTime),
                arrival = Common.Helper.DateTimeHelper.ParseLocalDateTime(leg.ScheduledArrivalTime.LocalDateTime),
                duration = GetDuration(leg.LegDuration),
                isDominantLeg = leg.IsDominantLeg.Value,
                stopOver = string.IsNullOrEmpty(leg.StopOverDuration) ? null : GetDuration(leg.StopOverDuration)
            });
        }
    }
}