using VAA.EAPI.GraphQL.Domain.Book.Constants;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.Common;
using VAA.EAPI.GraphQL.Domain.Book.Models.Request.Loyalty;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Loyalty;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;

namespace VAA.EAPI.GraphQL.Domain.Book.Mappers
{
    public class PointsToCurrencyMapper
    {
        public static IEnumerable<PointsCurrencyConverterRequest> PointsToCurrencyRQMapper(OfferResponse offerResponse, Basket basket)
        {
            var selectedOffer = offerResponse.Offers.First();

            var ptcTypes = offerResponse.DataList.CustomerDetails.Select(p => new { Ptc = p.Ptc, CustId = p.CustId });

            Dictionary<string, CurrencyAmount> ptcWiseAmount = new Dictionary<string, CurrencyAmount>();

            foreach (var ptc in ptcTypes)
            {
                if (!ptcWiseAmount.ContainsKey(ptc.Ptc))
                {
                    var offerAmount = selectedOffer.OfferItems.FirstOrDefault(o => o.Services.First().ServiceBeneficiaries.Contains(ptc.CustId)).OfferitemPricing.First().SalePrice.First(sale => sale.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum).Amount;

                    ptcWiseAmount.Add(ptc.Ptc, new() { BaseCurrency = offerAmount.BaseCurrency, MonetaryAmount = offerAmount.MonetaryAmount });
                }
            }

            return basket.People.GroupBy(x => x.Type).Select(p => new PointsCurrencyConverterRequest()
            {
                CountryCode = offerResponse.Pos.CountryCode,
                LoyaltyPoints = basket.CustomerProfile.MembershipDetails.FQTPoints,
                NumberOfPassengers = p.Count(),
                PassengerTypeCode = p.Key switch
                {
                    "ADULT" => "ADT",
                    "CHILD" => "CNN",
                    "TEEN" => "GBE",
                    "INFANT" => "INF",
                    _ => string.Empty
                },
                Tier = basket.CustomerProfile.MembershipDetails.FQTTierCode switch
                {
                    "Red" => FQTTier.FF.ToString(),
                    "Silver" => FQTTier.FO.ToString(),
                    "Gold" => FQTTier.GM.ToString(),
                    _ => string.Empty
                },
                Amount = ptcWiseAmount.FirstOrDefault(a => a.Key == p.Key switch
                {
                    "ADULT" => "ADT",
                    "CHILD" => "CNN",
                    "TEEN" => "GBE",
                    "INFANT" => "INF",
                    _ => string.Empty
                }).Value
            });
        }

        public static IEnumerable<PointsEquivalentCurrency> PointsToCurrencyRSMapper(IEnumerable<PointsCurrencyConverterResponse> response, Basket basket)
        {
            var totalCurrencyList = response.First().PtcWisePointsEquivalentCurrencyDetails.Select(x => x.TotalPointsEquivalentCurrency);

            return totalCurrencyList.OrderByDescending(x => x.LoyaltyPoints).Select(x => new PointsEquivalentCurrency()
            {
                Amount = Convert.ToDouble(x.Amount.MonetaryAmount),
                CurrencyCode = x.Amount.BaseCurrency,
                LoyaltyPoints = x.LoyaltyPoints.TrimStart('0')
            });
        }
    }
}