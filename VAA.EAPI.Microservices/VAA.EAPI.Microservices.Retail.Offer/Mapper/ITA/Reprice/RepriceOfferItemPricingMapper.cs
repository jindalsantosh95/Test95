using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Reprice
{
    public class RepriceOfferItemPricingMapper
    {
        public static List<OfferitemPricing> GetOfferItemPricing(resultTripDetailsSolutionPricing pricing, OfferResponse reshopResponse, string ptc)
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
                    BaseCurrencyAmount = new()
                    {
                        Currency = pricing.cocFareTotal?.currency,
                        Amount = pricing.cocFareTotal?.amount.ToString()
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
                    PriceComponentType = Price.PriceComponentTypes.TotalTaxEnum,
                    TaxBreakup = GetSalePriceTaxBreakUp(pricing)
                }
            };

            List<Price> saleDifferences = null;
            if (pricing.saleFareDifference?.amount != null)
            {
                saleDifferences ??= new();
                saleDifferences.Add(new Price
                {
                    Amount = new CurrencyAmount
                    {
                        BaseCurrency = pricing.saleFareDifference?.currency,
                        MonetaryAmount = pricing.saleFareDifference?.amount.ToString("F2")
                    },
                    PriceComponentName = string.Empty,
                    PriceComponentType = Price.PriceComponentTypes.BaseFareEnum
                });
                saleDifferences.Add(new Price
                {
                    Amount = new CurrencyAmount
                    {
                        BaseCurrency = pricing.saleFareDifference?.currency,
                        MonetaryAmount = Convert.ToString(pricing?.ext?.reshopTaxTotal?.Sum(i => i.totalSalePriceDifference?.amount))
                    },
                    PriceComponentName = string.Empty,
                    PriceComponentType = Price.PriceComponentTypes.TotalTaxEnum
                });
            }
            List<PenaltyPrice> penaltyPrices = null;
            foreach (var penalty in pricing.penaltyPrice ?? Array.Empty<resultTripDetailsSolutionPricingPenaltyPrice>())
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

            List<RefundPrice> refundPrices = null;
            foreach (var refund in pricing.refundPrice ?? Array.Empty<resultTripDetailsSolutionPricingRefundPrice>())
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
                PenaltyPrices = penaltyPrices,
                RefundPrice = refundPrices,
                SalePrice = salePrices,
                SalePriceDifference = saleDifferences,
                OriginalSalePrice = GetOriginalSalePrice(reshopResponse, ptc)
            });

            return offerItemPricings;
        }

        private static List<Tax> GetSalePriceTaxBreakUp(resultTripDetailsSolutionPricing pricing)
        {
            List<Tax> taxes = new();

            foreach (var tax in pricing.ext.reshopTaxTotal)
            {
                if (!taxes.Any(x => x.Code == tax.code) && tax.totalSalePrice.amount != 0)
                {
                    taxes.Add(new Tax
                    {
                        Amount = new()
                        {
                            BaseCurrency = tax.totalSalePrice.currency,
                            MonetaryAmount = Convert.ToString(tax.totalSalePrice.amount)
                        },
                        Code = tax.code,
                        CountryCode = pricing.tax.FirstOrDefault(x => x.code == tax.code).country,
                        Name = pricing.tax.FirstOrDefault(x => x.code == tax.code).name,
                        CollectionPoints = GetCollectionPoints(tax)
                    });
                }
            }
            return taxes;
        }
        private static List<CollectionPoint> GetCollectionPoints(resultTripDetailsSolutionPricingExtReshopTaxTotal reshopTaxTotal)
        {
            var collectionPoints = new List<CollectionPoint>();

            if (reshopTaxTotal.code == Constants.Application.XfTaxCode || reshopTaxTotal.code == Constants.Application.ZpTaxCode)
            {
                if (reshopTaxTotal.tax is { Length: > 0 })
                {
                    foreach (var item in reshopTaxTotal.tax)
                    {
                        collectionPoints.Add(new CollectionPoint
                        {
                            Amount = new CurrencyAmount
                            {
                                BaseCurrency = item.price.currency,
                                MonetaryAmount = Convert.ToString(item.price.amount)
                            },
                            LocationCode = item.origin
                        });
                    }
                }
                return collectionPoints;
            }

            return null;
        }

        private static List<Price> GetOriginalSalePrice(OfferResponse reshopResponse, string ptc)
        {
            foreach (var offerItem in reshopResponse.Offers.FirstOrDefault().OfferItems)
            {
                if (offerItem.Services.Any(x => x.ServiceBeneficiaries.Contains(reshopResponse.DataList.CustomerDetails.FirstOrDefault(x => (x.ItaPtc ?? x.Ptc)== ptc).CustId)))
                {
                    return offerItem.OfferitemPricing?.FirstOrDefault()?.OriginalSalePrice;
                }
            }
            return null;
        }
    }
}