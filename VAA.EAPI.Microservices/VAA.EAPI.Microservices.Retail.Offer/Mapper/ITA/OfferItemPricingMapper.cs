using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Infrastructure.PSS.Proxy.PassengerV4Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita
{
    public class OfferItemPricingMapper
    {
        public static List<OfferitemPricing> GetOfferitemPricings(resultBundledSolutionSummarySolutionPricing pricing)
        {
            List<OfferitemPricing> offerItemPricings = new();
            var salePrices = new List<Price>
            {
                    new()
                    {
                        Amount= new()
                        {
                            BaseCurrency = pricing.salePrice?.currency,
                            MonetaryAmount= pricing.salePrice?.amount.ToString()
                        },
                         PriceComponentName = string.Empty,
                        PriceComponentType = Price.PriceComponentTypes.BasePriceEnum
                    },
                    new()
                    {
                        Amount=new()
                        {
                            BaseCurrency=pricing.saleFareTotal?.currency,
                            MonetaryAmount=pricing.saleFareTotal?.amount.ToString()
                        },

                         PriceComponentName = string.Empty,
                        PriceComponentType = Price.PriceComponentTypes.BaseFareEnum
                    },
                    new()
                    {
                        Amount=new()
                        {
                            BaseCurrency=pricing.saleTaxTotal?.currency,
                            MonetaryAmount=pricing.saleTaxTotal?.amount.ToString()
                        },
                        PriceComponentName = string.Empty,
                        PriceComponentType = Price.PriceComponentTypes.TotalTaxEnum,
                    }
                 };

            List<Price> saleDifferences = null;
            if (pricing.saleFareDifference?.amount != null)
            {
                saleDifferences = new List<Price>();
                saleDifferences.Add(
                new Price
                {
                    Amount = new()
                    {
                        BaseCurrency = pricing.saleFareDifference?.currency,
                        MonetaryAmount = pricing.saleFareDifference?.amount.ToString()
                    },
                    PriceComponentName = string.Empty,
                    PriceComponentType = Price.PriceComponentTypes.BaseFareEnum
                });
            }
            if (pricing.saleTaxTotalDifference?.amount != null)
            {
                saleDifferences.Add(new Price
                {
                    Amount = new()
                    {
                        BaseCurrency = pricing.saleTaxTotalDifference?.currency,
                        MonetaryAmount = pricing.saleTaxTotalDifference?.amount.ToString()
                    },
                    PriceComponentName = string.Empty,
                    PriceComponentType = Price.PriceComponentTypes.TotalTaxEnum
                });
            }

            List<PenaltyPrice> penaltyPrices = new();
            if (pricing.penaltyPrice != null)
            {
                foreach (var penalty in pricing.penaltyPrice)
                {
                    if (penalty?.amount != null)
                    {
                        penaltyPrices.Add(new PenaltyPrice
                        {
                            Amount = new CurrencyAmount
                            {
                                BaseCurrency = penalty?.currency,
                                MonetaryAmount = penalty?.amount.ToString(),
                            }
                        });
                    }
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
                FareCalculationText = pricing.fareCalculation?.First().line.First(),
                AddCollectPrice = addCollectPrice,
                PenaltyPrices = penaltyPrices.Count > 0 ? penaltyPrices : null,
                SalePrice = salePrices,
                SalePriceDifference = saleDifferences
            });
            return offerItemPricings;
        }

        private static IEnumerable<Tax> GetTaxBreakdown(resultBundledSolutionSummarySolutionPricingFareCalculation[] fareCalculation)
        {
            return fareCalculation?.FirstOrDefault()?.boxedTax?.Select(tax =>
                new Tax
                {
                    Amount = new()
                    {
                        BaseCurrency = tax.price?.currency,
                        MonetaryAmount = tax.price?.amount.ToString() ?? string.Empty
                    },
                    Code = tax.taxCode
                }
            );
        }

    }
}
