using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlightOffer;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using PriceDetail = VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop.PriceDetail;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Reshop
{
    public class ReshopOfferItemPricingMapper
    {
        public static List<OfferitemPricing> GetOfferitemPricings(resultBundledSolutionSummarySolutionPricing pricing, ReshopRequest reshopRequest, string ptc)
        {
            List<OfferitemPricing> offerItemPricings = new();
            List<Price> saleDifferences = null;
            List<PenaltyPrice> penaltyPrices = null;
            List<RefundPrice> refundPrices = null;
            AddCollectPrice addCollectPrice = null;

            var salePrices = new List<Price>
            {
                    new()
                    {
                        Amount= new CurrencyAmount
                        {
                            BaseCurrency = pricing.salePrice?.currency,
                            MonetaryAmount= pricing.salePrice?.amount.ToString()
                        },
                         PriceComponentName = string.Empty,
                        PriceComponentType = Price.PriceComponentTypes.BasePriceEnum
                    },
                    new()
                    {
                        Amount=new CurrencyAmount
                        {
                            BaseCurrency=pricing.saleFareTotal?.currency,
                            MonetaryAmount=pricing.saleFareTotal?.amount.ToString()
                        },

                         PriceComponentName = string.Empty,
                        PriceComponentType = Price.PriceComponentTypes.BaseFareEnum
                    },
                    new()
                    {
                        Amount=new CurrencyAmount
                        {
                            BaseCurrency=pricing.saleTaxTotal?.currency,
                            MonetaryAmount=pricing.saleTaxTotal?.amount.ToString()
                        },
                         PriceComponentName = string.Empty,
                        PriceComponentType = Price.PriceComponentTypes.TotalTaxEnum,
                        TaxBreakup = GetTaxBreakdown(pricing.fareCalculation)

                    }
                 };

            if (pricing.saleFareDifference?.amount != null)
            {
                saleDifferences ??= new();
                saleDifferences.Add(
                new Price
                {
                    Amount = new CurrencyAmount
                    {
                        BaseCurrency = pricing.saleFareDifference?.currency,
                        MonetaryAmount = pricing.saleFareDifference?.amount.ToString("F2")
                    },
                    PriceComponentName = string.Empty,
                    PriceComponentType = Price.PriceComponentTypes.BaseFareEnum
                });
            }
            if (pricing.saleTaxTotalDifference?.amount != null)
            {
                saleDifferences ??= new();
                saleDifferences.Add(new Price
                {
                    Amount = new CurrencyAmount
                    {
                        BaseCurrency = pricing.saleTaxTotalDifference?.currency,
                        MonetaryAmount = pricing.saleTaxTotalDifference?.amount.ToString()
                    },
                    PriceComponentName = string.Empty,
                    PriceComponentType = Price.PriceComponentTypes.TotalTaxEnum
                });
            }
            foreach (var penalty in pricing.penaltyPrice ?? Array.Empty<resultBundledSolutionSummarySolutionPricingPenaltyPrice>())
            {
                if (penalty?.amount != null)
                {
                    penaltyPrices ??= new();
                    penaltyPrices.Add(new PenaltyPrice
                    {
                        Amount = new()
                        {
                            BaseCurrency = penalty?.currency,
                            MonetaryAmount = penalty?.amount.ToString(),
                        }
                    });
                }
            }
            foreach (var refund in pricing.refundPrice ?? Array.Empty<resultBundledSolutionSummarySolutionPricingRefundPrice>())
            {
                if (refund?.amount != null)
                {
                    refundPrices ??= new();
                    refundPrices.Add(new RefundPrice
                    {
                        Amount = new()
                        {
                            BaseCurrency = refund?.currency,
                            MonetaryAmount = refund?.amount.ToString(),
                        }
                    });
                }
            }
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
                AddCollectPrice = addCollectPrice,
                PenaltyPrices = penaltyPrices,
                RefundPrice = refundPrices,
                SalePrice = salePrices,
                SalePriceDifference = saleDifferences,
                OriginalSalePrice = GetOriginalSalePrice(reshopRequest, ptc)
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

        private static List<Price> GetOriginalSalePrice(ReshopRequest reshopRequest, string ptc)
        {
            var priceDetails = new List<PriceDetail>();

            foreach (var order in reshopRequest.ReshopOrder.Where(ro => ro.OrderProductType == "FLIGHT"))
            {
               if (order.Services.Any(x => x.ServiceBeneficiaries.Contains(reshopRequest.CustomerDetails.FirstOrDefault(x => (x.ItaPtc ?? x.Ptc).Is(ptc))?.CustId)))
               {
                    priceDetails = order.priceDetails;
               }
            }

            if (priceDetails.Count > 0)
            {
                var prices = new List<Price>();

                var baseFareAmount = priceDetails.First(x => x.PriceComponentType == PriceDetail.PriceComponentTypes.BaseFare);

                var taxBreakUp = priceDetails.Where(x => x.PriceComponentType == PriceDetail.PriceComponentTypes.TotalTax && !string.IsNullOrEmpty(x.PriceComponentName)).Select(price => new Tax
                {
                    Amount = new CurrencyAmount
                    {
                        BaseCurrency = price.Amount.BaseCurrency,
                        MonetaryAmount = price.Amount.MonetaryAmount
                    },
                    Code = price.PriceComponentName,
                });

                prices.Add(new Price
                {
                    PriceComponentType = Price.PriceComponentTypes.BaseFareEnum,
                    Amount = new CurrencyAmount
                    {
                        BaseCurrency = baseFareAmount.BaseCurrencyAmount.Currency == null ? baseFareAmount.Amount.BaseCurrency : baseFareAmount.BaseCurrencyAmount.Currency,
                        MonetaryAmount = baseFareAmount.BaseCurrencyAmount.Amount == null ? baseFareAmount.Amount.MonetaryAmount : baseFareAmount.BaseCurrencyAmount.Amount
                    }
                });

                prices.Add(new Price
                {
                    PriceComponentType = Price.PriceComponentTypes.BasePriceEnum,
                    Amount = new CurrencyAmount
                    {
                        BaseCurrency = priceDetails.First(x => x.PriceComponentType == PriceDetail.PriceComponentTypes.BasePrice).Amount.BaseCurrency,
                        MonetaryAmount = priceDetails.First(x => x.PriceComponentType == PriceDetail.PriceComponentTypes.BasePrice).Amount.MonetaryAmount
                    }
                });


                prices.Add(new Price
                {
                    PriceComponentName = string.Empty,
                    PriceComponentType = Price.PriceComponentTypes.TotalTaxEnum,
                    TaxBreakup = taxBreakUp,
                    Amount = new CurrencyAmount
                    {
                        BaseCurrency = priceDetails.First(x => x.PriceComponentType == PriceDetail.PriceComponentTypes.TotalTax).Amount.BaseCurrency,
                        MonetaryAmount = priceDetails.First(x => x.PriceComponentType == PriceDetail.PriceComponentTypes.TotalTax).Amount.MonetaryAmount
                    },

                });

                return prices;
            }

            return null;
        }

    }
}
