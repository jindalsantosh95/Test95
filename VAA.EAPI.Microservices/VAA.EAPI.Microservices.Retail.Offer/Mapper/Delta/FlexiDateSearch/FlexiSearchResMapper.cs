using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlexiDateSearch;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Delta.FlexiDateSearch;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Delta.FlexiDateSearch
{
    public class FlexiSearchResMapper
    {
        public static FlexiDateSearchResponse FlexiDateSearchResMapper(OffersSetList offerResponse, FlexiDateSearchQuery requestData)
        {
            return new FlexiDateSearchResponse()
            {
                ShoppingResponseId = offerResponse.OfferResponseId,
                FlexibleDateCalendar = new FlexibleDateCalendar()
                {
                    ColumnText = offerResponse.OfferDataList.FlexReturnDates.ToList(),
                    DepartureDetails = GetDepartureDetails(offerResponse, requestData)
                },
            };
        }

        private static List<DepartureDetails> GetDepartureDetails(OffersSetList offerResponse, FlexiDateSearchQuery requestData)
        {
            var departureDetailsList = new List<DepartureDetails>();
            foreach (var offerSet in offerResponse.OffersSets)
            {
                var departureDetails = new DepartureDetails()
                {
                    DepartureDate = Convert.ToDateTime(offerSet.ItineraryDepartureDate).Date,
                    FlexibleDatePriceCalendarCell = GetFlexibleDatePrices(offerSet.Offers, requestData)
                };
                departureDetailsList.Add(departureDetails);
            }
            return departureDetailsList;
        }

        private static List<FlexibleDatePriceCalendar> GetFlexibleDatePrices(IEnumerable<Infrastructure.Proxy.DeltaOffer.Offer> offers, FlexiDateSearchQuery requestData)
        {
            var flexiDatePriceList = new List<FlexibleDatePriceCalendar>();
            foreach (var offer in offers)
            {
                if (offer.AdditionalOfferProperties.SoldOut != true)
                {
                    var flexiDatePrice = new FlexibleDatePriceCalendar()
                    {
                        Availability = true,
                        OriginDestinations = GetOriginDestinations(requestData, offer.AdditionalOfferProperties),
                        SalePrice = GetSalesPrices(offer.OfferPricing.First(), requestData.AwardSearch),
                        LowestPriceOfferIndicator = offer.AdditionalOfferProperties.LowestFare ?? false,
                        OfferId = offer.OfferId
                    };
                    flexiDatePriceList.Add(flexiDatePrice);
                }
                else
                {
                    var flexiDatePrice = new FlexibleDatePriceCalendar()
                    {
                        Availability = false,
                        OfferId = offer.OfferId
                    };
                    flexiDatePriceList.Add(flexiDatePrice);
                }

            }
            return flexiDatePriceList;
        }

        private static List<OriginDestination> GetOriginDestinations(FlexiDateSearchQuery requestData, AdditionalOfferProperties addOfferProperties)
        {
            var originDestList = new List<OriginDestination>();
            int segmentCount = 0;
            requestData.Request.OffersCriteria.FlightRequestCriteria.SearchOriginDestination.ToList().ForEach(orgDest =>
            {
                var originDest = new OriginDestination()
                {
                    OriginAirport = orgDest.Origins.First().AirportCode,
                    DestinationAirport = orgDest.Destinations.First().AirportCode,
                    DepartureDate = segmentCount == 0 ? addOfferProperties.FlexDepartDate : addOfferProperties.FlexReturnDate,
                };
                originDestList.Add(originDest);
                segmentCount++;
            });
            return originDestList;
        }

        private static List<EAPI.Domain.Virgin.Models.Common.Price> GetSalesPrices(Pricing offerPricing, bool isAwardSearch)
        {
            List<EAPI.Domain.Virgin.Models.Common.Price> salePrices = new()
            {
                new EAPI.Domain.Virgin.Models.Common.Price()
                {
                    PriceComponentType = EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum,
                    Amount = new EAPI.Domain.Virgin.Models.Common.CurrencyAmount()
                    {
                        BaseCurrency = offerPricing.TotalAmt.CurrencyEquivalentPrice.CurrencyCode,
                        MonetaryAmount = offerPricing.TotalAmt.CurrencyEquivalentPrice.FormattedCurrencyAmt
                    }
                },
                new EAPI.Domain.Virgin.Models.Common.Price()
                {
                    PriceComponentType = EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.TotalTaxEnum,
                    Amount = new EAPI.Domain.Virgin.Models.Common.CurrencyAmount()
                    {
                        BaseCurrency = offerPricing.TaxSummary.TotalTaxAmt.CurrencyEquivalentPrice.CurrencyCode,
                        MonetaryAmount = offerPricing.TaxSummary.TotalTaxAmt.CurrencyEquivalentPrice.FormattedCurrencyAmt
                    }
                }
            };

            if (isAwardSearch)
            {
                salePrices.Add(new EAPI.Domain.Virgin.Models.Common.Price()
                {
                    PriceComponentType = EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.PointsEnum,
                    PointsAmount = new()
                    {
                        PointsCount = offerPricing.TotalAmt.MilesEquivalentPrice.MileCnt
                    }
                });
            }
            return salePrices;
        }
    }
}
