using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMSReshopCancel;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;

namespace VAA.EAPI.NDC.Offer.Mapper
{
    public static class OrderReshopCancelResponseMapper
    {
        public static IATA_OrderReshopRS MapReshopResponse(OrderDocument orderDocument, CancelReshopResponse cancelReshopResponse)
        {
            return new()
            {
                Items = new OrderReshopResponseType[1]
                {
                    new()
                    {
                        Order = new OrderKeyType
                        {
                            OrderID = orderDocument.OrderNumber.OrderId,
                            OwnerCode = Constants.Application.OWNERCODE
                        },
                        ReshopResults = new ReshopResultsType
                        {
                            Item = new ReshopOffersType
                            {
                                Offer = GetReshopOffers(cancelReshopResponse,orderDocument.OrderItems , cancelReshopResponse.DataList, cancelReshopResponse.IsRfc)
                            }
                        },
                        DataLists = new()
                        {
                            PaxList = GetPaxList(cancelReshopResponse.DataList.CustomerDetails),
                            PenaltyList = (cancelReshopResponse.DataList.Penalty != null && cancelReshopResponse.DataList.Penalty.Any() && cancelReshopResponse.IsRfc == false )? GetPenaltyList(cancelReshopResponse.DataList.Penalty) : null
                        }
                    }
                }
            };
        }
        private static PaxType[] GetPaxList(IEnumerable<Customer> customerDetails)
        {
            return customerDetails.Select((cust, index) =>
                new PaxType()
                {

                    PaxID = cust.CustId,
                    PTC = cust.Ptc == Constants.Application.CNNPTCCode ? Constants.Application.ChildPTCCode : cust.Ptc,
                }).ToArray();
        }

        private static PenaltyType[] GetPenaltyList(IEnumerable<Penalty> penalties)
        {
            return penalties?.Select(p => new PenaltyType()
            {
                DescText = [p.Text],
                PenaltyID = p.Id,
                TypeCode = PenaltyTypeCodeType.Cancellation,
                Price = new()
                {
                    TotalAmount = new()
                    {
                        CurCode = p.MaxPrice.Amount.BaseCurrency,
                        Value = Convert.ToDecimal(p.MaxPrice.Amount.MonetaryAmount)
                    }
                }
            }).ToArray();
        }

        private static ChangedOfferType[] GetReshopOffers(CancelReshopResponse cancelReshopResponse, IEnumerable<OrderItems> orderItems, Infrastructure.Model.OfferMS.DataList dataList, bool isRfc)
        {
            return cancelReshopResponse.Offers.Select(x => new ChangedOfferType
            {
                OfferID = cancelReshopResponse.ShoppingResponseId,
                OwnerCode = Constants.Application.OWNERCODE,
                DeleteOrderItem = GetDeleteOrderItem(x, orderItems, dataList, cancelReshopResponse.ShoppingResponseId, isRfc),
                OfferExpirationTimeLimitDateTime = Convert.ToDateTime(x.OfferExpirationTimeLimit).ToUniversalTime(),
                OfferExpirationTimeLimitDateTimeSpecified = true
            }).ToArray();
        }

        private static OrderItemDeletionOfferItemType[] GetDeleteOrderItem(ReshopOffer offer, IEnumerable<OrderItems> orderItems, Infrastructure.Model.OfferMS.DataList dataList, string offerId, bool isRfc)
        {
            return offer.OfferItems.Select((offerItem, index) =>
            {
                var deleteOfferItem = new OrderItemDeletionOfferItemType()
                {
                    ExistingOrderItem = [
                        new ExistingOrderItemType()
                        {
                            OrderItemRefID = orderItems.ElementAt(index).OrderItemId,
                            ExistingService = GetExistingServices(orderItems.ElementAt(index))
                        }],
                    OfferItemID = $"{offerId}-{index + 1}",
                    PriceDifferential = new PriceDifferentialType()
                    {
                        DifferentialTypeCode = DifferentialTypeCodeType.Refund,
                        DiffPrice = new DiffPriceAndFareDetailsType()
                        {
                            FareDetail = GetFareDetailsList(offerItem, dataList, isRfc),
                            Price = GetDiffPrice(offerItem, dataList, isRfc)
                        },
                        OldPrice = new PriceAndFareDetailsType()
                        {
                            Price = GetFarePrice(offerItem.OfferitemPricing.First().OriginalSalePrice)
                        },
                        NewPrice = new PriceAndFareDetailsType()
                        {
                            FareDetail = GetNewPriceFareDetail(offerItem, dataList, isRfc),
                            Price = GetFarePrice(offerItem.OfferitemPricing.First().SalePrice)
                        }
                    },
                };

                return deleteOfferItem;
            }).ToArray();
        }

        private static ExistingServiceType[] GetExistingServices(OrderItems orderItem)
        {
            var usedCouponSegmentRefIds = orderItem.AccountableDocumentRefs?.SelectMany(acc => acc.Coupons ?? []).Where(coupon => coupon.CouponStatus == Constants.Application.CouponStatus).Select(coupon => coupon.SegmentRefId).ToList() ?? [];
            return orderItem.Services.Select(x => new ExistingServiceType() { ServiceRefID = x.ServiceId , FutureStatusCode = x.SegmentRefIds?.Any(usedCouponSegmentRefIds.Contains) == true ? Constants.Application.ConfirmedOrderItemStatus : Constants.Application.CancelledOrderItemStatus }).ToArray();
        }

