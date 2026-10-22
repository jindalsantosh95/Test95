using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Common
{
    public class PriceMapping
    {
        public static AveragePassengerPrice GetAveragePassengerPrice(resultBundledSolutionSummarySolutionExt ext)
        {
            return new AveragePassengerPrice
            {
                Amount = new()
                {
                    MonetaryAmount = Convert.ToString(ext?.averagePassengerPrice?.amount),
                    BaseCurrency = ext?.averagePassengerPrice?.currency
                }
            };
        }

        public static HighestPassengerPrice GetHighestPassengerPrice(resultBundledSolutionSummarySolutionExt ext)
        {
            return new HighestPassengerPrice
            {
                Amount = new()
                {
                    MonetaryAmount = Convert.ToString(ext?.highestPassengerPrice?.amount),
                    BaseCurrency = ext?.highestPassengerPrice?.currency
                }
            };
        }

        public static List<Price> GetSalesPrice(resultBundledSolutionSummarySolution solution)
        {
            List<Price> salePrices = new() {
                new Price
                {
                Amount = new()
                {
                    BaseCurrency = solution.saleTotal.currency,
                    MonetaryAmount = solution.saleTotal.amount.ToString()
                },
                PriceComponentName = string.Empty,
                PriceComponentType = Price.PriceComponentTypes.BasePriceEnum
                },
            new Price
            {
                Amount = new()
                {
                    BaseCurrency = solution.displayFareTotal.currency,
                    MonetaryAmount = solution.displayFareTotal.amount.ToString()
                },
                PriceComponentName = string.Empty,
                PriceComponentType = Price.PriceComponentTypes.BaseFareEnum
            },
            new Price
            {
                Amount = new()
                {
                    BaseCurrency = solution.saleTaxTotal.currency,
                    MonetaryAmount = solution.saleTaxTotal.amount.ToString()
                },
                PriceComponentName = string.Empty,
                PriceComponentType = Price.PriceComponentTypes.TotalTaxEnum
            }
            };

            return salePrices;
        }
        public static List<string> GetServiceBeneficiaries(List<Customer> customerDetail, string Ptc)
        {
            List<string> serviceBeneficiaries = new();
            var customerDetails = customerDetail.Where(c => (c.ItaPtc ?? c.Ptc).Equals(Ptc));

            foreach (var c in customerDetails)
            {
                serviceBeneficiaries.Add(c.CustId);
            }

            return serviceBeneficiaries;
        }

        public static async Task<List<string>> AddpriceClassDetails(resultBundledSolutionSummarySolutionPricing pricing, resultBundledSolutionSummarySolutionItinerarySlice slice, List<PriceClass> reshopPriceClass, ICachedPriceClass cachedPriceClass, ICachedCabin cachedCabin)
        {
            List<string> priceClassRefIds = new();
            string pcRefId = string.Empty;
            foreach (var segment in slice.segment)
            {
                foreach (var bookingInfo in pricing.bookingInfo)
                {
                    if (segment.origin == bookingInfo.segment.origin && segment.destination == bookingInfo.segment.destination && segment.flight.number == bookingInfo.segment.flight.number && segment.departure.date == bookingInfo.segment.departure.date)
                    {
                        PriceClassTableEntity priceClassData = await cachedPriceClass.GetCachedPriceClassData(bookingInfo.fare.record1FareType);

                        if (!reshopPriceClass.Any(s => s.FareBasisCode == bookingInfo.fare.extendedFareCode && s.FareTypeCode == bookingInfo.fare.record1FareType))
                        {
                            int lastPcId = reshopPriceClass.Any() ? Convert.ToInt32(reshopPriceClass.Last().Id[2..]) : 0;
                            pcRefId = $"{Constants.Application.PriceClass}{++lastPcId}";
                            reshopPriceClass.Add(new PriceClass
                            {
                                Id = pcRefId,
                                FareBasisCode = bookingInfo.fare.extendedFareCode,
                                FareTypeCode = priceClassData.RowKey,
                                ProductName = priceClassData.ProductName,
                            });
                        }
                        else
                        {
                            pcRefId = reshopPriceClass.FirstOrDefault(s => s.FareBasisCode == bookingInfo.fare.extendedFareCode).Id;
                        }
                        if (!priceClassRefIds.Any(x => x == reshopPriceClass.FirstOrDefault(s => s.FareBasisCode == bookingInfo.fare.extendedFareCode && s.FareTypeCode == bookingInfo.fare.record1FareType)?.Id))
                            priceClassRefIds.Add(pcRefId);
                    }
                }
            }
            return priceClassRefIds.Count == 0 ? null : priceClassRefIds;
        }
        

        public static RefundPrice GetRefundPrice(resultBundledSolutionSummarySolution soln)
        {
            var refundPrice = soln.pricing.FirstOrDefault()?.refundPrice;
            if (refundPrice!= null)
            {
                return new RefundPrice
                {
                    Amount = new CurrencyAmount
                    {
                        MonetaryAmount = refundPrice.First().amount.ToString(),
                        BaseCurrency = refundPrice.First().currency
                    }
                };
            }
            return null;
        }
    }
}
