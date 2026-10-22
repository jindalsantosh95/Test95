using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlexiDateSearch;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita
{
    public class FlexiDateItaResMapper
    {
        public static FlexiDateSearchResponse FlexiDateSearchResMapper(result result, OfferRequest offerRequest)
        {
            var searchResponse = new FlexiDateSearchResponse
            {
                ShoppingResponseId = $"{result.session}/{result.solutionSet}",
            };
            if (result.Items is { Length: > 0 })
            {
                searchResponse.FlexibleDateCalendar = GetFlexibleDateCalendar(result, offerRequest);
            }

            return searchResponse;
        }

        private static FlexibleDateCalendar GetFlexibleDateCalendar(result result, OfferRequest offerRequest)
        {
            var resultFlexiDates = result.Items.First(x => x.GetType() == typeof(resultFlexibleDates)) as resultFlexibleDates;
            var flexiDateCalendar = new FlexibleDateCalendar
            {
                ColumnText = GetFlexiColumns(resultFlexiDates.column),
                DepartureDetails = GetDepartureDetails(resultFlexiDates, offerRequest),
            };

            return flexiDateCalendar;
        }

        private static List<string> GetFlexiColumns(IEnumerable<resultFlexibleDatesColumn> flexibleDatesColumn)
        {
            var flexiColumnsList = new List<string>();
            foreach (var column in flexibleDatesColumn)
            {
                var flexiColumn = column.returnDate;
                flexiColumnsList.Add(flexiColumn);
            }

            return flexiColumnsList;
        }

        private static List<DepartureDetails> GetDepartureDetails(resultFlexibleDates resultFlexiDates, OfferRequest offerRequest)
        {
            var departureDetailsList = new List<DepartureDetails>();
            foreach (var row in (resultFlexiDates.row ?? Array.Empty<resultFlexibleDatesRow>()).Where(x => x.cell != null))
            {
                var departureDetails = new DepartureDetails()
                {
                    DepartureDate = Convert.ToDateTime(row.departureDate).Date,
                    FlexibleDatePriceCalendarCell = GetFlexiDatePrices(row.cell, offerRequest),
                };
                departureDetailsList.Add(departureDetails);
            }

            return departureDetailsList;
        }

        private static List<FlexibleDatePriceCalendar> GetFlexiDatePrices(IEnumerable<resultFlexibleDatesRowCell> rowCell, OfferRequest offerRequest)
        {
            var flexiCellList = new List<FlexibleDatePriceCalendar>();
            foreach (var cells in rowCell)
            {
                if (!cells.solution.IsNullOrDefault())
                {
                    var flexiCell = new FlexibleDatePriceCalendar();
                    flexiCell.OriginDestinations = GetOriginDestinations(offerRequest, cells.solution.itinerary.slice.ToList());
                    flexiCell.SalePrice = GetTotalSalePrices(cells.solution);
                    flexiCell.PriceItems = GetPriceItems(cells.solution);
                    flexiCell.Availability = true;
                    flexiCell.LowestPriceOfferIndicator = cells.minPriceInGrid == true;
                    flexiCellList.Add(flexiCell);
                }
            }

            return flexiCellList;
        }

        private static List<Domain.FlexiDateSearch.OriginDestination> GetOriginDestinations(OfferRequest offerRequest, List<resultFlexibleDatesRowCellSolutionItinerarySlice> itinerarySlices)
        {
            var originDestinationList = new List<Domain.FlexiDateSearch.OriginDestination>();
            int slicecount = 0;
            foreach (var originDest in offerRequest.FlightsearchRequest.SearchOriginDestinations)
            {
                var originDestination = new Domain.FlexiDateSearch.OriginDestination()
                {
                    OriginAirport = originDest.Origin,
                    DestinationAirport = originDest.Destination,
                    DepartureDate = itinerarySlices[slicecount].departure.date
                };
                slicecount++;
                originDestinationList.Add(originDestination);
            }

            return originDestinationList;
        }

        private static List<PriceItems> GetPriceItems(resultFlexibleDatesRowCellSolution solution)
        {
            var priceItemsList = new List<PriceItems>();
            string passenger = "";
            foreach (var price in solution.pricing)
            {
                foreach (var ptc in price.passenger)
                {
                    passenger = ptc.ptc.First();
                }
                var priceitems = new PriceItems()
                {
                    Ptc = passenger,
                    SalePrice = GetSalePrices(price)
                };
                priceItemsList.Add(priceitems);
            }

            return priceItemsList;
        }

        private static List<Price> GetSalePrices(resultFlexibleDatesRowCellSolutionPricing price)
        {
            List<Price> salePriceList = new()
            {
                new Price()
                {
                    Amount = new CurrencyAmount()
                    {
                        MonetaryAmount = price.salePrice.amount.ToString(),
                        BaseCurrency = price.salePrice.currency
                    },
                    PriceComponentType = Price.PriceComponentTypes.BasePriceEnum
                },
                new Price()
                {
                    Amount = new CurrencyAmount()
                    {
                        MonetaryAmount = price.displayFareTotal.amount.ToString(),
                        BaseCurrency = price.displayFareTotal.currency
                    },
                    PriceComponentType = Price.PriceComponentTypes.BaseFareEnum
                }
            };

            return salePriceList;
        }

        private static List<Price> GetTotalSalePrices(resultFlexibleDatesRowCellSolution solution)
        {
            List<Price> salePriceList = new()
            {
                new Price()
                {
                    Amount = new CurrencyAmount()
                    {
                        MonetaryAmount = solution.saleTotal.amount.ToString(),
                        BaseCurrency = solution.saleTotal.currency
                    },
                    PriceComponentType = Price.PriceComponentTypes.BasePriceEnum
                },
                new Price
                {
                    Amount = new CurrencyAmount
                    {
                        BaseCurrency = solution.saleTaxTotal.currency,
                        MonetaryAmount = solution.saleTaxTotal.amount.ToString()
                    },
                    PriceComponentType = Price.PriceComponentTypes.TotalTaxEnum
                }
            };

            return salePriceList;
        }
    }
}
