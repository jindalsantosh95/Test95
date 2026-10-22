using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.ReshopForCancellation;
using VAA.EAPI.Microservices.Retail.Offer.Domain.OrderCancel;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using static VAA.EAPI.Domain.Virgin.Models.Common.Price;
using OfferitemPricing = VAA.EAPI.Microservices.Retail.Offer.Domain.OrderCancel.OfferitemPricing;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.ReshopForCancellation
{
    public class ReshopForCancellationResponseMapper
    {
        public static CancelReshopResponse RfcReshopResponse(ReshopForCancellationQuery reshopRequest, string offerExpirationTimeLimit)
        {
            return new CancelReshopResponse
            {
                DataList = new()
                {
                    CustomerDetails = reshopRequest.CustomerDetails,
                },
                Offers =
                [
                    new()
                    {
                        OfferItems = ReshopOfferItemMapper(reshopRequest),
                        OfferExpirationTimeLimit = offerExpirationTimeLimit
                    }
                ],
                ShoppingResponseId = Guid.NewGuid().ToString(),
                Parties = reshopRequest.Parties
            };
        }

        private static IEnumerable<ReshopOfferItem> ReshopOfferItemMapper(ReshopForCancellationQuery reshopRequest)
        {
            return reshopRequest.ReshopOrder.Select(x => new ReshopOfferItem()
            {
                OfferitemPricing = GetOfferItemPricings(x.priceDetails),
                Services = [
                    new()
                    {
                        ServiceBeneficiaries = x.Services.SelectMany(y => y.ServiceBeneficiaries).Distinct()
                    }
                ]
            });
        }

        private static List<OfferitemPricing> GetOfferItemPricings(IEnumerable<PriceDetail> priceDetails)
        {
            List<OfferitemPricing> offerItemPricings = new();

            OfferitemPricing offeritem = new()
            {
                OriginalSalePrice = new List<Price>
                {
                    new()
                    {
                        PriceComponentType = Price.PriceComponentTypes.BaseFareEnum,
                        Amount= new CurrencyAmount
                        {
                            BaseCurrency = priceDetails.First().Amount.BaseCurrency,
                            MonetaryAmount= priceDetails.First(x => x.PriceComponentType == PriceDetail.PriceComponentTypes.BaseFare).Amount.MonetaryAmount.ToString()
                        },
                    },
                    new()
                    {
                        PriceComponentType = Price.PriceComponentTypes.BasePriceEnum,
                        Amount=new CurrencyAmount
                        {
                           BaseCurrency = priceDetails.First().Amount.BaseCurrency,
                           MonetaryAmount = priceDetails.First(x => x.PriceComponentType == PriceDetail.PriceComponentTypes.BasePrice).Amount.MonetaryAmount.ToString()
                        },
                    },
                    new()
                    {
                        PriceComponentType = Price.PriceComponentTypes.TotalTaxEnum,
                        Amount=new CurrencyAmount
                        {
                            BaseCurrency = priceDetails.First().Amount.BaseCurrency,
                            MonetaryAmount= priceDetails.First(x => x.PriceComponentType == PriceDetail.PriceComponentTypes.TotalTax).Amount.MonetaryAmount.ToString()
                        },
                        TaxBreakup = TaxMapper(priceDetails.Where(x => x.PriceComponentType == PriceDetail.PriceComponentTypes.Tax), null)

                    }
                },
                SalePrice = new List<Price>
                {
                    new()
                    {
                        PriceComponentName = string.Empty,
                        PriceComponentType = Price.PriceComponentTypes.BaseFareEnum,
                        Amount= new CurrencyAmount
                        {
                            MonetaryAmount = Constants.Application.RfcMonetaryAmount,
                            BaseCurrency= priceDetails.First().Amount.BaseCurrency
                        },
                    },
                    new()
                    {
                        PriceComponentName = string.Empty,
                        PriceComponentType = Price.PriceComponentTypes.BasePriceEnum,
                        Amount=new CurrencyAmount
                        {
                            BaseCurrency= priceDetails.First().Amount.BaseCurrency,
                            MonetaryAmount= Constants.Application.RfcMonetaryAmount
                        },
                    },
                    new()
                    {
                        PriceComponentName = string.Empty,
                        PriceComponentType = Price.PriceComponentTypes.TotalTaxEnum,
                        Amount=new CurrencyAmount
                        {
                            MonetaryAmount= Constants.Application.RfcMonetaryAmount,
                            BaseCurrency= priceDetails.First().Amount.BaseCurrency
                        },
                        TaxBreakup = null

                    }
                }
            };
            offeritem.SalePriceDifference = new List<Price>
            {
                new()
                {
                    PriceComponentType = Price.PriceComponentTypes.BaseFareEnum,
                    Amount = new CurrencyAmount
                    {
                        BaseCurrency = priceDetails.First().Amount.BaseCurrency,
                        MonetaryAmount=  Convert.ToString(Convert.ToDecimal(offeritem.SalePrice.First(x => x.PriceComponentType == PriceComponentTypes.BaseFareEnum).Amount.MonetaryAmount) - Convert.ToDecimal(offeritem.OriginalSalePrice.First(x => x.PriceComponentType == PriceComponentTypes.BaseFareEnum).Amount.MonetaryAmount)),
                    },
                },
                new()
                {
                    PriceComponentType = Price.PriceComponentTypes.BasePriceEnum,
                    Amount=new CurrencyAmount
                    {
                        BaseCurrency = priceDetails.First().Amount.BaseCurrency,
                        MonetaryAmount=  Convert.ToString(Convert.ToDecimal(offeritem.SalePrice.First(x => x.PriceComponentType == PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount) - Convert.ToDecimal(offeritem.OriginalSalePrice.First(x => x.PriceComponentType == PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount)),
                    },
                },
                new()
                {
                    PriceComponentType = PriceComponentTypes.TotalTaxEnum,
                    Amount=new CurrencyAmount
                    {
                        BaseCurrency = priceDetails.First().Amount.BaseCurrency,
                        MonetaryAmount= Convert.ToString(Convert.ToDecimal(offeritem.SalePrice.First(x => x.PriceComponentType == PriceComponentTypes.TotalTaxEnum).Amount.MonetaryAmount) - Convert.ToDecimal(offeritem.OriginalSalePrice.First(x => x.PriceComponentType == PriceComponentTypes.TotalTaxEnum).Amount.MonetaryAmount)),
                    },
                    TaxBreakup = SaleDifferenceTaxMapper(offeritem.OriginalSalePrice.First(x => x.PriceComponentType == PriceComponentTypes.TotalTaxEnum).TaxBreakup),
                }

            };

            offeritem.RefundPrice = new List<RefundPrice>
            {
                new()
                {
                    Amount = new CurrencyAmount
                    {
                        BaseCurrency = priceDetails.First().Amount.BaseCurrency,
                        MonetaryAmount = Convert.ToString(Convert.ToDecimal(offeritem.SalePriceDifference.FirstOrDefault(x => x.PriceComponentType == PriceComponentTypes.BasePriceEnum)?.Amount.MonetaryAmount)),
                    }
                }
            };
            
            offerItemPricings.Add(offeritem);

            return offerItemPricings;
        }

        private static IEnumerable<Tax> SaleDifferenceTaxMapper(IEnumerable<Tax> salePriceTaxes)
        {
            return salePriceTaxes.Select(tax => new Tax
            {
                    Code = tax.Code,
                    Name = tax.Name,
                    Amount = new CurrencyAmount
                    {
                    MonetaryAmount = Convert.ToString(Convert.ToDecimal(Constants.Application.RfcMonetaryAmount) - Convert.ToDecimal(tax.Amount.MonetaryAmount)),
                        BaseCurrency = tax.Amount.BaseCurrency
                    },
                    Refundable = true
            });
            }

        private static IEnumerable<Tax> TaxMapper(IEnumerable<PriceDetail> priceDetail, resultRefundRefundSolutionPricing pricing)
        {
            if (priceDetail == null)
            {
                return null;
            }

            return priceDetail.Select(taxDetails => new Tax
            {
                    Amount = new CurrencyAmount
                    {
                        BaseCurrency = taxDetails.Amount.BaseCurrency,
                        MonetaryAmount = taxDetails.Amount.MonetaryAmount
                    },
                Code = taxDetails.PriceComponentName,
                Refundable = (pricing?.tax?.FirstOrDefault(t => taxDetails.PriceComponentName == t.code))?.nettingResult?.FirstOrDefault()?.status != "REFUND" ? false : true,
            });

        }

        public static CancelReshopResponse NonRfcReshopResponse(ReshopForCancellationQuery request, result repriceResponse, string expiration)
        {
            List<Penalty> penalties = [];
            PenaltyRef penaltyRefIds = new();
            string offerDocId = Guid.NewGuid().ToString();

            return new CancelReshopResponse
            {
                ShoppingResponseId = offerDocId,
                OrderId = request.OrderId,
                Pos = request.Pos,
                Parties = request.Parties,
                DataList = new()
                {
                    CustomerDetails = request.CustomerDetails,
                    Penalty = penalties
                },
                Offers =
                [
                    new()
                    {
                        OfferItems = GetReshopOffers(repriceResponse , request , penalties, offerDocId),
                        OfferExpirationTimeLimit = expiration
                    }
                ],
            };
        }

        private static IEnumerable<ReshopOfferItem> GetReshopOffers(result repriceResponse, ReshopForCancellationQuery request, List<Penalty> penalties, string offerDocId)
        {
            List<ReshopOfferItem> reshopOfferItems = [];

            var refundSolution = repriceResponse?.Items?.FirstOrDefault(x => x.GetType() == typeof(resultRefund)) as resultRefund;

            if (refundSolution != null && refundSolution.refundSolution.Length != 0)
            {
                IEnumerable<RefundPrice> refundPrices = null;
                List<PenaltyPrice> penaltyPrices = null;
                PenaltyRef penaltyRefIds = new();
                int offerItemId = 1;

                foreach (var order in request.ReshopOrder)
                {
                    var priceDetails = order.priceDetails;

                    var pricing = new resultRefundRefundSolutionPricing();

                    var ptcs = (from customer in request.CustomerDetails
                                join beneficiary in order.Services.First().ServiceBeneficiaries on customer.CustId equals beneficiary
                                select customer.Ptc).First();

                    if (ptcs == "ADT")
                    {
                        pricing = refundSolution.refundSolution.First().pricing.First();
                    }
                    else
                    {
                        pricing = (refundSolution?.refundSolution?.FirstOrDefault()?.pricing?.FirstOrDefault(p => p.passenger.Any(passenger => passenger.ptc != null && passenger.ptc.Contains(ptcs))));
                    }

                    penaltyRefIds.CancellationPenaltyRefIds ??= new();

                    var penaltiesToAdd = (pricing.penaltyPrice ?? Array.Empty<resultRefundRefundSolutionPricingPenaltyPrice>()).Where(penalty => penalty?.amount != null && penalty?.amount != 0)
                   .Where(penalty => !penalties.Any(p => p.Type == Constants.Application.ChangePenalty && p.MaxPrice.Amount.MonetaryAmount == penalty.amount.ToString()))
                   .Select(penalty =>
                   {
                       int lastPenaltyId = penalties.Any() ? Convert.ToInt32(penalties.Last().Id.Split("-")[0].Substring(3)) : 0;
                       string penaltyRefId = $"{Constants.Application.Penalty}{++lastPenaltyId}-{Constants.Application.CancellationPenalty}";

                       penalties.Add(new Penalty
                       {
                           Id = penaltyRefId,
                           Type = Constants.Application.CancellationPenalty,
                           MaxPrice = new MaxPrice
                           {
                               Amount = new CurrencyAmount
                               {
                                   MonetaryAmount = penalty.amount.ToString(),
                                   BaseCurrency = penalty.currency
                               }
                           },
                           MinPrice = new MinPrice
                           {
                               Amount = new CurrencyAmount
                               {
                                   MonetaryAmount = penalty.amount.ToString(),
                                   BaseCurrency = penalty.currency
                               }
                           }
                       });

                       return penaltyRefId;
                   });

                    penaltyRefIds.CancellationPenaltyRefIds?.AddRange(penaltiesToAdd);

                    refundPrices = (pricing.refundPrice?.Where(refund => refund?.amount != null)
                   .Select(refund => new RefundPrice
                   {
                       Amount = new CurrencyAmount
                       {
                           BaseCurrency = refund?.currency,
                           MonetaryAmount = refund?.amount.ToString(),
                       }
                   }));


                    reshopOfferItems.Add(new ReshopOfferItem
                    {
                        OfferItemId = $"{offerDocId}-{offerItemId++}",

                        OfferitemPricing = [
                            new OfferitemPricing() {

                                 RefundPrice = refundPrices != null ? refundPrices : null,
                                 PenaltyPrices = penalties != null ? AddReshopCancelPenaltyList(penalties) : null,

                                OriginalSalePrice = new List<Price>
                                {
                                    new()
                                    {
                                        PriceComponentType = Price.PriceComponentTypes.BaseFareEnum,
                                        Amount = new CurrencyAmount
                                        {
                                            BaseCurrency = priceDetails.First().Amount.BaseCurrency,
                                            MonetaryAmount = priceDetails.First(x => x.PriceComponentType == PriceDetail.PriceComponentTypes.BaseFare).Amount.MonetaryAmount.ToString()
                                        },
                                    },
                                    new()
                                    {
                                        PriceComponentType = Price.PriceComponentTypes.BasePriceEnum,
                                        Amount = new CurrencyAmount
                                        {
                                            BaseCurrency = priceDetails.First().Amount.BaseCurrency,
                                            MonetaryAmount = priceDetails.First(x => x.PriceComponentType == PriceDetail.PriceComponentTypes.BasePrice).Amount.MonetaryAmount.ToString()
                                        },
                                    },
                                    new()
                                    {
                                        PriceComponentType = Price.PriceComponentTypes.TotalTaxEnum,
                                        Amount=new CurrencyAmount
                                        {
                                            BaseCurrency = priceDetails.First().Amount.BaseCurrency,
                                            MonetaryAmount =priceDetails.First(x => x.PriceComponentType == PriceDetail.PriceComponentTypes.TotalTax).Amount.MonetaryAmount.ToString()
                                        },
                                        TaxBreakup = TaxMapper(order.priceDetails.Where(x => x.PriceComponentType == PriceDetail.PriceComponentTypes.Tax), pricing)

                                    }
                                },
                                SalePriceDifference = GetSalePriceDifference(pricing),
                                SalePrice = GetSalePrice(pricing,order)
                            }],

                        Services = [
                            new()
                            {
                                ServiceBeneficiaries = order.Services.First().ServiceBeneficiaries,
                                PenaltyRef = new PenaltyRef{
                                   CancellationPenaltyRefIds = penalties.LastOrDefault() != null ? new List<string> { penalties.LastOrDefault()?.Id } : new List<string>()
                                }
                            }
                            ]
                    });
                }
            }

            return reshopOfferItems;
        }

        private static IEnumerable<PenaltyPrice> AddReshopCancelPenaltyList(List<Penalty> penalties)
        {
            return penalties.Select(price => new PenaltyPrice
            {
                ID = price.Id,
                Amount = price.MaxPrice.Amount
            });
        }

        private static List<Price> GetSalePrice(resultRefundRefundSolutionPricing pricing, ReshopOrder order)
        {
            var basePrice = order.priceDetails.First(x => x.PriceComponentType == PriceDetail.PriceComponentTypes.BaseFare).Amount.MonetaryAmount.Parse<decimal>() + pricing.saleFareDifference.amount;
            var totalTax = order.priceDetails.First(x => x.PriceComponentType == PriceDetail.PriceComponentTypes.TotalTax).Amount.MonetaryAmount.Parse<decimal>() + pricing.saleTaxTotalDifference.amount;

            var salepriceLst = new List<Price>
            {
               new()
               {
                   Amount= new CurrencyAmount
                   {
                       BaseCurrency = order.priceDetails.First().Amount.BaseCurrency,
                       MonetaryAmount = basePrice.ToString(),
                   },
                   PriceComponentType = PriceComponentTypes.BaseFareEnum,
                },
               new()
               {
                   Amount= new CurrencyAmount
                   {
                        BaseCurrency = order.priceDetails.First().Amount.BaseCurrency,
                       MonetaryAmount = totalTax.ToString(),
                   },
                   PriceComponentType=Price.PriceComponentTypes.TotalTaxEnum,
                   TaxBreakup = order.priceDetails.Where(p => p.PriceComponentType == PriceDetail.PriceComponentTypes.Tax).Select(x =>
                   {
                       var tax = pricing.tax?.FirstOrDefault(t => x.PriceComponentName == t.code);
                       if(tax?.nettingResult?.FirstOrDefault()?.status != "REFUND")
                       {
                           return new Tax()
                           {
                               Amount = x.Amount,
                               Code = x.PriceComponentName
                           };
                       }

                       return null;
                   }).Where(x => x != null)
               },
               new()
               {
                   Amount = new CurrencyAmount
                   {
                       BaseCurrency = order.priceDetails.First().Amount.BaseCurrency,
                       MonetaryAmount = (basePrice + totalTax).ToString()
                   },
                   PriceComponentType = PriceComponentTypes.BasePriceEnum
               }
            };
            return salepriceLst;
        }

        private static List<PenaltyPrice> LoadPenaltiesWithPrice(resultRefundRefundSolutionPricing price, List<Penalty> penalties, string ptc)
        {
            List<PenaltyPrice> penaltyPrices = null;

            if (price.penaltyPrice != null)
            {
                penaltyPrices ??= [];
                var amount = new CurrencyAmount()
                {
                    BaseCurrency = price.penaltyPrice.First().currency,
                    MonetaryAmount = price.penaltyPrice.First().amount.ToString()
                };

                penaltyPrices.Add(new() { Amount = amount });

                if (!penalties.Any(x => x.Id.Contains(ptc)))
                {
                    penalties.Add(new()
                    {
                        Id = $"{GetPenaltyCancellationId(ptc)}",
                        Type = "Cancellation",
                        Text = $"{GetPtcDesc(ptc)} Cancellation Penalty",
                        MinPrice = new() { Amount = amount },
                        MaxPrice = new() { Amount = amount },
                    });
                }
            }

            return penaltyPrices;
        }


        private static string GetPtcDesc(string ptc)
        {
            return ptc switch
            {
                "GBE" => "Young Adult",
                "CNN" => "Child",
                "INF" => "Infant",
                _ => "Adult",
            };
        }

        private static string GetPenaltyCancellationId(string ptc)
        {
            return ptc switch
            {
                "GBE" => "PEN2-REFUND",
                "CNN" => "PEN3-REFUND",
                "INF" => "PEN4-REFUND",
                _ => "PEN1-REFUND",
            };
        }


        private static List<Price> GetSalePriceDifference(resultRefundRefundSolutionPricing price)
        {
            List<Price> salePriceDifference = null;
            if (price.saleFareDifference != null)
            {
                salePriceDifference ??= [];
                salePriceDifference.Add(new Price
                {
                    Amount = new CurrencyAmount
                    {
                        BaseCurrency = price.saleFareDifference.currency,
                        MonetaryAmount = price.saleFareDifference.amount.ToString()
                    },
                    PriceComponentType = Price.PriceComponentTypes.BaseFareEnum
                });
                salePriceDifference.Add(new Price
                {
                    Amount = new CurrencyAmount
                    {
                        BaseCurrency = price.saleTaxTotalDifference.currency,
                        MonetaryAmount = price.saleTaxTotalDifference.amount.ToString()
                    },
                    PriceComponentType = Price.PriceComponentTypes.TotalTaxEnum,
                    TaxBreakup = price.tax?.Select(x => new Tax()
                    {
                        Code = x.code,
                        CountryCode = x.country,
                        Amount = new CurrencyAmount
                        {
                            BaseCurrency = x.salePriceDifference.currency,
                            MonetaryAmount = x.salePriceDifference.amount.ToString()
                        },
                        Refundable = string.Equals(x.nettingResult?.FirstOrDefault()?.status, "REFUND", StringComparison.OrdinalIgnoreCase),
                    })
                });

                if (price.refundPrice != null)
                {
                    salePriceDifference.Add(new Price
                    {
                        Amount = new CurrencyAmount
                        {
                            BaseCurrency = price.refundPrice.First().currency,
                            MonetaryAmount = (price.saleFareDifference.amount + price.saleTaxTotalDifference.amount).ToString()
                        },
                        PriceComponentType = Price.PriceComponentTypes.BasePriceEnum
                    });
                }
            }

            return salePriceDifference;
        }
    }
}

