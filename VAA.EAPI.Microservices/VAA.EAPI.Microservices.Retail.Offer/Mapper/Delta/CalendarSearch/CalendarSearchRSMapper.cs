using CSharpFunctionalExtensions;
using System;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Offer.Domain.CalendarSearch;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Delta.CalendarSearch
{
    public class CalendarSearchRSMapper
    {
        public static CalendarSearchResponse GetCalendar(bool isAwardSearch, OffersSetList response)
        {
            var fromPrices = (from offerset in response.OffersSets
                              from offer in offerset.Offers.Where(x => x.AdditionalOfferProperties.SoldOut != true && x.OfferPricing.First().TotalAmt.CurrencyEquivalentPrice != null)
                              select new CalendarFromPrice()
                              {
                                  FromDate = DateOnly.ParseExact(offer.OfferItems.First().RetailItems.First().RetailItemMetaData.FareInformation.First().PriceCalendar.PriceCalendarDate, "yyyy-M-dd"),
                                  Price = GetCalendarFromPrice(offer, isAwardSearch)
                              }).ToList();
            return new CalendarSearchResponse
            {
                Calendar = new Calendar
                {
                    FromPrices = fromPrices,
                    From = fromPrices.Min(fp => fp.FromDate),
                    To = fromPrices.Max(fp => fp.FromDate)
                }
            };
        }

        private static CalendarPrice GetCalendarFromPrice(Infrastructure.Proxy.DeltaOffer.Offer offer, bool isAwardSearch)
        {
            var totalAmt = offer.OfferPricing.First().TotalAmt;
            return new CalendarPrice()
            {
                Amount = totalAmt?.CurrencyEquivalentPrice?.CurrencyAmt,
                AwardPoints = isAwardSearch ? totalAmt?.MilesEquivalentPrice.MileCnt : null,
                Currency = totalAmt?.CurrencyEquivalentPrice?.CurrencyCode,
                MinimumPriceInWeek = offer.AdditionalOfferProperties.LowestFareInWeek ?? false,
                MinimumPriceInMonth = offer.AdditionalOfferProperties.LowestFareInCalendar ?? false,
                Direct = offer.AdditionalOfferProperties.TotalTripStopCnt == 0
            };
        }
    }
}