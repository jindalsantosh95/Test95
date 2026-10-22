using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita
{
    public class OfferRepriceOfferItemPricingMapper
    {
        public static List<OfferitemPricing> GetOfferItemPricing(resultTripDetailsSolutionPricing pricing)
        {
            List<OfferitemPricing> offerItemPricings = new();

            var salePrices = new List<Price>
            {
                new()
                {
                    Amount = new()
                    {
                        BaseCurrency = pricing.salePrice?.currency,
                        MonetaryAmount = pricing.salePrice?.amount.ToString()
                    },
                    PriceComponentName = string.Empty,
                    PriceComponentType = Price.PriceComponentTypes.BasePriceEnum
                },
                new()
                {
                    Amount = new()
                    {
                        BaseCurrency = pricing.saleFareTotal?.currency,
                        MonetaryAmount = pricing.saleFareTotal?.amount.ToString()
                    },

                    PriceComponentName = string.Empty,
                    PriceComponentType = Price.PriceComponentTypes.BaseFareEnum
                },
                new()
                {
                    Amount = new()
                    {
                        BaseCurrency = pricing.saleTaxTotal?.currency,
                        MonetaryAmount = pricing.saleTaxTotal?.amount.ToString()
                    },
                    PriceComponentName = string.Empty,
                    PriceComponentType = Price.PriceComponentTypes.TotalTaxEnum
                }
            };

            foreach (var fare in pricing.fareCalculation)
            {
                foreach (var tax in fare.boxedTax)
                {
                    if (salePrices.All(q => q.PriceComponentName != tax.taxCode))
                    {
                        salePrices.Add(new Price
                        {
                            Amount = new()
                            {
                                BaseCurrency = tax.price.currency,
                                MonetaryAmount = Convert.ToString(tax.price.amount)
                            },
                            PriceComponentName = tax.taxCode,
                            PriceComponentType = Price.PriceComponentTypes.TaxEnum
                        });
                    }
                }
            }

            List<Price> saleDifferences = null;

            if (pricing.saleFareDifference?.amount != null)
            {
                saleDifferences = new List<Price>
                {
                    new()
                    {
                        Amount = new()
                        {
                            BaseCurrency = pricing.saleFareDifference?.currency,
                            MonetaryAmount = pricing.saleFareDifference?.amount.ToString()
                        },
                        PriceComponentName = string.Empty,
                        PriceComponentType = Price.PriceComponentTypes.BaseFareEnum
                    }
                };
            }

            List<PenaltyPrice> penaltyPrices = new();
            foreach (var penalty in pricing.penaltyPrice)
            {
                if (penalty?.amount != null)
                {
                    penaltyPrices.Add(new PenaltyPrice()
                    {
                        Amount = new CurrencyAmount()
                        {
                            BaseCurrency = penalty.currency,
                            MonetaryAmount = penalty.amount.ToString(),
                        }
                    });
                }
            }

            AddCollectPrice addCollectPrice = null;

            if (pricing.addCollectPrice?.amount != null)
            {
                addCollectPrice = new AddCollectPrice
                {
                    Amount = new()
                    {
                        BaseCurrency = pricing.addCollectPrice?.currency,
                        MonetaryAmount = pricing.addCollectPrice?.amount.ToString()
                    }
                };
            }

            offerItemPricings.Add(new OfferitemPricing
            {
                FareCalculationText = pricing.fareCalculation.First().line.First(),
                AddCollectPrice = addCollectPrice,
                PenaltyPrices = penaltyPrices.Count > 0 ? penaltyPrices : null,
                SalePrice = salePrices,
                SalePriceDifference = saleDifferences
            });

            return offerItemPricings;
        }
    }
}