        private static DifferentialPriceType GetDiffPrice(ReshopOfferItem offerItem, Infrastructure.Model.OfferMS.DataList dataList, bool isRfc)
        {
            List<DifferentialFareDetailType> differentialFareDetailTypes = [];
            var pricing = offerItem.OfferitemPricing.First();
            var currency = offerItem.OfferitemPricing.First().OriginalSalePrice.First().Amount.BaseCurrency; 
            var refundPrices = offerItem.OfferitemPricing.FirstOrDefault()?.RefundPrice;

            var price = new DifferentialPriceType()
            {
                BaseAmount = new AmountType()
                {
                    CurCode = currency,
                    Value = Convert.ToDecimal(pricing.SalePriceDifference.FirstOrDefault(x => x.PriceComponentType == Price.PriceComponentTypes.BaseFareEnum).Amount.MonetaryAmount)
                },
                DueByAirlineAmount = new AmountType()
                {
                    CurCode = currency,
                    Value = refundPrices != null ? Convert.ToDecimal(refundPrices?.FirstOrDefault()?.Amount.MonetaryAmount.Replace("-", "")) : 0.00m
                },
                DueToAirlineAmount = new AmountType()
                {
                    CurCode = currency,
                    Value = Convert.ToDecimal(0.00m)
                },
                TotalAmount = new AmountType()
                {
                    CurCode = currency,
                    Value = Convert.ToDecimal(pricing.SalePriceDifference.FirstOrDefault(x => x.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount)
                },
                TaxSummary = GetPriceTaxSummary(pricing.SalePriceDifference)
            };

            return price;
        }

        private static DifferentialFareDetailType[] GetFareDetailsList(ReshopOfferItem offerItem,Infrastructure.Model.OfferMS.DataList dataList,bool isRfc)
        {
            List<DifferentialFareDetailType> differentialFareDetailTypes = new();

            var penaltyRefId = isRfc == false ? offerItem.Services.First().PenaltyRef.CancellationPenaltyRefIds : null;
            if (penaltyRefId.IsNullOrEmpty()) return differentialFareDetailTypes.ToArray();

            var penalty = dataList.Penalty != null && dataList.Penalty.Any() ? dataList.Penalty.FirstOrDefault(x => x.Id == penaltyRefId.First()) : null;
            if (penalty == null)
                return differentialFareDetailTypes.ToArray();

            DifferentialFareDetailType differentialFareDetailType = new()
            {
                PaxRefID = offerItem.Services.First().ServiceBeneficiaries.ToArray(),
                PenaltyRefID = penaltyRefId.ToArray()
            };

            differentialFareDetailTypes.Add(differentialFareDetailType);

            return differentialFareDetailTypes.ToArray();
        }

        private static FareDetailType[] GetOldPriceFareDetail(ReshopOfferItem offerItem)
        {
            return [new()
            {
                PaxRefID = offerItem.Services.First().ServiceBeneficiaries.ToArray(),
                
            } ];
        }

        private static FareDetailType[] GetNewPriceFareDetail(ReshopOfferItem offerItem, Infrastructure.Model.OfferMS.DataList dataList, bool isRfc)
        {
            var ptc = dataList.CustomerDetails.FirstOrDefault(c => offerItem.Services.First().ServiceBeneficiaries.Contains(c.CustId)).Ptc;

            return [new()
            {
                PaxRefID = offerItem.Services.First().ServiceBeneficiaries.ToArray()
            } ];
        }

        private static PriceType GetFarePrice(IEnumerable<Price> salePrice)
        {
            PriceType priceType = new()
            {
                BaseAmount = new AmountType()
                {
                    CurCode = salePrice.FirstOrDefault(s => s.PriceComponentType == Price.PriceComponentTypes.BaseFareEnum)?.Amount.BaseCurrency,
                    Value = Convert.ToDecimal(salePrice.FirstOrDefault(s => s.PriceComponentType == Price.PriceComponentTypes.BaseFareEnum)?.Amount.MonetaryAmount)
                },
                TotalAmount = new AmountType()
                {
                    CurCode = salePrice.FirstOrDefault(s => s.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum)?.Amount.BaseCurrency,
                    Value = Convert.ToDecimal(salePrice.FirstOrDefault(s => s.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum)?.Amount.MonetaryAmount)
                },
                TaxSummary = GetPriceTaxSummary(salePrice)
            };

            return priceType;
        }

        private static TaxSummaryType[] GetPriceTaxSummary(IEnumerable<Price> salePrice)
        {
            return salePrice.Where(x => x.PriceComponentType == Price.PriceComponentTypes.TotalTaxEnum)?.Select(totalTax =>
                new TaxSummaryType()
                {
                    TotalTaxAmount = new AmountType()
                    {
                        CurCode = totalTax.Amount.BaseCurrency,
                        Value = Convert.ToDecimal(totalTax.Amount.MonetaryAmount)
                    },
                    Tax = GetTaxBreakdown(totalTax.TaxBreakup),
                    TotalRefundableTaxAmount = new AmountType
                    {
                        CurCode = totalTax.Amount.BaseCurrency,
                        Value = Convert.ToDecimal(CalculateRefundableAmount(totalTax.TaxBreakup?.Where(x => x.Refundable == true).Select(y => y.Amount).ToList())),
                    }
                }).ToArray();
        }

        private static decimal CalculateRefundableAmount(IEnumerable<CurrencyAmount> refundableAmount)
        {
            if (refundableAmount != null && refundableAmount.Any())
            {
                return refundableAmount.Select(x => Convert.ToDecimal(x.MonetaryAmount)).Sum();
            }
            return 0.00m;
        }

        private static TaxType[] GetTaxBreakdown(IEnumerable<Tax> taxes)
        {
            return taxes?.Select(tax => new TaxType()
            {
                Amount = new AmountType()
                {
                    CurCode = tax.Amount.BaseCurrency,
                    Value = Convert.ToDecimal(tax.Amount?.MonetaryAmount)
                },
                TaxCode = tax.Code
            }).ToArray();
        }
    }
}
