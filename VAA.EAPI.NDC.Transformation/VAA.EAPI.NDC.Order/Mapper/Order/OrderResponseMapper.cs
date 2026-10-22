using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Baggage;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using static VAA.EAPI.Domain.Virgin.Models.Common.Price;
using LengthUnitCodeType = VAA.EAPI.Domain.Virgin.Models.Products.Baggage.LengthUnitCodeType;
using OrderItems = VAA.EAPI.Domain.Virgin.Models.Order.OrderItems;
using WeightUnitCodeType = VAA.EAPI.NDC.Order.Domain.OrderViewRS.WeightUnitCodeType;

namespace VAA.EAPI.NDC.Order.Mapper.Order
{
    public static class OrderResponseMapper
    {
        public static IATA_OrderViewRS MapOrderResponse(OrderDocument orderDocument, [Optional] List<EAPI.Domain.Entities.Common.NDCWarning> nDCWarnings, [Optional] bool isHoldBooking, [Optional] bool isOrderChange, [Optional] bool isOrderCancel, [Optional] bool isNameCorrection, [Optional] OrderDocument parentOrderDoc, [Optional] OrderDocument childOrderDoc, [Optional] bool isCashPayment, [Optional] bool isEasyPay, [Optional] string approvalCode, [Optional] bool retriveResponse, [Optional] bool isChangeReshopHoldBooking, [Optional] bool isReshopDisruption)
        {
            return MapOrderResponseInternal(orderDocument, nDCWarnings, isHoldBooking, isOrderChange, isOrderCancel, isNameCorrection, parentOrderDoc, childOrderDoc, isCashPayment, isEasyPay, approvalCode, retriveResponse, isChangeReshopHoldBooking, isReshopDisruption, false);
        }

        public static IATA_OrderViewRS MapOrderRetrieveResponse(OrderDocument orderDocument, List<EAPI.Domain.Entities.Common.NDCWarning> nDCWarnings, OrderRetrieveResponseMapOptions options)
        {
            options ??= new OrderRetrieveResponseMapOptions();

            return MapOrderResponseInternal(orderDocument, nDCWarnings, options.IsHoldBooking, false, false, false, null, null, options.IsCashPayment, options.IsEasyPay, options.ApprovalCode, true, false, false, options.IsDisruptedOrderRetrieve);
        }

        private static IATA_OrderViewRS MapOrderResponseInternal(OrderDocument orderDocument, List<EAPI.Domain.Entities.Common.NDCWarning> nDCWarnings, bool isHoldBooking, bool isOrderChange, bool isOrderCancel, bool isNameCorrection, OrderDocument parentOrderDoc, OrderDocument childOrderDoc, bool isCashPayment, bool isEasyPay, string approvalCode, bool retriveResponse, bool isChangeReshopHoldBooking, bool isReshopDisruption, bool isDisruptedOrderRetrieve)
        {

            IEnumerable<PaxType> paxList = Enumerable.Empty<PaxType>();
            IEnumerable<PaxSegmentType> paxSegmentList = Enumerable.Empty<PaxSegmentType>();
            IEnumerable<ContactInfoType> contactInfoList = Enumerable.Empty<ContactInfoType>();
            IEnumerable<OriginDestType> originDestList = Enumerable.Empty<OriginDestType>();
            IEnumerable<PaxJourneyType> paxJourneyList = Enumerable.Empty<PaxJourneyType>();
            IEnumerable<BaggageAllowanceType> baggageAllowanceList = Enumerable.Empty<BaggageAllowanceType>();
            IEnumerable<DatedMarketingSegmentType> datedMarketingSegmentList = Enumerable.Empty<DatedMarketingSegmentType>();
            IEnumerable<DatedOperatingSegmentType> datedOperatingSegmentList = Enumerable.Empty<DatedOperatingSegmentType>();
            IEnumerable<DatedOperatingLegType> datedOperatingLegList = Enumerable.Empty<DatedOperatingLegType>();
            IEnumerable<ServiceDefinitionType> serviceDefinitionList = Enumerable.Empty<ServiceDefinitionType>();
            IEnumerable<PriceClassType> priceClassList = Enumerable.Empty<PriceClassType>();
            IEnumerable<PenaltyType> penaltyTypeList = Enumerable.Empty<PenaltyType>();

            return new IATA_OrderViewRS()
            {

                Items = GetOrderViewResponse(orderDocument, nDCWarnings, isHoldBooking, isOrderChange, isOrderCancel, isNameCorrection, ref paxList, ref paxSegmentList, ref contactInfoList, ref originDestList, ref paxJourneyList, ref baggageAllowanceList, ref datedMarketingSegmentList, ref datedOperatingSegmentList, ref datedOperatingLegList, ref serviceDefinitionList, ref priceClassList, ref penaltyTypeList, parentOrderDoc, childOrderDoc, isCashPayment, isEasyPay, isChangeReshopHoldBooking, isReshopDisruption, isDisruptedOrderRetrieve),

                PaymentFunctions = (parentOrderDoc != null && childOrderDoc != null) ? GetPaymentFunction(parentOrderDoc, isOrderChange, approvalCode, GetVisibleOrderItems(parentOrderDoc, isDisruptedOrderRetrieve)).Concat(GetPaymentFunction(childOrderDoc, isOrderChange, approvalCode, GetVisibleOrderItems(childOrderDoc, isDisruptedOrderRetrieve))).ToArray()
                                    : isHoldBooking ? null : GetPaymentFunction(orderDocument, isOrderChange, approvalCode, GetVisibleOrderItems(orderDocument, isDisruptedOrderRetrieve)),

                AugmentationPoint = retriveResponse ? GetBookingIds(orderDocument) : null
            };
        }

        private static AugmentationPointType GetBookingIds(OrderDocument orderDocument)
        {
            var childRlocDetails = orderDocument?.BookingReference?.SplitRLOC?.ChildRLOCDetails;

            if (childRlocDetails == null || !childRlocDetails.Any()) return null;

            var xmlDoc = new XmlDocument();
            var orders = new List<XmlElement>();
            var ns = Constants.Application.NameSpace;

            foreach (var order in childRlocDetails)
            {
                var orderElement = xmlDoc.CreateElement("Order", ns);

                var orderIdElement = xmlDoc.CreateElement("OrderID", ns);
                orderIdElement.InnerText = $"VS932{order.ChildRLOC}{DateTime.UtcNow:yy}";
                orderElement.AppendChild(orderIdElement);

                var bookingRefElement = xmlDoc.CreateElement("BookingRef", ns);

                var bookingIdElement = xmlDoc.CreateElement("BookingID", ns);
                bookingIdElement.InnerText = order.ChildRLOC;
                bookingRefElement.AppendChild(bookingIdElement);

                orderElement.AppendChild(bookingRefElement);

                orders.Add(orderElement);
            }

            return new AugmentationPointType
            {
                Any = orders.ToArray()
            };
        }

        private static ReceiptIATA_EasyPayType MapEasyPayPaymentSummary(string approvalCode)
        {
            return new ReceiptIATA_EasyPayType
            {
                ApprovalCode = approvalCode
            };
        }

        private static SettlementPlanType MapCashPaymentSummary(string agencyId)
        {
            return new SettlementPlanType
            {
                IATA_Number = Convert.ToDecimal(agencyId),
                IATA_NumberSpecified = true,
                PaymentTypeCode = Constants.Application.IFGPaymentTypeCode
            };
        }

        private static OrderViewResponseType[] GetOrderViewResponse(OrderDocument orderDocument, List<EAPI.Domain.Entities.Common.NDCWarning> nDCWarnings, bool isHoldBooking, bool isOrderChange, bool isOrderCancel, bool isNameCorrection, ref IEnumerable<PaxType> paxList, ref IEnumerable<PaxSegmentType> paxSegmentList, ref IEnumerable<ContactInfoType> contactInfoList, ref IEnumerable<OriginDestType> originDestList, ref IEnumerable<PaxJourneyType> paxJourneyList, ref IEnumerable<BaggageAllowanceType> baggageAllowanceList, ref IEnumerable<DatedMarketingSegmentType> datedMarketingSegmentList, ref IEnumerable<DatedOperatingSegmentType> datedOperatingSegmentList, ref IEnumerable<DatedOperatingLegType> datedOperatingLegList, ref IEnumerable<ServiceDefinitionType> serviceDefinitionList, ref IEnumerable<PriceClassType> priceClassList, ref IEnumerable<PenaltyType> penaltyTypeList, [Optional] OrderDocument parentOrderDoc, [Optional] OrderDocument childOrderDoc, bool isCashPayment, bool isEasyPay, bool isChangeReshopHoldBooking, bool isReshopDisruption, bool isDisruptedOrderRetrieve)
        {

            return new OrderViewResponseType[]
            {
                new OrderViewResponseType()
                {
                    DataLists = GetDataList(orderDocument.DataList, isOrderChange, isOrderCancel,orderDocument, isNameCorrection, ref paxList, ref paxSegmentList, ref contactInfoList, ref originDestList, ref paxJourneyList, ref baggageAllowanceList, ref datedMarketingSegmentList,  ref datedOperatingSegmentList, ref datedOperatingLegList, ref serviceDefinitionList,  ref priceClassList, ref penaltyTypeList),
                    Order = parentOrderDoc != null && childOrderDoc != null
                        ? GetSplitOrder(orderDocument, isOrderChange, isOrderCancel, ref serviceDefinitionList, ref baggageAllowanceList, parentOrderDoc, childOrderDoc, isChangeReshopHoldBooking, isReshopDisruption, isDisruptedOrderRetrieve)
                        : GetOrder(orderDocument, isHoldBooking, isOrderChange, isOrderCancel, ref serviceDefinitionList, ref baggageAllowanceList, isChangeReshopHoldBooking, isReshopDisruption, isDisruptedOrderRetrieve),
                    TicketDocInfo = GetTicketDoc(orderDocument, isOrderChange, isNameCorrection, isCashPayment, isEasyPay, isDisruptedOrderRetrieve, GetVisibleOrderItems(orderDocument, isDisruptedOrderRetrieve)),
                    Warning = (nDCWarnings?.Any() ?? false) ? GetWarnings(nDCWarnings) : null
                }
            };
        }

        private static OrderedOrderType[] GetSplitOrder(OrderDocument orderDocument, bool isOrderChange, bool isOrderCancel, ref IEnumerable<ServiceDefinitionType> serviceDefinitionList, ref IEnumerable<BaggageAllowanceType> baggageAllowanceList, OrderDocument parentOrderDoc, OrderDocument childOrderDoc, bool isChangeReshopHoldBooking, bool isReshopDisruption, bool isDisruptedOrderRetrieve)
        {
            var parentOrderItems = GetOrderItems(parentOrderDoc, false, isOrderChange, isOrderCancel, serviceDefinitionList, baggageAllowanceList, isChangeReshopHoldBooking, isReshopDisruption, isDisruptedOrderRetrieve);
            var childOrderItems = GetOrderItems(childOrderDoc, false, isOrderChange, isOrderCancel, serviceDefinitionList, baggageAllowanceList, isChangeReshopHoldBooking, isReshopDisruption, isDisruptedOrderRetrieve);

            return new OrderedOrderType[]
            {
                new OrderedOrderType()
                {
                    OrderID = parentOrderDoc?.OrderNumber?.OrderId,
                    OwnerCode = parentOrderDoc?.OrderNumber?.OwnerAirline,
                    OrderItem = parentOrderItems,
                    StatusCode= GetOrderStatus(parentOrderItems , isOrderChange),
                    TotalPrice = GetTotalPrice(parentOrderDoc, isOrderChange, isDisruptedOrderRetrieve)
                },
                new OrderedOrderType()
                {
                    OrderID = childOrderDoc?.OrderNumber?.OrderId,
                    OwnerCode = childOrderDoc?.OrderNumber?.OwnerAirline,
                    OrderItem = childOrderItems,
                    StatusCode= GetOrderStatus(childOrderItems , isOrderChange),
                    TotalPrice = GetTotalPrice(childOrderDoc, isOrderChange, isDisruptedOrderRetrieve)
                }
            };
        }

        public static PaymentFunctionsOrderType[] GetPaymentFunction(OrderDocument orderDocument, bool isOrderChange, [Optional] string approvalCode)
        {
            return GetPaymentFunction(orderDocument, isOrderChange, approvalCode, orderDocument.OrderItems);
        }

        private static PaymentFunctionsOrderType[] GetPaymentFunction(OrderDocument orderDocument, bool isOrderChange, string approvalCode, IEnumerable<OrderItems> visibleOrderItems)
        {
            if (isOrderChange)
            {
                var latestHistory = orderDocument.OrderHistory.OrderBy(v => v.Version).Last();
                orderDocument.OrderItems = latestHistory.OrderItems.Concat(orderDocument.OrderItems);
                visibleOrderItems = orderDocument.OrderItems;
            }
            var transactionIds = visibleOrderItems.Where(item => item.PaymentDetails != null)?.SelectMany(id => id.PaymentDetails)?.Select(x => x.PaymentReferenceId)?.Distinct()?.ToList();
            var agencyId = decimal.Parse(orderDocument.Parties.Participants.FirstOrDefault(x => x.PartyType == Infrastructure.Model.Participant.PartyTypeEnum.Seller)?.AgencyID).ToString("G29");

            var paymentFunction = transactionIds.Where(x => x != null)?.Select(item =>
            {
                var orderItems = visibleOrderItems.Where(x => x.PaymentDetails != null && x.PaymentDetails.Any(x => x.PaymentReferenceId == item));
                var paymentDetail = orderItems.SelectMany(x => x.PaymentDetails).First(p => p.PaymentReferenceId == item);
                var paymentCommitmentDateTime = $"{paymentDetail.PaymentCommitmentDateTime}Z";
                PaymentProcessingSummaryPaymentMethodType paymentMethodType;
                {
                    if (paymentDetail.Amount?.MonetaryAmount == "0.00")
                    {
                        paymentMethodType = new PaymentProcessingSummaryPaymentMethodType
                        {
                            Item = new OfflinePaymentInstructionReceiptType
                            {
                                PaymentBrandCode = Constants.Application.OfflineBrandTypeCode,
                                PaymentTypeCode = Constants.Application.OfflinePaymentTypeCode

                            }
                        };
                    }
                    else
                    {
                        paymentMethodType = new PaymentProcessingSummaryPaymentMethodType
                        {
                            Item = paymentDetail.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum ? MapCashPaymentSummary(agencyId)
                               : paymentDetail.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay ? MapEasyPayPaymentSummary(approvalCode)
                               : orderItems.First().PaymentDetails is not null ? GetPaymentProcessingSummaryPaymentMethod(orderItems.First(), item) : null,
                        };
                    }
                }
                return new PaymentFunctionsOrderType()
                {
                    OrderAssociation = new OrderAssociationType()
                    {
                        OrderRefID = orderDocument.OrderNumber.OrderId,
                        OrderItemRefID = orderItems?.Select(item => item.OrderItemId)?.ToArray()
                    },
                    PaymentProcessingSummary = new PaymentProcessingSummaryType()
                    {
                        Amount = new AmountType()
                        {
                            CurCode = visibleOrderItems.First().PriceDetails.First().Amount.BaseCurrency,
                            Value = ConvertToDecimal(Convert.ToDecimal(orderItems.SelectMany(x => x.PaymentDetails).First(p => p.PaymentReferenceId == item).Amount?.MonetaryAmount))
                        },
                        PaymentCommitmentDateTime = Convert.ToDateTime(paymentCommitmentDateTime).ToUniversalTime(),
                        PaymentCommitmentDateTimeSpecified = true,
                        PaymentID = item,
                        PaymentStatusCode = orderItems.SelectMany(x => x.PaymentDetails).First(p => p.PaymentReferenceId == item).PaymentProcessingStatus.ToString(),
                        PaymentProcessingSummaryPaymentMethod = paymentMethodType,

                    }
                };
            });

            return paymentFunction.ToArray();
        }

        private static WarningType[] GetWarnings(List<NDCWarning> nDCWarnings)
        {
            return nDCWarnings.Select(x => new WarningType()
            {
                Code = Convert.ToString(x.Code),
                DescText = x.ShortText,
                LangCode = Constants.Application.LangCode,
                TypeCode = x.Type,
                TagText = x.TagText,
                URL = x?.DocURL?.Href
            }).ToArray();
        }

        private static OrderedOrderType[] GetOrder(OrderDocument orderDocument, bool isHoldBooking, bool isOrderChange, bool isOrderCancel, ref IEnumerable<ServiceDefinitionType> serviceDefinitionList, ref IEnumerable<BaggageAllowanceType> baggageAllowanceList, bool isChangeReshopHoldBooking, bool isReshopDisruption, bool isDisruptedOrderRetrieve)
        {
            var orderItems = GetOrderItems(orderDocument, isHoldBooking, isOrderChange, isOrderCancel, serviceDefinitionList, baggageAllowanceList, isChangeReshopHoldBooking, isReshopDisruption, isDisruptedOrderRetrieve);

            return new OrderedOrderType[]
            {
                new OrderedOrderType()
                {
                    OrderID = orderDocument?.OrderNumber?.OrderId,
                    OwnerCode = orderDocument?.OrderNumber?.OwnerAirline,
                    OrderItem = orderItems,
                    StatusCode= GetOrderStatus(orderItems , isOrderChange),
                    TotalPrice = GetTotalPrice(orderDocument, isOrderChange, isDisruptedOrderRetrieve)
                }
            };
        }

        private static PriceType GetTotalPrice(OrderDocument orderDocument, bool isOrderChange, bool isDisruptedOrderRetrieve = false)
        {
            PriceType priceType = new();
            decimal totalAmount = decimal.Zero;
            string currency = string.Empty;

            foreach (var orderItem in GetVisibleOrderItems(orderDocument, isDisruptedOrderRetrieve))
            {
                var priceDetails = GetResponsePriceDetails(orderItem, isDisruptedOrderRetrieve);

                if (priceDetails == null)
                {
                    continue;
                }

                foreach (var p in priceDetails)
                {
                    if (p.PriceComponentType == null)
                        totalAmount += Convert.ToDecimal(p.Amount.MonetaryAmount);

                    if (p.PriceComponentType == PriceComponentTypes.BasePriceEnum)
                    {
                        totalAmount += Convert.ToDecimal(p.Amount.MonetaryAmount) * orderItem.Services.SelectMany(x => x.ServiceBeneficiaries).Distinct().Count();
                        currency = p.Amount.BaseCurrency;
                    }
                }
            }

            priceType.TotalAmount = MapToAmount(ConvertToDecimal(totalAmount), currency);
            return priceType;
        }

        private static AmountType MapToAmount(decimal monetaryAmount, string currencyCode)
        {
            if (!monetaryAmount.ToString().Equals("#.00"))
            {
                return new AmountType()
                {
                    CurCode = currencyCode,
                    Value = Convert.ToDecimal(monetaryAmount)
                };
            }
            else
            {
                var monetaryHolder = Convert.ToDecimal(monetaryAmount);
                string correctedMonetaryAmount = monetaryHolder.ToString("#.00", System.Globalization.CultureInfo.InvariantCulture);
                return new AmountType()
                {
                    CurCode = currencyCode,
                    Value = Convert.ToDecimal(correctedMonetaryAmount)
                };
            }
        }

        private static OrderedOrderItemType[] GetOrderItems(OrderDocument orderDocument, bool isHoldBooking, bool isOrderChange, bool isOrderCancel, IEnumerable<ServiceDefinitionType> serviceDefinitionList, IEnumerable<BaggageAllowanceType> baggageAllowanceList, bool isChangeReshopHoldBooking, bool isReshopDisruption, bool isDisruptedOrderRetrieve)
        {
            if (isOrderChange)
            {
                var latestHistory = orderDocument.OrderHistory.OrderBy(v => v.Version).Last();
                return latestHistory.OrderItems.Select(orderItem =>
                    new OrderedOrderItemType()
                    {
                        OrderItemID = orderItem.OrderItemId,
                        OwnerCode = orderItem.OwnerAirline,
                        Price = GetPrice(orderItem.PriceDetails),
                        FareDetail = orderItem.Services.Any(x => x.Product?.ProductType == Product.ProductTypes.FlightEnum) ? GetFareDetails(orderDocument, orderItem.Services, latestHistory.DataList, orderItem.PriceDetails.FirstOrDefault(x => x.PriceComponentType == PriceComponentTypes.BasePriceEnum)?.Amount?.BaseCurrency, isOrderChange) : null,
                        Service = GetServices(orderItem.Services, orderDocument, isOrderChange, latestHistory.DataList.SegmentDetails, latestHistory.OrderItems, serviceDefinitionList, baggageAllowanceList, latestHistory.DataList),
                        PaymentTimeLimitDateTime = !string.IsNullOrEmpty(latestHistory.TimeLimits?.FirstOrDefault()?.Limit) ? Convert.ToDateTime(latestHistory.TimeLimits?.FirstOrDefault()?.Limit).ToUniversalTime() : DateTime.MinValue,
                        PaymentTimeLimitDateTimeSpecified = !string.IsNullOrEmpty(latestHistory.TimeLimits?.FirstOrDefault()?.Limit),
                        StatusCode = GetOrderItemStatus(orderItem.Services, orderDocument, isOrderChange, latestHistory.DataList.SegmentDetails, latestHistory.OrderItems, serviceDefinitionList, baggageAllowanceList, latestHistory.DataList),
                    }).Concat(orderDocument.OrderItems.Select(orderItem =>
                    new OrderedOrderItemType()
                    {
                        OrderItemID = orderItem.OrderItemId,
                        ExistingOrderItemID = orderItem.ExistingOrderItemId,
                        OwnerCode = orderItem.OwnerAirline,
                        Price = isOrderChange ? null : GetPrice(orderItem.PriceDetails),
                        FareDetail = orderItem.Services.Any(x => x.Product?.ProductType == Product.ProductTypes.FlightEnum) ? GetFareDetails(orderDocument, orderItem.Services, orderDocument.DataList, orderItem.PriceDetails.FirstOrDefault(x => x.PriceComponentType == PriceComponentTypes.BasePriceEnum)?.Amount?.BaseCurrency, isOrderChange) : null,
                        Service = GetServices(orderItem.Services, orderDocument, isOrderChange, orderDocument.DataList.SegmentDetails, orderDocument.OrderItems, serviceDefinitionList, baggageAllowanceList, orderDocument.DataList),
                        PaymentTimeLimitDateTime = !string.IsNullOrEmpty(orderDocument.TimeLimits?.FirstOrDefault()?.Limit) ? Convert.ToDateTime(orderDocument.TimeLimits?.FirstOrDefault()?.Limit).ToUniversalTime() : DateTime.MinValue,
                        PaymentTimeLimitDateTimeSpecified = !string.IsNullOrEmpty(orderDocument.TimeLimits?.FirstOrDefault()?.Limit),
                        StatusCode = GetOrderItemStatus(orderItem.Services, orderDocument, isOrderChange, orderDocument.DataList.SegmentDetails, orderDocument.OrderItems, serviceDefinitionList, baggageAllowanceList, orderDocument.DataList),
                        PriceDifferential = isOrderChange && (orderItem.AccountableDocumentRefs != null || isChangeReshopHoldBooking || isReshopDisruption) && orderItem.Services?.FirstOrDefault()?.Product?.ProductType == Product.ProductTypes.FlightEnum ? GetPriceDifferential(orderItem, orderDocument.DataList.Penalty, isOrderCancel && isHoldBooking, isOrderChange, isOrderCancel, isChangeReshopHoldBooking, isReshopDisruption) : null
                    })).ToArray();
            }
            else
            {
                var visibleOrderItems = GetVisibleOrderItems(orderDocument, isDisruptedOrderRetrieve).ToArray();

                return visibleOrderItems.Select(orderItem =>
                new OrderedOrderItemType()
                {
                    OrderItemID = orderItem.OrderItemId,
                    OwnerCode = orderItem.OwnerAirline,
                    SellerFollowUpAction = isDisruptedOrderRetrieve ? null : GetSellerFollowUpAction(orderItem, orderDocument),
                    Price = isOrderCancel ? null : GetPrice(GetResponsePriceDetails(orderItem, isDisruptedOrderRetrieve)),
                    FareDetail = orderItem.Services.Any(x => x.Product?.ProductType == Product.ProductTypes.FlightEnum) ? GetFareDetails(orderDocument, orderItem.Services, orderDocument.DataList, GetResponsePriceDetails(orderItem, isDisruptedOrderRetrieve).FirstOrDefault(x => x.PriceComponentType == PriceComponentTypes.BasePriceEnum)?.Amount?.BaseCurrency, isOrderChange) : null,
                    Service = GetServices(orderItem.Services, orderDocument, isOrderChange, orderDocument.DataList.SegmentDetails, visibleOrderItems, serviceDefinitionList, baggageAllowanceList, orderDocument.DataList),
                    PaymentTimeLimitDateTime = !string.IsNullOrEmpty(orderDocument.TimeLimits?.FirstOrDefault()?.Limit) ? Convert.ToDateTime(orderDocument.TimeLimits?.FirstOrDefault()?.Limit).ToUniversalTime() : DateTime.MinValue,
                    PaymentTimeLimitDateTimeSpecified = !string.IsNullOrEmpty(orderDocument.TimeLimits?.FirstOrDefault()?.Limit),
                    StatusCode = GetOrderItemStatus(orderItem.Services, orderDocument, isOrderChange, orderDocument.DataList.SegmentDetails, visibleOrderItems, serviceDefinitionList, baggageAllowanceList, orderDocument.DataList),
                    PriceDifferential = isOrderCancel && (orderItem.AccountableDocumentRefs != null || isHoldBooking) && orderItem.Services?.FirstOrDefault()?.Product?.ProductType == Product.ProductTypes.FlightEnum ? GetPriceDifferential(orderItem, orderDocument.DataList.Penalty, isOrderCancel && isHoldBooking, isOrderChange, isOrderCancel, isChangeReshopHoldBooking, isReshopDisruption) : null
                }).ToArray();
            }
        }

        private static IEnumerable<OrderItems> GetVisibleOrderItems(OrderDocument orderDocument, bool isDisruptedOrderRetrieve)
        {
            if (!isDisruptedOrderRetrieve)
            {
                return orderDocument.OrderItems;
            }

            var replacedOrderItemIds = orderDocument.OrderItems
                .Where(orderItem => !orderItem.ExistingOrderItemId.IsNullOrWhiteSpace())
                .Select(orderItem => orderItem.ExistingOrderItemId)
                .ToHashSet();

            return orderDocument.OrderItems.Where(orderItem => !replacedOrderItemIds.Contains(orderItem.OrderItemId));
        }

        private static IEnumerable<Price> GetResponsePriceDetails(OrderItems orderItem, bool isDisruptedOrderRetrieve)
        {
            var shouldUseOriginalSalePrice = isDisruptedOrderRetrieve &&
                !orderItem.ExistingOrderItemId.IsNullOrWhiteSpace() &&
                orderItem.OriginalSalePrice?.Any() == true;

            return shouldUseOriginalSalePrice ? orderItem.OriginalSalePrice : orderItem.PriceDetails;
        }

        private static SellerFollowUpActionType GetSellerFollowUpAction(OrderItems orderItem, OrderDocument orderDocument)
        {
            if (orderItem.SellerFollowUpAction != null && orderItem.SellerFollowUpAction.Any())
            {
                return new()
                {
                    ActionCode = orderItem.SellerFollowUpAction.ToArray(),
                    ActionInd = true
                };
            }

            var isOcnTriggered = !string.IsNullOrEmpty(orderItem.OCNTimestamp);
            var hasDisruptedSegment = orderDocument.DataList?.SegmentDetails?.Any(segment =>
                segment.Status?.Code == Constants.Application.FlightStatusCodeCancelled ||
                segment.Status?.Code == "TK") ?? false;

            if (isOcnTriggered && hasDisruptedSegment)
            {
                return new()
                {
                    ActionCode = ["ContactAirline"],
                    ActionInd = true
                };
            }

            return null;
        }

        private static PriceDifferentialType GetPriceDifferential(OrderItems orderItem, [Optional] IEnumerable<Penalty> penalties, bool isCancelledHoldBooking, bool isOrderChange, bool isOrderCancel, bool isChangeReshopHoldBooking, bool isReshopDisruption)
        {
            DifferentialTypeCodeType differentialTypeCode;

            if (isReshopDisruption)
            {
                differentialTypeCode = DifferentialTypeCodeType.EvenExchange;
            }
            else if (isChangeReshopHoldBooking)
            {
                differentialTypeCode = DifferentialTypeCodeType.AddCol;
            }
            else if(isCancelledHoldBooking)
            {
                differentialTypeCode = DifferentialTypeCodeType.Refund;
            }
            else if ((orderItem.SalePriceDifference?.FirstOrDefault(s => s.PriceComponentType == PriceComponentTypes.BaseFareEnum)?.Amount?.MonetaryAmount == "0.00") && orderItem.SalePriceDifference?.FirstOrDefault(s => s.PriceComponentType == PriceComponentTypes.TotalTaxEnum)?.Amount?.MonetaryAmount == "0.00")
            {
                differentialTypeCode = DifferentialTypeCodeType.EvenExchange;
            }
            else if (orderItem.AddCollectPrice?.Amount != null)
            {
                differentialTypeCode = DifferentialTypeCodeType.AddCol;
            }
            else if (orderItem.RefundPrice?.FirstOrDefault()?.Amount != null)
            {
                differentialTypeCode = DifferentialTypeCodeType.Refund;
            }
            else if (Convert.ToDecimal(orderItem.PriceDetails?.FirstOrDefault(s => s.PriceComponentType == PriceComponentTypes.BaseFareEnum)?.Amount?.MonetaryAmount) == 0 && Convert.ToDecimal(orderItem.SalePriceDifference?.FirstOrDefault(s => s.PriceComponentType == PriceComponentTypes.BaseFareEnum)?.Amount?.MonetaryAmount) != 0)
            {
                differentialTypeCode = DifferentialTypeCodeType.Refund;
            }
            else
            {
                differentialTypeCode = DifferentialTypeCodeType.EvenExchange;
            }

            return orderItem.Services.Any(x => x.Product?.ProductType == Product.ProductTypes.SeatEnum) ? null : new()
            {
                DifferentialTypeCode = differentialTypeCode,
                DiffPrice = orderItem.SalePriceDifference is null ? null : new DiffPriceAndFareDetailsType
                {
                    Price = GetDifferencePrice(orderItem, orderItem.Services.First().ServiceBeneficiaries.ToArray(), penalties, isOrderChange, isOrderCancel, isChangeReshopHoldBooking, isReshopDisruption, isCancelledHoldBooking).Price,
                    FareDetail = GetFareDetail(orderItem, orderItem.Services.First().ServiceBeneficiaries.ToArray(), isOrderChange)
                },
                NewPrice = new()
                {
                    Price = GetPrice(orderItem.PriceDetails, isReshopDisruption),
                    FareDetail = [new()
                    {
                        PaxRefID = orderItem.Services.First().ServiceBeneficiaries.ToArray()
                    }]
                },
                OldPrice = new()
                {
                    Price = GetPrice(orderItem.OriginalSalePrice)
                }
            };
        }
        private static IEnumerable<Tax> GetDiffPriceTaxBreakup(IEnumerable<Tax> newTaxes, IEnumerable<Tax> oldTaxes)
        {
            if (newTaxes == null && oldTaxes == null)
                return Enumerable.Empty<Tax>();
            newTaxes = newTaxes ?? Enumerable.Empty<Tax>();
            oldTaxes = oldTaxes ?? Enumerable.Empty<Tax>();

            var diffTaxes = new List<Tax>();
            var remainingOldTaxes = oldTaxes.ToList();

            foreach (var tax in newTaxes)
            {
                var oldTax = remainingOldTaxes.FirstOrDefault(o => o.Code == tax.Code);

                diffTaxes.Add(new Tax
                {
                    Code = tax.Code,
                    Amount = new CurrencyAmount
                    {
                        BaseCurrency = tax.Amount.BaseCurrency,
                        MonetaryAmount = (Convert.ToDecimal(tax.Amount.MonetaryAmount)
                                          - Convert.ToDecimal(oldTax?.Amount.MonetaryAmount ?? "0"))
                                          .ToString()
                    },
                    Name = tax?.Name,
                    CountryCode = tax?.CountryCode
                });

                if (oldTax != null)
                    remainingOldTaxes.Remove(oldTax);
            }

            foreach (var oldTax in remainingOldTaxes)
            {
                diffTaxes.Add(new Tax
                {
                    Code = oldTax.Code,
                    Amount = new CurrencyAmount
                    {
                        BaseCurrency = oldTax.Amount.BaseCurrency,
                        MonetaryAmount = (-Convert.ToDecimal(oldTax.Amount.MonetaryAmount)).ToString()
                    },
                    Name = oldTax?.Name,
                    CountryCode = oldTax?.CountryCode
                });
            }

            return diffTaxes;
        }

        private static DiffPriceAndFareDetailsType GetDifferencePrice(OrderItems orderItem, string[] custID, [Optional] IEnumerable<Penalty> penalties, bool isOrderChange, bool isOrderCancel, bool isChangeReshopHoldBooking, bool isReshopDisruption, bool isCancelledHoldBooking = false)
        {
            var differentialPriceType = new DifferentialPriceType();
            var newTaxes = orderItem.PriceDetails.FirstOrDefault(p => p.PriceComponentType == PriceComponentTypes.TotalTaxEnum)?.TaxBreakup;
            var oldTaxes = isCancelledHoldBooking? newTaxes : orderItem.OriginalSalePrice.FirstOrDefault(p => p.PriceComponentType == PriceComponentTypes.TotalTaxEnum)?.TaxBreakup;
            var diffTaxes = GetDiffPriceTaxBreakup(newTaxes, oldTaxes);
            foreach (var x in orderItem.SalePriceDifference)
            {
                switch (x.PriceComponentType)
                {
                    case PriceComponentTypes.TotalTaxEnum:
                        {
                            differentialPriceType.TaxSummary = new TaxSummaryType[1]
                            {
                            new TaxSummaryType()
                            {
                            Tax = GetTaxType(diffTaxes, isReshopDisruption),
                            TotalTaxAmount = MapAmountToCurrencyAmount(x.Amount, isReshopDisruption)
                            }
                        };
                            break;
                        }
                    case PriceComponentTypes.BaseFareEnum:
                        {
                            differentialPriceType.BaseAmount = MapAmountToCurrencyAmount(x.Amount, isReshopDisruption);
                            break;
                        }
                }
            }

            differentialPriceType.TotalAmount = new AmountType()
            {
                Value = differentialPriceType.TaxSummary.First().TotalTaxAmount.Value + differentialPriceType.BaseAmount?.Value ?? 0,
                CurCode = differentialPriceType.BaseAmount?.CurCode
            };

            if (isReshopDisruption)
            {
                differentialPriceType.DueByAirlineAmount = new AmountType
                {
                    CurCode = differentialPriceType.BaseAmount?.CurCode,
                    Value = 0.00m
                };
                differentialPriceType.DueToAirlineAmount = new AmountType
                {
                    CurCode = differentialPriceType.BaseAmount?.CurCode,
                    Value = 0.00m
                };
            }
            else if (isChangeReshopHoldBooking)
            {

                differentialPriceType.DueByAirlineAmount = new AmountType()
                {
                    CurCode = differentialPriceType.BaseAmount?.CurCode,
                    Value = 0.00m
                };
                differentialPriceType.DueToAirlineAmount = new AmountType()
                {
                    CurCode = differentialPriceType.BaseAmount?.CurCode,
                    Value = Convert.ToDecimal(orderItem.PriceDetails?.FirstOrDefault(s => s.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum)?.Amount?.MonetaryAmount ?? "0")
                };

            }
            else
            {
                differentialPriceType.DueByAirlineAmount = new AmountType()
                {
                    CurCode = differentialPriceType.BaseAmount?.CurCode,
                    Value = orderItem.RefundPrice != null ? Math.Abs(Convert.ToDecimal(orderItem.RefundPrice.First().Amount.MonetaryAmount)) : 0.00m
                };
                differentialPriceType.DueToAirlineAmount = new AmountType()
                {
                    CurCode = differentialPriceType.BaseAmount?.CurCode,
                    Value = orderItem.AddCollectPrice != null ? Convert.ToDecimal(orderItem.AddCollectPrice?.Amount.MonetaryAmount) : 0.00m

                };
            }

            if (!isReshopDisruption && Convert.ToDecimal(orderItem.PriceDetails?.FirstOrDefault(s => s.PriceComponentType == PriceComponentTypes.BaseFareEnum)?.Amount?.MonetaryAmount) == 0 && Convert.ToDecimal(orderItem.SalePriceDifference?.FirstOrDefault(s => s.PriceComponentType == PriceComponentTypes.BaseFareEnum)?.Amount?.MonetaryAmount) != 0)
            {
                var refundPrice = orderItem.RefundPrice != null ? Math.Abs(Convert.ToDecimal(orderItem.RefundPrice.First().Amount.MonetaryAmount)) : 0.00m;
                differentialPriceType.DueByAirlineAmount = new AmountType()
                {
                    CurCode = orderItem.SalePriceDifference.FirstOrDefault()?.Amount.BaseCurrency,
                    Value = Math.Abs(refundPrice)
                };
            }

            return new DiffPriceAndFareDetailsType { Price = differentialPriceType };
        }

        private static DifferentialFareDetailType[] GetFareDetail(OrderItems orderItem, string[] custID, bool isOrderChange)
        {
            List<DifferentialFareDetailType> differentialFareDetailTypeList = new();
            DifferentialFareDetailType differentialFareDetailType = new();
            differentialFareDetailType.PaxRefID = custID;

            if (isOrderChange)
            {
                differentialFareDetailType.PenaltyRefID = orderItem.Services.First().PenaltyRef?.ChangePenaltyRefIds?.Where(x => x.EndsWith("-CHANGE")).ToArray();
            }
            else
            {
                differentialFareDetailType.PenaltyRefID = orderItem.Services.First().PenaltyRef?.CancellationPenaltyRefIds?.ToArray();
            }

            if (differentialFareDetailType.PenaltyRefID.IsNullOrEmpty())
                return differentialFareDetailTypeList.ToArray();

            differentialFareDetailTypeList.Add(differentialFareDetailType);
            return differentialFareDetailTypeList.ToArray();
        }

        private static FareDetailType[] GetFareDetails(OrderDocument orderDocument, IEnumerable<OrderService> services, DataList dataList, string basePriceCurrency, bool isOrderChange)
        {
            return new FareDetailType[]
            {
                new FareDetailType
                {
                    PaxRefID = services?.FirstOrDefault()?.ServiceBeneficiaries.ToArray(),
                    FareComponent = services?.All(x => x.PenaltyRef != null) == true ? GetFareComponenet(orderDocument, dataList, services, basePriceCurrency, isOrderChange) : Array.Empty<FareComponentType>()
                }
            };
        }

        private static FareComponentType[] GetFareComponenet(OrderDocument orderDocument, DataList dataList, IEnumerable<OrderService> services, string basePriceCurrency, bool isOrderChange)
        {
            return dataList.OriginDestinationAssociations.Select(ond =>
            {
                var segments = dataList.Journey.FirstOrDefault(x => x.JourneyId == ond.JourneyRefIds.FirstOrDefault())?.FlightSegmentRefIds;
                var penaltyRef = orderDocument.OrderItems.SelectMany(x => x.Services).FirstOrDefault(x => x.SegmentRefIds.Any(seg => segments.Any(s => s.Contains(seg))))?.PenaltyRef;
                var penalties = dataList.Penalty?.Where(x => penaltyRef.CancellationPenaltyRefIds.Contains(x.Id) || penaltyRef.ChangePenaltyRefIds.Contains(x.Id)).Select(y => y);
                var priceClassId = services.FirstOrDefault(serv => serv.SegmentRefIds.FirstOrDefault() == segments.FirstOrDefault())?.PriceClassRefIds;
                var priceclass = dataList.PriceClass?.FirstOrDefault(x => x?.Id == priceClassId?.FirstOrDefault());

                if (isOrderChange)
                {
                    dataList.Penalty = dataList.Penalty.Where(x => !x.Text.Contains("Change Penalty")).ToList();
                }
                var cancelRestrictionsList = dataList.Penalty?.Where(x => penaltyRef.CancellationPenaltyRefIds.Contains(x.Id)).SelectMany(penalty => GetCancelRestrictions(penalty, basePriceCurrency));
                var changeRestrictionsList = dataList.Penalty?.Where(x => penaltyRef.ChangePenaltyRefIds != null && penaltyRef.ChangePenaltyRefIds.Contains(x.Id))?.SelectMany(penalty => GetChangeRestrictions(penalty, basePriceCurrency));


                return new FareComponentType()
                {
                    CancelRestrictions = cancelRestrictionsList?.ToArray(),
                    ChangeRestrictions = changeRestrictionsList?.ToArray(),
                    PriceClassRefID = priceclass?.Id,
                    FareBasisCode = priceclass?.FareBasisCode,
                    FareTypeCode = orderDocument.FareTypeRequest?.FareSubTypeCode,
                    NegotiatedCode = orderDocument.FareTypeRequest?.FareTypeCode,
                    PaxSegmentRefID = segments?.ToArray()
                };
            }).ToArray();
        }

        private static OrderedServiceType[] GetServices(IEnumerable<OrderService> services, OrderDocument orderDocument, bool isOrderChange, IEnumerable<Segment> segments, IEnumerable<OrderItems> orderItems, IEnumerable<ServiceDefinitionType> serviceDefinitionList, IEnumerable<BaggageAllowanceType> baggageAllowanceList, DataList dataList)
        {
            int serviceId = 0;
            var serviceList = new List<OrderedServiceType>();

            if (!services.IsNullOrEmpty())
            {
                foreach (var pax in services.SelectMany(x => x.ServiceBeneficiaries ?? Enumerable.Empty<String>()).Distinct())
                {
                    foreach (var service in services.Where(x => x.ServiceBeneficiaries.Contains(pax)))
                    {
                        //Seat Services
                        if (service.Product?.ProductType == Product.ProductTypes.SeatEnum)
                        {
                            serviceList.Add(new OrderedServiceType()
                            {
                                ServiceID = service.ServiceId,
                                BookingRef = GetBookingRef(orderDocument),
                                OrderServiceAssociation = new OrderServiceAssociationType()
                                {
                                    Item = new SeatOnLegType()
                                    {
                                        Seat = new SeatType()
                                        {
                                            RowNumber = service.CurrentSeatAssignment[..^1],
                                            ColumnID = service.CurrentSeatAssignment[^1].ToString()
                                        },
                                        SeatAssignmentAssociations = new SeatAssignmentAssociationsType()
                                        {
                                            Item = new PaxSegmentRefType()
                                            {
                                                PaxSegmentRefID = service.SegmentRefIds.First()
                                            }
                                        }
                                    }
                                },
                                ServiceRefID = serviceList.FirstOrDefault(s => s?.OrderServiceAssociation?.Item is PaxSegmentRefType paxSegment && paxSegment.PaxSegmentRefID == service.SegmentRefIds?.FirstOrDefault())?.ServiceID,
                                PaxRefID = service.ServiceBeneficiaries.First(),
                                DeliveryStatusCode = OrderItemStatusMapper.GetDeliveryStatusCode(orderItems, service, segments),
                                StatusCode = OrderItemStatusMapper.GetStatusCode(service.SegmentRefIds.FirstOrDefault(), orderItems, isOrderChange, segments)
                            });
                        }
                        else if (service.Product?.ProductType == Product.ProductTypes.FlightEnum)
                        {
                            serviceList.Add(new OrderedServiceType
                            {
                                ServiceID = $"{Constants.Application.SV}{++serviceId}",
                                BookingRef = GetBookingRef(orderDocument),
                                PaxRefID = pax,
                                OrderServiceAssociation = new OrderServiceAssociationType
                                {
                                    Item = new PaxSegmentRefType
                                    {
                                        PaxSegmentRefID = service.SegmentRefIds.First()
                                    }
                                },
                                DeliveryStatusCode = OrderItemStatusMapper.GetDeliveryStatusCode(orderItems, service, segments),
                                StatusCode = OrderItemStatusMapper.GetStatusCode(service.SegmentRefIds.FirstOrDefault(), orderItems, isOrderChange, segments),
                            });
                        }
                        else
                        {
                            serviceList.Add(new OrderedServiceType()
                            {
                                ServiceID = $"{Constants.Application.SV}{++serviceId}",
                                BookingRef = GetBookingRef(orderDocument),
                                OrderServiceAssociation = new OrderServiceAssociationType()
                                {
                                    Item = new OrderServiceDefinitionRefType()
                                    {
                                        OrderFlightAssociations = new OrderFlightAssociationsType()
                                        {
                                            Item = new PaxSegmentRefType()
                                            {
                                                PaxSegmentRefID = service.SegmentRefIds?.FirstOrDefault()
                                            }
                                        },
                                        ServiceDefinitionRefID = serviceDefinitionList.FirstOrDefault(x => x.ServiceCode == orderDocument.DataList.OrderInstructions?.FirstOrDefault(x => x.InstructionId == service.OrderInstructionRefs?.FirstOrDefault())?.ItemCode)?.ServiceDefinitionID
                                    }
                                },
                                ServiceRefID = serviceList.FirstOrDefault(s => s?.OrderServiceAssociation?.Item is PaxSegmentRefType paxSegment && paxSegment.PaxSegmentRefID == service.SegmentRefIds?.FirstOrDefault())?.ServiceID,
                                PaxRefID = service.ServiceBeneficiaries.FirstOrDefault(),
                                DeliveryStatusCode = OrderItemStatusMapper.GetDeliveryStatusCode(orderItems, service, segments),
                                StatusCode = OrderItemStatusMapper.GetStatusCode(service.SegmentRefIds.FirstOrDefault(), orderItems, isOrderChange, segments)
                            });
                        }
                    }

                }
                foreach (var pax in services.SelectMany(x => x.ServiceBeneficiaries ?? Enumerable.Empty<String>()).Distinct())
                {
                    foreach (var service in services)
                    {
                        if (service?.BaggageRefIds != null)
                        {
                            foreach (var baggageRefId in service?.BaggageRefIds)
                            {
                                var baggageInfo = dataList.BaggageAllowances.FirstOrDefault(x => x.BaggageAllowanceId == baggageRefId);

                                if (baggageInfo == null)
                                    continue;

                                var baggageId = baggageAllowanceList.Where(x => x.TypeCode == baggageInfo.BaggageAllowanceTypeCode).FirstOrDefault(x =>
                                {
                                    var dimensionAllowance = baggageInfo?.FreeBaggageAllowance?.DimensionAllowance?.FirstOrDefault(z => z.LengthUnitOfMeasurement == LengthUnitCodeType.CMT);
                                    var maximumDimensionAllowance = x?.MaximumDimensionAllowance?.FirstOrDefault(y => y.LengthUnitOfMeasurement == Domain.OrderViewRS.LengthUnitCodeType.CMT);
                                    var weightAllowance = baggageInfo?.FreeBaggageAllowance?.WeightAllowance?.FirstOrDefault(z => z.WeightUnitOfMeasurement == VAA.EAPI.Domain.Virgin.Models.Products.Baggage.WeightUnitCodeType.KGM);
                                    var maximumWeightAllowance = x?.WeightAllowance?.FirstOrDefault(y => y.WeightUnitOfMeasurement == Domain.OrderViewRS.WeightUnitCodeType.KGM);

                                    return x.TypeCode == baggageInfo?.BaggageAllowanceTypeCode
                                    && x.PieceAllowance.TotalQty > 0
                                    && x.PieceAllowance.TotalQty == baggageInfo.FreeBaggageAllowance.AllowedBagCnt
                                    && maximumDimensionAllowance?.HeightMeasure.ToString() == dimensionAllowance?.HeightMeasure
                                    && maximumDimensionAllowance?.LengthMeasure.ToString() == dimensionAllowance?.LengthMeasure
                                    && maximumDimensionAllowance?.LinearMeasure.ToString() == dimensionAllowance?.LinearMeasure
                                    && maximumDimensionAllowance?.WidthMeasure.ToString() == dimensionAllowance?.WidthMeasure
                                    && maximumWeightAllowance?.MaximumWeightMeasure.ToString() == weightAllowance?.MaximumWeightMeasure;
                                })?.BaggageAllowanceID;

                                if (baggageId != null)
                                {
                                    var baggageServiceDefId = serviceDefinitionList?.FirstOrDefault(x => ((BaggageAllowanceRefType)x.ServiceDefinitionAssociation?.Item)?.BaggageAllowanceRefID?.FirstOrDefault() == baggageId)?.ServiceDefinitionID;

                                    serviceList.Add(new OrderedServiceType
                                    {
                                        ServiceID = $"{Constants.Application.SV}{++serviceId}",
                                        BookingRef = GetBookingRef(orderDocument),
                                        PaxRefID = pax,
                                        OrderServiceAssociation = new OrderServiceAssociationType
                                        {
                                            Item = new OrderServiceDefinitionRefType
                                            {
                                                OrderFlightAssociations = new OrderFlightAssociationsType
                                                {
                                                    Item = new PaxSegmentRefType
                                                    {
                                                        PaxSegmentRefID = service.SegmentRefIds.First()
                                                    }
                                                },
                                                ServiceDefinitionRefID = baggageServiceDefId
                                            }
                                        },
                                        ServiceRefID = serviceList.FirstOrDefault(s => s?.OrderServiceAssociation?.Item is PaxSegmentRefType paxSegment && paxSegment.PaxSegmentRefID == service.SegmentRefIds?.FirstOrDefault() && s.PaxRefID == pax)?.ServiceID,
                                        DeliveryStatusCode = OrderItemStatusMapper.GetDeliveryStatusCode(orderItems, service, segments),
                                        StatusCode = OrderItemStatusMapper.GetStatusCode(service.SegmentRefIds.FirstOrDefault(), orderItems, isOrderChange, segments),
                                    });
                                }
                            }
                        }

                    }
                    foreach (var service in services)
                    {
                        if (service.Product?.ProductType == Product.ProductTypes.FlightEnum)
                        {
                            var orderDocServiceDef = dataList.ServiceDefinitions?.FirstOrDefault(a => a.ServiceDefinitionId == service.ServiceDefinitionRefIds?.FirstOrDefault());

                            var serviceDefDescription = string.Join(",",orderDocServiceDef?.Description?.OrderBy(description => description)?? Enumerable.Empty<string>());

                            var matchedItem = serviceDefinitionList.Where(a => a.Desc != null)
                                .Select(a => new
                                {
                                    a.ServiceDefinitionID,
                                    Description = string.Join(",", a.Desc.Select(d => d.DescText).OrderBy(x => x))
                                }).FirstOrDefault(x => serviceDefDescription == x.Description);

                            if (matchedItem == null) continue;
                            var serviceDefinitionRefID = matchedItem?.ServiceDefinitionID ?? string.Empty;

                            serviceList.Add(new OrderedServiceType()
                            {
                                ServiceID = $"{Constants.Application.SV}{++serviceId}",
                                BookingRef = GetBookingRef(orderDocument),
                                OrderServiceAssociation = new OrderServiceAssociationType()
                                {
                                    Item = new OrderServiceDefinitionRefType()
                                    {
                                        OrderFlightAssociations = new OrderFlightAssociationsType()
                                        {
                                            Item = new PaxSegmentRefType()
                                            {
                                                PaxSegmentRefID = service.SegmentRefIds?.FirstOrDefault()
                                            }
                                        },
                                        ServiceDefinitionRefID = serviceDefinitionRefID
                                    }
                                },
                                ServiceRefID = serviceList.FirstOrDefault(s => s?.OrderServiceAssociation?.Item is PaxSegmentRefType paxSegment && paxSegment.PaxSegmentRefID == service.SegmentRefIds?.FirstOrDefault() && s.PaxRefID == pax)?.ServiceID,
                                PaxRefID = pax,
                                DeliveryStatusCode = OrderItemStatusMapper.GetDeliveryStatusCode(orderItems, service, segments),
                                StatusCode = OrderItemStatusMapper.GetStatusCode(service.SegmentRefIds.FirstOrDefault(), orderItems, isOrderChange, segments)
                            });
                        }
                    }
                }

                serviceList.AddRange(
                    services.Where(service => service.Product?.ProductType == Product.ProductTypes.NameCorrection)
                    .Select(service => new OrderedServiceType()
                    {
                        DeliveryStatusCode = Constants.Application.DeliveryStatusCodeDelivered,
                        OrderServiceAssociation = new OrderServiceAssociationType()
                        {
                            Item = new OrderServiceDefinitionRefType()
                            {
                                ServiceDefinitionRefID = Constants.Application.NameChangeServiceDefinitionId
                            }
                        },
                        PaxRefID = service.CustomerRefIds.First(),
                        ServiceID = Constants.Application.NameChangeServiceId,
                        StatusCode = Constants.Application.DeliveryStatusCodeConfirmed
                    })
                );
            }

            return serviceList?.ToArray();
        }

        private static BookingRefType[] GetBookingRef(OrderDocument orderDocument)
        {
            return new BookingRefType[]
            {
                new BookingRefType()
                {
                    BookingID = orderDocument.BookingReference?.RLOC,
                    BookingEntity = new BookingEntityType()
                    {
                        Item = new CarrierType()
                        {
                            AirlineDesigCode = Constants.Application.OwnerAirline
                        }
                    }
                }
            };
        }

        private static PriceType GetPrice(IEnumerable<Price> priceDetails, bool isReshopDisruption = false)
        {
            if (!string.IsNullOrEmpty(priceDetails?.FirstOrDefault()?.PriceComponentType.ToString()) && priceDetails.Count() > 1)
            {
                return new PriceType()
                {
                    TotalAmount = MapAmountToCurrencyAmount(priceDetails.FirstOrDefault(x => x.PriceComponentType == PriceComponentTypes.BasePriceEnum)?.Amount, isReshopDisruption),
                    BaseAmount = MapAmountToCurrencyAmount(priceDetails.FirstOrDefault(x => x.PriceComponentType == PriceComponentTypes.BaseFareEnum)?.Amount, isReshopDisruption),
                    TaxSummary = new TaxSummaryType[]
                     {
                        new TaxSummaryType()
                        {
                            Tax = GetTaxType(priceDetails.FirstOrDefault(x => x.PriceComponentType == PriceComponentTypes.TotalTaxEnum)?.TaxBreakup, isReshopDisruption),
                            TotalTaxAmount = MapAmountToCurrencyAmount(priceDetails.FirstOrDefault(x => x.PriceComponentType == PriceComponentTypes.TotalTaxEnum)?.Amount, isReshopDisruption),
                            TotalRefundableTaxAmount = isReshopDisruption
                                ? new AmountType
                                {
                                    CurCode = priceDetails.FirstOrDefault(x => x.PriceComponentType == PriceComponentTypes.TotalTaxEnum)?.Amount?.BaseCurrency,
                                    Value = 0.00m
                                }
                                : null
                        }
                     }
                };
            }
            else
            {
                return new PriceType()
                {
                    TotalAmount = MapAmountToCurrencyAmount(priceDetails?.FirstOrDefault()?.Amount, isReshopDisruption)
                };
            }
        }

        private static TaxType[] GetTaxType(IEnumerable<Tax> taxBreakUp, bool isReshopDisruption = false)
        {
            return taxBreakUp?.Select(x => new TaxType()
            {
                Amount = MapAmountToCurrencyAmount(x?.Amount, isReshopDisruption),
                TaxCode = x?.Code,
                DescText = x?.Name
            }).ToArray();
        }

        public static AmountType MapAmountToCurrencyAmount(CurrencyAmount currencyAmount, bool isReshopDisruption = false)
        {
            var monetaryHolder = isReshopDisruption ? 0.00m : Convert.ToDecimal(currencyAmount?.MonetaryAmount);
            string correctedMonetaryAmount = monetaryHolder.ToString("#.00", System.Globalization.CultureInfo.InvariantCulture);
            return new AmountType()
            {
                CurCode = currencyAmount?.BaseCurrency,
                Value = Convert.ToDecimal(correctedMonetaryAmount)
            };
        }

        private static DataListsType GetDataList(DataList dataList, bool isOrderChange, bool isOrderCancel, OrderDocument orderDocument, bool isNameCorrection, ref IEnumerable<PaxType> paxList, ref IEnumerable<PaxSegmentType> paxSegmentList, ref IEnumerable<ContactInfoType> contactInfoList, ref IEnumerable<OriginDestType> originDestList, ref IEnumerable<PaxJourneyType> paxJourneyList, ref IEnumerable<BaggageAllowanceType> baggageAllowanceList, ref IEnumerable<DatedMarketingSegmentType> datedMarketingSegmentList, ref IEnumerable<DatedOperatingSegmentType> datedOperatingSegmentList, ref IEnumerable<DatedOperatingLegType> datedOperatingLegList, ref IEnumerable<ServiceDefinitionType> serviceDefinitionList, ref IEnumerable<PriceClassType> priceClassList, ref IEnumerable<PenaltyType> penaltyTypeList)
        {
            IEnumerable<Segment> segmentDetails = null;
            IEnumerable<BaggageAllowances> baggageAllowances = null;
            IEnumerable<ServiceDefinition> serviceDefinitions = null;

            if (isOrderChange)
            {
                var latestHistory = orderDocument.OrderHistory.OrderBy(v => v.Version).Last();
                segmentDetails = latestHistory.DataList.SegmentDetails.Where(s => s.Status.Code != "TR" && s.Status.DeliveryStatusCode != "Delivered").Concat(orderDocument.DataList.SegmentDetails);
                baggageAllowances = latestHistory.DataList.BaggageAllowances.Where(bag => bag.BaggageAllowanceId.EndsWith("OLD")).Concat(orderDocument.DataList.BaggageAllowances);
                serviceDefinitions = latestHistory.DataList.ServiceDefinitions.Where(srv => srv.ServiceDefinitionId.EndsWith("OLD")).Concat(orderDocument.DataList.ServiceDefinitions);
            }

            baggageAllowanceList = isOrderChange ? GetBaggageAllowance(baggageAllowances) : GetBaggageAllowance(dataList.BaggageAllowances);
            priceClassList = GetpriceClass(dataList.PriceClass);
            paxList = GetPax(dataList.CustomerDetails, dataList.OrderInstructions, ref contactInfoList);
            paxSegmentList = isOrderChange ? GetPaxSegment(segmentDetails, isOrderChange, ref datedOperatingSegmentList, ref datedMarketingSegmentList, ref datedOperatingLegList) : GetPaxSegment(dataList.SegmentDetails, isOrderChange, ref datedOperatingSegmentList, ref datedMarketingSegmentList, ref datedOperatingLegList);
            originDestList = GetOriginDest(dataList.OriginDestinationAssociations, dataList.Journey, segmentDetails ?? dataList.SegmentDetails);
            paxJourneyList = GetPaxJourney(dataList.Journey);
            serviceDefinitionList = isOrderChange ? GetServiceDefinition(baggageAllowances, serviceDefinitions, dataList.OrderInstructions, isNameCorrection, ref serviceDefinitionList) : GetServiceDefinition(dataList.BaggageAllowances, dataList.ServiceDefinitions, dataList.OrderInstructions, isNameCorrection, ref serviceDefinitionList);
            penaltyTypeList = (dataList.Penalty != null && dataList.Penalty.Any()) ? isOrderCancel ? GetpenaltyList(dataList.Penalty) : isOrderChange ? GetOrderChangepenaltyList(dataList.Penalty) : null : null;

            return new DataListsType
            {
                BaggageAllowanceList = baggageAllowanceList?.ToArray(),
                PriceClassList = priceClassList?.ToArray(),
                PaxList = paxList?.ToArray(),
                PaxSegmentList = paxSegmentList?.ToArray(),
                ContactInfoList = contactInfoList.Any() ? contactInfoList.ToArray() : null,
                DatedMarketingSegmentList = datedMarketingSegmentList?.ToArray(),
                DatedOperatingSegmentList = datedOperatingSegmentList?.ToArray(),
                DatedOperatingLegList = datedOperatingLegList?.ToArray(),
                OriginDestList = originDestList?.ToArray(),
                PaxJourneyList = paxJourneyList?.ToArray(),
                ServiceDefinitionList = serviceDefinitionList?.ToArray(),
                PenaltyList = penaltyTypeList?.ToArray()
            };
        }

        private static IEnumerable<ServiceDefinitionType> GetServiceDefinition(IEnumerable<BaggageAllowances> baggageAllowances, IEnumerable<ServiceDefinition> serviceDefinitions, IEnumerable<OrderInstructions> orderInstructions, bool isNameCorrection, ref IEnumerable<ServiceDefinitionType> serviceDefinitionList)
        {
            var serviceDefId = 0;
            var srvOldCount = 0;
            var cabinBaggageCount = 0;
            var checkedInBaggageCount = 0;
            var cabinOldCount = 0;
            var checkedOldCount = 0;

            var baggageDistinct = (baggageAllowances ?? Enumerable.Empty<BaggageAllowances>())
                .Where(baggage => baggage != null)
                .Select(b => new
            {
                Original = b,
                WeightKg = b.FreeBaggageAllowance?.WeightAllowance?.Select(w =>
                {
                    double weight = w.MaximumWeightMeasure.Parse<double>(0);

                    return w.WeightUnitOfMeasurement.ToString() == "LBR" ? weight * 0.453592 : weight;
                }).FirstOrDefault() ?? 0,

                DimensionCm = b.FreeBaggageAllowance?.DimensionAllowance?.Select(d =>
                {
                    double dimension = d.LinearMeasure.Parse<double>(0);

                    return d.LengthUnitOfMeasurement.ToString() == "INH" ? dimension * 2.54 : dimension;
                }).FirstOrDefault() ?? 0,
                AllowedBagCnt = b.FreeBaggageAllowance?.AllowedBagCnt

            }).DistinctBy(b => new { Weight = Math.Round(b.WeightKg, 2), Dimension = Math.Round(b.DimensionCm, 2), b.AllowedBagCnt }).Select(b => b.Original);


            serviceDefinitionList = (serviceDefinitionList ?? Enumerable.Empty<ServiceDefinitionType>()).Concat(from serviceDefinition in (serviceDefinitions ?? Enumerable.Empty<ServiceDefinition>())
                                                                 .Where(serviceDefinition => serviceDefinition != null)
                                                                 .DistinctBy(serviceDefinition => string.Join(",", (serviceDefinition.Description ?? Enumerable.Empty<string>()).OrderBy(description => description)))
                                                                 select (new ServiceDefinitionType()
                                                                 {
                                                                     ServiceDefinitionID = serviceDefinition.ServiceDefinitionId?.EndsWith("OLD") == true ? $"{Constants.Application.SRV}{++srvOldCount}OLD" : $"{Constants.Application.SRV}{++serviceDefId}",
                                                                     OwnerCode = Constants.Application.OwnerAirline,
                                                                     Name = Constants.Application.Service,
                                                                     Desc = (serviceDefinition.Description ?? Enumerable.Empty<string>()).Select(des => new DescType()
                                                                     {
                                                                         DescText = des
                                                                     }).ToArray()
                                                                 })).ToArray().Concat(
                            from baggage in baggageDistinct
                            select (new ServiceDefinitionType()
                            {
                                ServiceDefinitionID = $"{Constants.Application.SRV}" + $"{(baggage.BaggageAllowanceId?.EndsWith("OLD") == true ? ++srvOldCount : ++serviceDefId)}" + $"{(baggage.BaggageAllowanceId?.EndsWith("OLD") == true ? "OLD" : "")}",
                                OwnerCode = Constants.Application.OwnerAirline,
                                Name = baggage.BaggageAllowanceTypeCode == Constants.Application.CarryOnBaggage ? Constants.Application.CarryOnBagService : Constants.Application.CheckedBagService,
                                ServiceDefinitionAssociation = new ServiceDefinitionAssociationType()
                                {
                                    Item = new BaggageAllowanceRefType()
                                    {
                                        BaggageAllowanceRefID = new string[1]
                                        {
                                            baggage.BaggageAllowanceTypeCode == Constants.Application.CheckedBaggage ? $"BAGA{(baggage.BaggageAllowanceId?.EndsWith("OLD") == true ? ++checkedOldCount : ++checkedInBaggageCount)}{(baggage.BaggageAllowanceId?.EndsWith("OLD") == true ? "OLD" : "")}": $"CAB{(baggage.BaggageAllowanceId?.EndsWith("OLD") == true ? ++cabinOldCount : ++cabinBaggageCount)}{(baggage.BaggageAllowanceId?.EndsWith("OLD") == true ? "OLD" : "")}"
                                        }
                                    }
                                }

                            }
                    )).ToArray().Concat(isNameCorrection ? new[]
                    {
                        new ServiceDefinitionType()
                        {
                            Name = Constants.Application.NameChangeServiceDefinitionType,
                            OwnerCode = Constants.Application.OwnerAirline,
                            ServiceDefinitionID = Constants.Application.NameChangeServiceDefinitionId
                        }
                    } : Enumerable.Empty<ServiceDefinitionType>());

            //Include Services definitions related to Flight Service,if any.
            var flightServices = orderInstructions?.Where(x => x.ItemText != null && x.ItemText.Contains(Constants.Application.FlightService));
            if (flightServices?.Any() ?? false)
            {
                return serviceDefinitionList.Concat(
                    from fltSrv in flightServices.DistinctBy(x => new { x.ItemCode })
                    let ssrInfo = fltSrv.ItemText.Split('/')
                    select new ServiceDefinitionType
                    {
                        ServiceDefinitionID = $"{Constants.Application.SRV}{++serviceDefId}",
                        OwnerCode = Constants.Application.OwnerAirline,
                        Name = ssrInfo[4],
                        ServiceCode = fltSrv.ItemCode
                    }
                ).ToArray();
            }

            return serviceDefinitionList;
        }

        private static IEnumerable<PaxJourneyType> GetPaxJourney(IEnumerable<Journey> journeys)
        {
            return journeys?.Select(journey => new PaxJourneyType()
            {
                Duration = GetDuration(TimeSpan.Parse(journey?.Duration)),
                PaxSegmentRefID = journey?.FlightSegmentRefIds.ToArray(),
                PaxJourneyID = journey?.JourneyId
            });
        }

        private static IEnumerable<OriginDestType> GetOriginDest(IEnumerable<OriginDestinationAssociation> originDestinationAssociations, IEnumerable<Journey> journeys, IEnumerable<Segment> segments)
        {
            var journeyById = (journeys ?? Enumerable.Empty<Journey>())
                .Where(journey => !journey.JourneyId.IsNullOrEmpty())
                .GroupBy(journey => journey.JourneyId)
                .ToDictionary(group => group.Key, group => group.First());

            var segmentById = (segments ?? Enumerable.Empty<Segment>())
                .Where(segment => !segment.SegmentId.IsNullOrEmpty())
                .GroupBy(segment => segment.SegmentId)
                .ToDictionary(group => group.Key, group => group.First());

            return originDestinationAssociations?.Select(od =>
            {
                var journey = od.JourneyRefIds?
                    .Select(journeyId => !journeyId.IsNullOrEmpty() && journeyById.TryGetValue(journeyId, out var matchingJourney) ? matchingJourney : null)
                    .FirstOrDefault(matchingJourney => matchingJourney != null);

                var journeySegments = journey?.FlightSegmentRefIds?
                    .Select(segmentId => !segmentId.IsNullOrEmpty() && segmentById.TryGetValue(segmentId, out var matchingSegment) ? matchingSegment : null)
                    .Where(segment => segment != null)
                    .ToArray();

                return new OriginDestType()
                {
                    DestCode = journeySegments?.LastOrDefault()?.DestinationAirportCode ?? od.OriginDestination?.DestinationAirport,
                    OriginCode = journeySegments?.FirstOrDefault()?.OriginAirportCode ?? od?.OriginDestination?.OriginAirport,
                    OriginDestID = od?.OdLabel,
                    PaxJourneyRefID = od?.JourneyRefIds?.ToArray(),
                };
            });
        }

        private static IEnumerable<PaxSegmentType> GetPaxSegment(IEnumerable<Segment> segmentDetails, bool isOrderChange, ref IEnumerable<DatedOperatingSegmentType> datedOperatingSegmentList, ref IEnumerable<DatedMarketingSegmentType> datedMarketingSegmentList, ref IEnumerable<DatedOperatingLegType> datedOperatingLegList)
        {
            List<PaxSegmentType> paxSegmentList = new List<PaxSegmentType>();
            int index = 0;
            foreach (var seg in segmentDetails)
            {
                var paxSegment = new PaxSegmentType
                {
                    PaxSegmentID = isOrderChange ? seg.SegmentId.EndsWith("OLD") ? seg.SegmentId : seg.SegmentId : $"SEG{++index}",
                    CabinTypeAssociationChoice = new CabinTypeAssociationChoiceType
                    {
                        Item = new CabinTypeType
                        {
                            CabinTypeCode = seg.CabinType?.CabinCode,
                            CabinTypeName = seg.CabinType?.CabinName
                        }
                    },
                    MarketingCarrierRBD_Code = seg.ClassOfService,
                    DatedMarketingSegmentRefId = GetDatedMarketingSegmentRefId(seg, index, isOrderChange, ref datedOperatingSegmentList, ref datedMarketingSegmentList, ref datedOperatingLegList)
                };
                paxSegmentList.Add(paxSegment);
            }
            return paxSegmentList.AsEnumerable();
        }

        private static string GetDatedMarketingSegmentRefId(Segment seg, int index, bool isOrderChange, ref IEnumerable<DatedOperatingSegmentType> datedOperatingSegmentList, ref IEnumerable<DatedMarketingSegmentType> datedMarketingSegmentList, ref IEnumerable<DatedOperatingLegType> datedOperatingLegList)
        {
            var dmsId = isOrderChange ? seg.SegmentId.EndsWith("OLD") ? $"DMS{seg.SegmentId.Trim('S', 'E', 'G')}" : $"DMS{seg.SegmentId.Trim('S', 'E', 'G')}" : $"DMS{index}";

            datedMarketingSegmentList = datedMarketingSegmentList.Append(new DatedMarketingSegmentType
            {
                DatedMarketingSegmentId = dmsId,
                DatedOperatingSegmentRefId = GetDatedOperatingSegmentRefId(seg, index, isOrderChange, ref datedOperatingLegList, ref datedOperatingSegmentList),
                Dep = new TransportPointType()
                {
                    IATA_LocationCode = seg.OriginAirportCode,
                    AircraftScheduledDateTime = Convert.ToDateTime(seg.ScheduledDepartureTime.LocalDateTime),
                    AircraftScheduledDateTimeSpecified = true
                },
                Arrival = new TransportPointType()
                {
                    IATA_LocationCode = seg.DestinationAirportCode,
                    AircraftScheduledDateTime = Convert.ToDateTime(seg.ScheduledArrivalTime.LocalDateTime),
                    AircraftScheduledDateTimeSpecified = true
                },
                CarrierDesigCode = seg.MarketingCarrier,
                CarrierName = GetCarrierName(seg.MarketingCarrier),
                MarketingCarrierFlightNumberText = seg.MarketingFlightNumber
            });

            return dmsId;
        }

        private static string GetCarrierName(string code)
        {
            return code switch
            {
                "VS" => "Virgin Atlantic Airways",
                "DL" => "Delta Air Lines",
                "CA" => "Air China",
                "SQ" => "Singapore Airlines",
                "NZ" => "Air New Zealand",
                "VA" => "Virgin Australia",
                "AM" => "Aeromexico",
                "AF" => "Air France",
                "KL" => "KLM",
                "WS" => "WestJet",
                _ => default
            };
        }

        private static string GetDatedOperatingSegmentRefId(Segment seg, int index, bool isOrderChange, ref IEnumerable<DatedOperatingLegType> datedOperatingLegList, ref IEnumerable<DatedOperatingSegmentType> datedOperatingSegmentList)
        {
            var dosId = isOrderChange ? seg.SegmentId.EndsWith("OLD") ? $"DOS{seg.SegmentId.Trim('S', 'E', 'G')}" : $"DOS{seg.SegmentId.Trim('S', 'E', 'G')}" : $"DOS{index}";

            List<string> datedOperatingLegRefIDs = new List<string>();
            if (seg.Legs != null)
            {
                foreach (var leg in seg.Legs)
                {
                    var legRefID = GetDatedOperatingLegRefID(leg, seg?.AirCraft, index, isOrderChange, seg.SegmentId, ref datedOperatingLegList);
                    datedOperatingLegRefIDs.Add(legRefID);
                }
            }
            datedOperatingSegmentList = datedOperatingSegmentList.Append(new DatedOperatingSegmentType()
            {
                DatedOperatingSegmentId = dosId,
                CarrierDesigCode = seg.OperatingCarrier ?? seg.MarketingCarrier,
                CarrierName = seg.OperatingCarrier == null ? GetCarrierName(seg.MarketingCarrier) : GetCarrierName(seg.OperatingCarrier),
                OperatingCarrierFlightNumberText = seg.OperatingFlightNumber ?? seg.MarketingFlightNumber,
                Duration = GetDuration(TimeSpan.Parse(seg.FlightDuration)),
                DatedOperatingLegRefID = datedOperatingLegRefIDs.ToArray()
            });

            return dosId;
        }

        private static string GetDatedOperatingLegRefID(Legs legs, string equipmentCode, int index, bool isOrderChange, string segId, ref IEnumerable<DatedOperatingLegType> datedOperatingLegList)
        {
            var dolId = isOrderChange ? segId.EndsWith("OLD") ? $"DOL{segId.Trim('S', 'E', 'G')}" : $"DOL{segId.Trim('S', 'E', 'G')}" : $"DOL{index}";

            datedOperatingLegList = datedOperatingLegList.Append(new DatedOperatingLegType
            {
                DatedOperatingLegID = dolId,
                Arrival = new TransportPointType()
                {
                    IATA_LocationCode = legs.DestinationAirportCode,
                    AircraftScheduledDateTime = Convert.ToDateTime(legs.ScheduledArrivalTime.LocalDateTime),
                    AircraftScheduledDateTimeSpecified = true
                },
                Dep = new TransportPointType()
                {
                    IATA_LocationCode = legs.OriginAirportCode,
                    AircraftScheduledDateTime = Convert.ToDateTime(legs.ScheduledDepartureTime.LocalDateTime),
                    AircraftScheduledDateTimeSpecified = true
                },
                CarrierAircraftType = new CarrierAircraftTypeType
                {
                    CarrierAircraftTypeCode = legs.CarrierAircraftTypeCode,
                    CarrierAircraftTypeName = legs.CarrierAircraftTypeName
                },
                OnGroundDuration = String.IsNullOrEmpty(legs.StopOverDuration) ? null : GetDuration(legs.StopOverDuration.Contains(":") ? TimeSpan.Parse(legs?.StopOverDuration) : TimeSpan.FromMinutes(int.Parse(legs?.StopOverDuration))),
                IATA_AircraftType = string.IsNullOrEmpty(equipmentCode) ? null : new IATA_AircraftTypeType() { IATA_AircraftTypeCode = equipmentCode },
            });

            return dolId;
        }

        public static string GetDuration(TimeSpan timeSpan)
        {
            return string.Format("{0}{1}{2}{3}{4}{5}{6}", Constants.Application.PT, timeSpan.Days * 24 + timeSpan.Hours, Constants.Application.H, timeSpan.Minutes, Constants.Application.M, timeSpan.Seconds, Constants.Application.S);
        }

        private static IEnumerable<PaxType> GetPax(IEnumerable<Customer> customerDetails, IEnumerable<OrderInstructions> orderInstructions, ref IEnumerable<ContactInfoType> contactInfoList)
        {
            bool isGSTDetailsPresent = false;

            if (!orderInstructions.IsNullOrEmpty())
                isGSTDetailsPresent = orderInstructions.Any(x => x.ItemCode == Constants.Application.SSRGSTN) && orderInstructions.Any(x => x.ItemCode == Constants.Application.SSRGSTA) && orderInstructions.Any(x => x.ItemCode == Constants.Application.SSRGSTA) && orderInstructions.Any(x => x.ItemCode == Constants.Application.SSRGSTE);

            List<PaxType> paxList = new List<PaxType>();
            int index = 0;

            foreach (var cust in customerDetails)
            {
                paxList.Add(new PaxType()
                {
                    ContactInfoRefID = GetContactInfoRefId(cust, index, ref contactInfoList),
                    Individual = new IndividualType()
                    {
                        Birthdate = Convert.ToDateTime(cust.DateOfBirth),
                        BirthdateSpecified = !String.IsNullOrEmpty(cust.DateOfBirth),
                        GenderCode = GetGenderCode(cust.Gender?.Split("I").First()),
                        GenderCodeSpecified = cust.Gender != null,
                        GivenName = new string[] { cust.FirstName },
                        Surname = cust.LastName
                    },
                    Employer = cust?.Employer != null ? GetEmployerDetails(cust?.Employer) : null,
                    IdentityDoc = (cust.TravelDocuments != null) ? GetIdentityDocuments(cust) : null,
                    RedressCase = (cust.TravelDocuments != null) ? GetRedressInfo(cust.TravelDocuments) : null,
                    PaxID = cust.CustId,
                    PTC = cust.Ptc,
                    PaxRefID = null,
                    LoyaltyProgramAccount = cust.LoyaltyMemberDetails != null ? GetLoyaltyProgramAccounts(cust.LoyaltyMemberDetails) : null
                });
                index++;
            }

            var infPax = paxList.Where(x => x.PTC == Constants.Application.InfantPTCCode);

            if (infPax.Any())
            {
                var adtPax = paxList.Where(x => x.PTC == Constants.Application.AdultPTCCode);

                for (int i = 0; i < infPax.Count(); i++)
                {
                    adtPax.ElementAt(i).PaxRefID = infPax.ElementAt(i).PaxID;
                }
            }

            return paxList;
        }

        private static LoyaltyProgramAccountType[] GetLoyaltyProgramAccounts(LoyaltyMemberDetails loyaltyMemberDetails)
        {
            return [new LoyaltyProgramAccountType() {
                AccountNumber = loyaltyMemberDetails.MemberNumber,
                LoyaltyProgram = new()
                {
                    ProgramCode = loyaltyMemberDetails.ProgramCode,
                    ProgramName = GetProgramName(loyaltyMemberDetails.ProgramCode)
                }
            }];
        }

        private static string GetProgramName(string code)
        {
            return code switch
            {
                "VS" => "Virgin Atlantic / Flying Club",
                "DL" => "Delta Air Lines / SkyMiles",
                "VA" => "Virgin Australia / Velocity",
                "VX" => "Virgin America / Elevate",
                "CA" => "Air China / PhoenixMiles",
                "NZ" => "Air New Zealand / AirPoints",
                "NH" => "All Nippon Airways / ANA Mileage Club",
                "HA" => "Hawaiian Airlines / Hawaiian Miles",
                "MH" => "Malaysia Airlines / Enrich",
                "SQ" => "Singapore Airlines / Kris Flyer ",
                "SA" => "South African Airways / Voyager",
                "KL" => "KLM / Flying Blue",
                "AF" => "Air France / Flying Blue",
                _ => default
            };
        }

        private static EmployerType GetEmployerDetails(Employer employer)
        {
            return new EmployerType()
            {
                Name = employer?.Name,
                TaxDeductedatSourceCapabilityInd = employer.TaxDeductedAtSourceCapabilityInd,
                SalesTaxRegistration = new List<SalesTaxRegistrationType>()
                {
                    new SalesTaxRegistrationType()
                    {
                        SalesTaxRegistrationID = employer.SalesTaxRegistration?.SalesTaxRegistrationId,
                        CityName =  employer.SalesTaxRegistration.Address?.City,
                        CountryCode = employer.SalesTaxRegistration.Address?.Country,
                        PostalCode = employer.SalesTaxRegistration.Address?.PostalCode,
                        StreetText = new string[]{
                            employer.SalesTaxRegistration.Address?.AddressLine1,
                            employer.SalesTaxRegistration.Address?.AddressLine2
                        },
                        EmailAddressText = employer.SalesTaxRegistration.Contact?.Email,
                        PhoneNumber = employer.SalesTaxRegistration.Contact.Phone is null ? null :employer.SalesTaxRegistration.Contact.Phone.Contains("/") ? new string[]
                        {
                            employer.SalesTaxRegistration.Contact?.Phone.Split('/').FirstOrDefault() ,
                            employer.SalesTaxRegistration.Contact?.Phone.Split('/').LastOrDefault()
                        } :  new string[] { employer.SalesTaxRegistration.Contact?.Phone }
                    }
                }.ToArray()
            };
        }

        private static RedressCaseType[] GetRedressInfo(IEnumerable<TravelDocument> travelDocs)
        {
            return travelDocs.Where(x => x.DocumentType.Equals("Redress") || x.DocumentType.Equals(Constants.Application.AEA))?.Select(travelDoc =>
                 new RedressCaseType
                 {
                     CountryCode = travelDoc?.CountryOfIssue,
                     RedressCaseID = travelDoc?.DocumentNumber
                 }
            ).ToArray();
        }

        private static IdentityDocType[] GetIdentityDocuments(Customer customer)
        {
            var travelDocs = customer.TravelDocuments.Where(x => x.DocumentType is not null);
            var noTravelDocs = customer.TravelDocuments.Where(x => x.DocumentType is null);

            return (travelDocs.Where(x => x.DocumentType.Is(Constants.Application.passportDocumentType) || x.DocumentType.Is(Constants.Application.greenCardDocumentType) || x.DocumentType.Is(Constants.Application.PT) || x.DocumentType.Is(Constants.Application.GC))?
                 .Select(travelDocument =>
                 new IdentityDocType
                 {
                     Birthdate = Convert.ToDateTime(travelDocument.DateOfBirth),
                     BirthdateSpecified = travelDocument.DateOfBirth != null,
                     GenderCode = GetGenderCode(travelDocument.Gender),
                     GenderCodeSpecified = travelDocument.Gender != null,
                     CitizenshipCountryCode = travelDocument.CountryOfResidence,
                     ExpiryDate = Convert.ToDateTime(travelDocument.ExpirationDate),
                     ExpiryDateSpecified = travelDocument.ExpirationDate != null,
                     GivenName = new string[] { travelDocument.PersonName?.FirstName },
                     IdentityDocID = travelDocument.DocumentNumber,
                     IdentityDocTypeCode = (travelDocument.DocumentType.Is(Constants.Application.passportDocumentType) || travelDocument.DocumentType.Is(Constants.Application.PT)) ? Constants.Application.PT : Constants.Application.GC,
                     IssueDate = Convert.ToDateTime(travelDocument.IssueDate),
                     IssueDateSpecified = (travelDocument.IssueDate != null),
                     IssuingCountryCode = travelDocument.CountryOfIssue,
                     MiddleName = new string[] { travelDocument.PersonName?.MiddleName },
                     ResidenceCountryCode = travelDocument.CountryOfResidence,
                     Surname = travelDocument.PersonName?.LastName,
                     TitleName = travelDocument.PersonName?.Title,
                     Visa = (travelDocument.DocumentType.Is(Constants.Application.passportDocumentType) || travelDocument.DocumentType.Is(Constants.Application.PT)) ? GetVisaDocuments(customer?.TravelDocuments) : null
                 }
             ))
             .Concat(travelDocs != null ? travelDocs.Where(doc => doc.DocumentType.Is(Constants.Application.knownTravellerDocumentType) || doc.DocumentType.Is(Constants.Application.CR))?
                .Select(travelDoc =>
                new IdentityDocType
                {
                    IdentityDocID = travelDoc.DocumentNumber,
                    IdentityDocTypeCode = Constants.Application.CR,
                    IssuingCountryCode = travelDoc.CountryOfIssue,
                    GivenName = new string[] { customer?.FirstName },
                    Surname = customer?.LastName,
                    TitleName = customer?.Title,
                    ExpiryDate = Convert.ToDateTime(travelDoc.ExpirationDate),
                    ExpiryDateSpecified = travelDoc.ExpirationDate != null,
                    IssueDate = Convert.ToDateTime(travelDoc.IssueDate),
                    IssueDateSpecified = (travelDoc.IssueDate != null),
                }
             ) : Enumerable.Empty<IdentityDocType>()).ToArray();
        }

        private static VisaType[] GetVisaDocuments(List<TravelDocument> travelDocuments)
        {
            if (travelDocuments.Any(x => x.DocumentType is null)) return null;

            return travelDocuments.Where(x => x.DocumentType.Is(Constants.Application.visaDocumentType) || x.DocumentType.Is(Constants.Application.VI))?.Select(travelDocument =>
                   new VisaType
                   {
                       ExpiryDate = Convert.ToDateTime(travelDocument.ExpirationDate),
                       ExpiryDateSpecified = (travelDocument.ExpirationDate != null),
                       HostCountryCode = travelDocument.CountryOfResidence,
                       IssuingCountryCode = travelDocument.CountryOfIssue,
                       IssueDate = Convert.ToDateTime(travelDocument.IssueDate),
                       IssueDateSpecified = (travelDocument.IssueDate != null),
                       VisaTypeCode = null,
                       VisaID = travelDocument.DocumentNumber
                   }).ToArray();
        }

        private static GenderCodeType GetGenderCode(string gender) =>
            gender switch
            {
                "M" => GenderCodeType.M,
                "F" => GenderCodeType.F,
                "U" => GenderCodeType.U,
                _ => GenderCodeType.X,
            };

        private static string GetContactInfoRefId(Customer customer, int index, ref IEnumerable<ContactInfoType> contactInfoList)
        {
            var hasContactDetails = customer.Contacts != null && (!customer.Contacts.Email.IsNullOrWhiteSpace() || !customer.Contacts.Phone.IsNullOrWhiteSpace());
            var contactRefused = !hasContactDetails && customer.Contacts?.ContactRefusedInd == true;
            if (customer.Ptc == Constants.Application.InfantPTCCode || (!hasContactDetails && !contactRefused))
            {
                return null;
            }

            Constants.Application.ContactTypeMap.TryGetValue(customer.Contacts.PhoneType?.FirstOrDefault() ?? 'H', out Constants.ContactType contactType);
            var contactInfoId = $"CID_{++index}";
            contactInfoList = contactInfoList.Append(new ContactInfoType()
            {
                ContactInfoID = contactInfoId,
                ContactRefusedInd = contactRefused,
                ContactRefusedIndSpecified = contactRefused,
                EmailAddress = customer.Contacts.Email.IsNullOrWhiteSpace() ? null : [
                    new EmailAddressType() {
                        EmailAddressText = customer.Contacts.Email
                }],
                Phone = customer.Contacts.Phone.IsNullOrWhiteSpace() ? null : [
                    new PhoneType() {
                        PhoneNumber = customer.Contacts.Phone,
                        CountryDialingCode = customer.Contacts.CountryDailingCode,
                        ContactTypeText = Enum.GetName(contactType)
                }],
                PostalAddress = customer.Addresses == null ? null : [new()
                {
                    CityName = customer.Addresses.City,
                    CountryCode = customer.Addresses.Country,
                    CountrySubDivisionName = customer.Addresses.Region,
                    PostalCode = customer.Addresses.PostalCode,
                    PO_BoxCode = customer.Addresses.AddressLine3,
                    CountryName = customer.Addresses.DepartureCity,
                    StreetText = [customer.Addresses.AddressLine1 , customer.Addresses.AddressLine2],
                    ContactTypeText = customer.Addresses.AddressType
                }]
            });

            return contactInfoId;
        }

        private static PriceClassType[] GetpriceClass(IEnumerable<PriceClass> priceClass)
        {
            return priceClass?.Select(pc =>
                new PriceClassType
                {
                    PriceClassID = pc.Id,
                    FareBasisCode = pc.FareBasisCode,
                    Name = pc.ProductName
                }
            ).ToArray();
        }

        private static BaggageAllowanceType[] GetBaggageAllowance(IEnumerable<BaggageAllowances> baggageAllowances)
        {
            var checkedBagCount = 0;
            var carryOnBagCount = 0;
            var carryOnOldCount = 0;
            var checkedBagOldCount = 0;

            var distinctBags = baggageAllowances?.Select(bag =>
            {
                // Normalize weights
                double weightKg = bag.FreeBaggageAllowance?.WeightAllowance?
                    .Where(w => w?.MaximumWeightMeasure != null)
                    .Select(w =>
                    {
                        double weight = 0;
                        double.TryParse(w.MaximumWeightMeasure, out weight);
                        return w.WeightUnitOfMeasurement.ToString() == "LBR" ? weight * 0.453592 : weight;
                    }).FirstOrDefault() ?? 0;

                // Normalize dimensions to cm
                double dimensionCm = bag.FreeBaggageAllowance?.DimensionAllowance?
                    .Where(d => d?.LinearMeasure != null)
                    .Select(d =>
                    {
                        double dim = 0;
                        double.TryParse(d.LinearMeasure, out dim);
                        return d.LengthUnitOfMeasurement.ToString() == "INH" ? dim * 2.54 : dim;
                    }).FirstOrDefault() ?? 0;

                return new
                {
                    Bag = bag,
                    BagType = bag.BaggageAllowanceTypeCode,
                    BagCount = bag.FreeBaggageAllowance?.AllowedBagCnt ?? 0,
                    NormalizedWeight = Math.Round(weightKg, 2),
                    NormalizedDimension = Math.Round(dimensionCm, 2)
                };
            }).GroupBy(x => new
            {
                x.BagType,
                x.BagCount,
                x.NormalizedWeight,
                x.NormalizedDimension
            }).Select(g => g.First().Bag);

            return distinctBags?.Select(bag =>
            {
                var dimensionAllowanceInCMT = bag.FreeBaggageAllowance?.DimensionAllowance?.FirstOrDefault(x => x.LengthUnitOfMeasurement == LengthUnitCodeType.CMT);
                var dimensionAllowanceInINH = bag.FreeBaggageAllowance?.DimensionAllowance?.FirstOrDefault(x => x.LengthUnitOfMeasurement == LengthUnitCodeType.INH);
                var weightAllowanceInKGM = bag.FreeBaggageAllowance?.WeightAllowance?.FirstOrDefault(x => x.WeightUnitOfMeasurement == EAPI.Domain.Virgin.Models.Products.Baggage.WeightUnitCodeType.KGM);
                var weightAllowanceInLBR = bag.FreeBaggageAllowance?.WeightAllowance?.FirstOrDefault(x => x.WeightUnitOfMeasurement == EAPI.Domain.Virgin.Models.Products.Baggage.WeightUnitCodeType.LBR);

                bool isOld = bag.BaggageAllowanceId.EndsWith("OLD");

                return new BaggageAllowanceType()
                {

                    BaggageAllowanceID = bag.BaggageAllowanceTypeCode == "Checked" ? $"BAGA{(isOld ? ++checkedBagOldCount : ++checkedBagCount)}{(isOld ? "OLD" : "")}" : $"CAB{(isOld ? ++carryOnOldCount : ++carryOnBagCount)}{(isOld ? "OLD" : "")}",
                    TypeCode = bag.BaggageAllowanceTypeCode,
                    DescText = bag.BaggageAllowanceTypeCode == "CarryOn" && bag.FreeBaggageAllowance?.BagDescriptors?.FirstOrDefault()?.BagDescriptorName.Contains(Constants.Application.CarryOnBaggageText) == true ? new[] { bag.FreeBaggageAllowance?.BagDescriptors?.FirstOrDefault()?.BagDescriptorName } : null,
                    PieceAllowance = new PieceAllowanceType()
                    {
                        TotalQty = bag.FreeBaggageAllowance?.AllowedBagCnt ?? 0
                    },
                    MaximumDimensionAllowance = bag.FreeBaggageAllowance.DimensionAllowance == null ? null : new[]
                    {
                new DimensionAllowanceType
                {
                    HeightMeasureSpecified = true,
                    HeightMeasure = Convert.ToDecimal(dimensionAllowanceInCMT?.HeightMeasure ?? "0"),
                    LengthMeasureSpecified = true,
                    LengthMeasure = Convert.ToDecimal(dimensionAllowanceInCMT?.LengthMeasure ?? "0"),
                    LinearMeasureSpecified = true,
                    LinearMeasure = Convert.ToDecimal(dimensionAllowanceInCMT?.LinearMeasure ?? "0"),
                    WidthMeasureSpecified = true,
                    WidthMeasure = Convert.ToDecimal(dimensionAllowanceInCMT?.WidthMeasure ?? "0"),
                    LengthUnitOfMeasurement = (Domain.OrderViewRS.LengthUnitCodeType)LengthUnitCodeType.CMT
                },
                new DimensionAllowanceType
                {
                    HeightMeasureSpecified = true,
                    HeightMeasure = Convert.ToDecimal(dimensionAllowanceInINH?.HeightMeasure ?? "0"),
                    LengthMeasureSpecified = true,
                    LengthMeasure = Convert.ToDecimal(dimensionAllowanceInINH?.LengthMeasure ?? "0"),
                    LinearMeasureSpecified = true,
                    LinearMeasure = Convert.ToDecimal(dimensionAllowanceInINH?.LinearMeasure ?? "0"),
                    WidthMeasureSpecified = true,
                    WidthMeasure = Convert.ToDecimal(dimensionAllowanceInINH?.WidthMeasure ?? "0"),
                    LengthUnitOfMeasurement = (Domain.OrderViewRS.LengthUnitCodeType)LengthUnitCodeType.INH
                }
            },
                    WeightAllowance = bag.FreeBaggageAllowance?.WeightAllowance == null ? null : new[]
                    {
                        new WeightAllowanceType
                           {
                             MaximumWeightMeasureSpecified = true,
                             MaximumWeightMeasure = Convert.ToDecimal(weightAllowanceInKGM?.MaximumWeightMeasure ?? "0"),
                             WeightUnitOfMeasurement = WeightUnitCodeType.KGM,
                             TotalMaximumWeightMeasureSpecified = true,
                             TotalMaximumWeightMeasure = Convert.ToDecimal((bag.FreeBaggageAllowance?.AllowedBagCnt ?? 0) * Convert.ToDecimal(weightAllowanceInKGM?.MaximumWeightMeasure ?? "0"))
                    },
                        new WeightAllowanceType
                           {
                             MaximumWeightMeasureSpecified = true,
                             MaximumWeightMeasure = Convert.ToDecimal(weightAllowanceInLBR?.MaximumWeightMeasure ?? "0"),
                             WeightUnitOfMeasurement = WeightUnitCodeType.LBR,
                             TotalMaximumWeightMeasureSpecified = true,
                             TotalMaximumWeightMeasure = Convert.ToDecimal((bag.FreeBaggageAllowance?.AllowedBagCnt ?? 0) * Convert.ToDecimal(weightAllowanceInLBR?.MaximumWeightMeasure ?? "0"))
                            }
             }
                };
            }).ToArray();
        }

        private static TicketDocInfoType[] GetTicketDoc(OrderDocument orderDocument, bool isOrderChange, bool isNameCorrection, bool isCashPayment, bool isEasyPay, bool isDisruptedOrderRetrieve, IEnumerable<OrderItems> visibleOrderItems)
        {
            var orderItems = visibleOrderItems ?? Enumerable.Empty<OrderItems>();
            var accountableDocuments = orderItems
                .Where(orderItem => orderItem?.AccountableDocumentRefs != null)
                .SelectMany(orderItem => orderItem.AccountableDocumentRefs)
                .Where(accountableDocument => accountableDocument != null);
          
            var orderCancelTicketInfo = accountableDocuments.Where(accountableDocument =>
                accountableDocument.Coupons?.Any(coupon => coupon?.CouponStatus == "VOID" || coupon?.CouponStatus == "RFND") == true);
            var ticketInfo = orderItems
                .Where(orderItem => orderItem?.AccountableDocumentRefs != null
                    && Convert.ToDecimal(GetResponsePriceDetails(orderItem, isDisruptedOrderRetrieve)?.FirstOrDefault(priceDetail => priceDetail?.PriceComponentType == PriceComponentTypes.BasePriceEnum)?.Amount?.MonetaryAmount) != 0)
                .SelectMany(orderItem => orderItem.AccountableDocumentRefs)
.Where(accountableDocument => accountableDocument?.Coupons?.Any(coupon => coupon != null && coupon.CouponStatus != "EXCH") == true);
            var countryCode = orderDocument?.PointOfSale?.CountryCode;

            if (orderCancelTicketInfo.Any())
            {
                ticketInfo = orderCancelTicketInfo;
            }

            if (ticketInfo.Any())
            {
                return ticketInfo.Select(ticketItem =>
                new TicketDocInfoType()
                {
                    BookletQty = Convert.ToDecimal(Constants.Application.NumberOfBooklets),
                    BookletQtySpecified = true,
                    PaxRefID = ticketItem.CustomerRefId,
                    ReferencedOrder = ticketItem.DocumentType == AccountableDocumentRef.DocumentTypes.YEnum ? GetReferenceOrder(ticketItem, orderDocument, visibleOrderItems) : null,
                    Ticket = [new TicketType()
                    {
                        TicketNumber = ticketItem.DocumentNumber,
                        TicketDocTypeCode = ticketItem.DocumentType == AccountableDocumentRef.DocumentTypes.TEnum ? Constants.Application.TicketTypeT : Constants.Application.DocTypePaidSeats,
                        PrimaryDocInd = ticketItem.Primary,
                        PrimaryDocIndSpecified = ticketItem.Primary,
                        ReportingTypeCode = ticketItem.DocumentType == AccountableDocumentRef.DocumentTypes.TEnum ? GetReportingTypeCode(countryCode, isCashPayment, isEasyPay) : ReportingTypeCodeType.Airline,
                        Coupon = GetCouponInfo(ticketItem, orderDocument.DataList.SegmentDetails)
                    }],
                    PaymentRefID = orderItems.FirstOrDefault(item => item?.AccountableDocumentRefs?.Any(document => document?.DocumentNumber == ticketItem.DocumentNumber) == true && item.PaymentDetails != null)?.PaymentDetails?.Where(paymentDetail => paymentDetail != null).Select(paymentDetail => paymentDetail.PaymentReferenceId)?.ToArray(),
                    IssueInfo = string.IsNullOrEmpty(ticketItem.IssueDate) ? null : GetIssueInfo(ticketItem.IssueDate),
                    OriginalIssueInfo = isOrderChange || isNameCorrection || isDisruptedOrderRetrieve ? GetOriginalIssueInfo(ticketItem) : null
                }).ToArray();
            }
            return null;
        }
        private static OrderStructureKeyType GetReferenceOrder(AccountableDocumentRef ticketItem, OrderDocument orderDocument, IEnumerable<OrderItems> visibleOrderItems)
        {
            var segmentRefId = ticketItem?.Coupons?.FirstOrDefault()?.SegmentRefId;
            var customerRefId = ticketItem?.CustomerRefId;

            var matchingOrderItem = visibleOrderItems?.FirstOrDefault(oi => oi.AccountableDocumentRefs?.Any(refDoc => refDoc.DocumentType == AccountableDocumentRef.DocumentTypes.YEnum && refDoc.CustomerRefId == customerRefId) == true && oi.Services?.Any(s => s.SegmentRefIds?.Contains(segmentRefId) == true) == true);

            if (matchingOrderItem == null) return null;

            var OrderItemId = matchingOrderItem.OrderItemId;

            return new OrderStructureKeyType
            {
                OrderID = $"VS932{orderDocument.BookingReference.RLOC}{DateTime.UtcNow:yy}",
                OwnerCode = "VS",
                OrderItem = new OrderItemKeyType[]
                        {
                             new OrderItemKeyType()
                             {
                                 OrderItemRefID = OrderItemId
                             }
                        }
            };
        }
        private static ReportingTypeCodeType GetReportingTypeCode(string countryCode, bool isCashPayment, bool isEasyPay)
        {
            if (!isCashPayment && !isEasyPay && !countryCode?.Is("US") == true)
            {
                return ReportingTypeCodeType.Airline;
            }
            else if (isCashPayment)
            {
                return countryCode?.Equals("US", StringComparison.OrdinalIgnoreCase) == true ? ReportingTypeCodeType.ARC : ReportingTypeCodeType.BSP;
            }
            else if (isEasyPay && !countryCode?.Equals("US", StringComparison.OrdinalIgnoreCase) == true)
            {
                return ReportingTypeCodeType.BSP;
            }
            else if (!isCashPayment && countryCode?.Is("US") == true)
            {
                return ReportingTypeCodeType.ARC;
            }

            return ReportingTypeCodeType.Airline;
        }

        private static OriginalTicketIssuanceInfoType GetOriginalIssueInfo(AccountableDocumentRef ticketItem)
        {
            if (ticketItem.OriginalDocumentNumber == null) return null;

            return new()
            {
                IssueDate = Convert.ToDateTime(ticketItem.OriginalIssueDate),
                IssueDateSpecified = true,
                IssuingCarrier = new() { AirlineDesigCode = Constants.Application.OwnerAirline },
                TicketNumber = ticketItem.OriginalDocumentNumber
            };
        }

        private static CouponType[] GetCouponInfo(AccountableDocumentRef accountableDoc, IEnumerable<Segment> segmentDetails)
        {
            if (accountableDoc.Coupons != null)
                return segmentDetails.SelectMany(segment => accountableDoc.Coupons.Where(coupon => coupon.SegmentRefId == segment.SegmentId).Select(coupon =>
                {
                    var couponCounter = 0;
                    return new CouponType
                    {
                        CouponNumber = Convert.ToDecimal(((accountableDoc?.AllFlown == true || accountableDoc?.AllFlown == null) && string.IsNullOrEmpty(coupon?.CouponNumber)) ? GetCouponNumberForAllFlown(ref couponCounter)
                                      : ((string.IsNullOrEmpty(coupon?.CouponNumber) && accountableDoc?.DocumentType == AccountableDocumentRef.DocumentTypes.YEnum) ? "1" : coupon.CouponNumber)),
                        FareBasisCode = coupon?.FareBasis ?? null,
                        CouponStatusCode = GetCouponStatus(coupon?.CouponStatus),
                        SoldAirlineInfo = new string[] { segment.SegmentId },
                        RFIC = accountableDoc?.ReasonForIssuance?.RFIC ?? null,
                        RFISC = accountableDoc?.ReasonForIssuance?.RFISC ?? null,
                        CouponValidityPeriod = accountableDoc.DocumentType == AccountableDocumentRef.DocumentTypes.TEnum ? GetCouponValidity(segment?.TicketNotValidBefore, segment?.TicketNotValidAfter) : null
                    };
                })).ToArray();

            return null;
        }

        private static CouponValidityPeriodType GetCouponValidity(string ticketNotValidBefore, string ticketNotValidAfter)
        {
            if (string.IsNullOrEmpty(ticketNotValidBefore) && string.IsNullOrEmpty(ticketNotValidAfter))
                return null;

            var result = new CouponValidityPeriodType();

            if (!string.IsNullOrEmpty(ticketNotValidBefore))
            {
                result.EffectiveDateSpecified = true;
                result.EffectiveDate = ticketNotValidBefore.Parse<DateTime>();
            }

            if (!string.IsNullOrEmpty(ticketNotValidAfter))
            {
                result.ExpirationDateSpecified = true;
                result.ExpirationDate = ticketNotValidAfter.Parse<DateTime>();
            }

            return result;

        }

        private static string GetCouponNumberForAllFlown(ref int couponNumber)
        {
            return couponNumber == 4 ? "1" : couponNumber++.ToString();
        }

        private static string GetCouponStatus(string couponStatus)
        {
            return couponStatus switch
            {
                Constants.Application.SearchOpenCouponStatus => Constants.Application.CouponStatusCodeOpen,
                Constants.Application.SearchFlownCouponStatus => Constants.Application.CouponStatusCodeFlownorused,
                Constants.Application.SearchUsedCouponStatus => Constants.Application.CouponStatusCodeFlownorused,
                Constants.Application.SearchVoidCouponStatus => Constants.Application.CouponStatusCodeVoid,
                Constants.Application.SearchCheckedInCouponStatus => Constants.Application.CouponStatusCodeCheckedIn,
                Constants.Application.SearchRefundCouponStatus => Constants.Application.CouponStatusCodeRefund,
                Constants.Application.SearchEMDOpenCouponStatus => Constants.Application.CouponStatusCodeOpen,
                _ => couponStatus,
            };
        }

        private static TicketIssuanceInfoType GetIssueInfo(string dateOfIssue)
        {
            return new TicketIssuanceInfoType()
            {
                IssueDateSpecified = true,
                IssueDate = Convert.ToDateTime(dateOfIssue),
                IssuingCarrier = new CarrierType()
                {
                    AirlineDesigCode = Constants.Application.OwnerAirline
                }
            };
        }

        private static List<ChangeRestrictionsType> GetChangeRestrictions(Penalty penalty, string basePriceCurrency)
        {
            var descriptionText = penalty.Text?.Split(':').Last().Trim();
            var baseCurrency = penalty.MinPrice.Amount.BaseCurrency ?? basePriceCurrency;
            var setPricision = new NumberFormatInfo()
            {
                NumberDecimalDigits = penalty.MinPrice.Amount.DecimalPrecisionCount
            };
            var zero = 0.00M;

            switch (descriptionText)
            {
                case Constants.Application.Free:
                case Constants.Application.ChangeAllowedFree:
                    return
                    [
                        new ChangeRestrictionsType
                        {
                            AllowedModificationInd = true,
                            AllowedModificationIndSpecified = true,
                            JourneyStageCode = GetJourneyStageCode(penalty.Text),
                            JourneyStageCodeSpecified = true,
                            Fee = new ChangeOrCancelFeeType
                            {
                                Amount = new AmountType { CurCode = baseCurrency, Value = zero }
                            }
                        }
                    ];
                case Constants.Application.ChangeNotAllowed:
                    return
                    [
                        new ChangeRestrictionsType
                        {
                            AllowedModificationInd = false,
                            AllowedModificationIndSpecified = true,
                            JourneyStageCode = JourneyStageTypeCodeType.PriorToDeparture,
                            JourneyStageCodeSpecified = true
                        },
                        new ChangeRestrictionsType
                        {
                            AllowedModificationInd = false,
                            AllowedModificationIndSpecified = true,
                            JourneyStageCode = JourneyStageTypeCodeType.AfterDeparture,
                            JourneyStageCodeSpecified = true
                        }
                    ];
                default:
                    return
                    [
                        new ChangeRestrictionsType
                        {
                            AllowedModificationInd = true,
                            AllowedModificationIndSpecified = true,
                            JourneyStageCode = GetJourneyStageCode(penalty.Text),
                            JourneyStageCodeSpecified = true,
                            Fee = new ChangeOrCancelFeeType
                            {
                                Amount = new AmountType
                                {
                                    CurCode = baseCurrency,
                                    Value = string.IsNullOrEmpty(penalty.MinPrice.Amount.MonetaryAmount)? zero : decimal.Parse(string.Format(setPricision, "{0:N2}", decimal.Parse(penalty.MinPrice.Amount.MonetaryAmount))),
                                }
                            }
                        }
                    ];
            }
        }

        private static List<CancelRestrictionsType> GetCancelRestrictions(Penalty penalty, string basePriceCurrency)
        {
            var description = penalty.Text?.Split(':') ?? Array.Empty<string>();
            var baseCurrency = penalty.MinPrice.Amount.BaseCurrency ?? basePriceCurrency;
            var setPricision = new NumberFormatInfo()
            {
                NumberDecimalDigits = penalty.MinPrice.Amount.DecimalPrecisionCount
            };
            var zero = 0.00M;

            string journeyStageCodeText = description.Any() ? description[0].Trim() : string.Empty;
            string descriptionText = description.Any() ? description.Last().Trim() : string.Empty;

            if (descriptionText != journeyStageCodeText)
            {
                switch (descriptionText)
                {
                    case Constants.Application.PartialRefund:
                        return
                        [
                            new CancelRestrictionsType
                            {
                                AllowedModificationInd = true,
                                AllowedModificationIndSpecified = true,
                                JourneyStageCode = GetJourneyStageCode(journeyStageCodeText),
                                JourneyStageCodeSpecified = true,
                                Fee = new ChangeOrCancelFeeType
                                {
                                    Amount = new AmountType
                                    {
                                        CurCode = baseCurrency,
                                        Value = zero
                                    }
                                }
                            }
                        ];
                    case Constants.Application.FullyRefundable:
                        return
                        [
                            new CancelRestrictionsType
                            {
                                AllowedModificationInd = true,
                                AllowedModificationIndSpecified = true,
                                JourneyStageCode = JourneyStageTypeCodeType.PriorToDeparture,
                                JourneyStageCodeSpecified = true,
                                Fee = new ChangeOrCancelFeeType
                                {
                                    Amount = new AmountType
                                    {
                                        CurCode = baseCurrency,
                                        Value = zero
                                    }
                                }
                            },
                            new CancelRestrictionsType
                            {
                                AllowedModificationInd = true,
                                AllowedModificationIndSpecified = true,
                                JourneyStageCode = JourneyStageTypeCodeType.AfterDeparture,
                                JourneyStageCodeSpecified = true,
                                Fee = new ChangeOrCancelFeeType
                                {
                                    Amount = new AmountType
                                    {
                                        CurCode = baseCurrency,
                                        Value = zero
                                    }
                                }
                            }
                        ];
                    case Constants.Application.NonRefundable:
                    default:
                        return
                        [
                            new CancelRestrictionsType
                            {
                                AllowedModificationInd = false,
                                AllowedModificationIndSpecified = true,
                                JourneyStageCode = GetJourneyStageCode(journeyStageCodeText),
                                JourneyStageCodeSpecified = true
                            }
                        ];
                }
            }
            else
            {
                switch (descriptionText)
                {
                    case Constants.Application.FullyRefundable:
                        return
                        [
                            new CancelRestrictionsType
                            {
                                AllowedModificationInd = true,
                                AllowedModificationIndSpecified = true,
                                JourneyStageCode = JourneyStageTypeCodeType.PriorToDeparture,
                                JourneyStageCodeSpecified = true,
                                Fee = new ChangeOrCancelFeeType
                                {
                                    Amount = new AmountType
                                    {
                                        CurCode = baseCurrency,
                                        Value = zero
                                    }
                                }
                            },
                            new CancelRestrictionsType
                            {
                                AllowedModificationInd = true,
                                AllowedModificationIndSpecified = true,
                                JourneyStageCode = JourneyStageTypeCodeType.AfterDeparture,
                                JourneyStageCodeSpecified = true,
                                Fee = new ChangeOrCancelFeeType
                                {
                                    Amount = new AmountType
                                    {
                                        CurCode = baseCurrency,
                                        Value = zero,
                                    }
                                }
                            }
                        ];
                    case Constants.Application.RefundFee:
                    default:
                        return
                        [
                            new CancelRestrictionsType
                            {
                                AllowedModificationInd = true,
                                AllowedModificationIndSpecified = true,
                                JourneyStageCode = JourneyStageTypeCodeType.PriorToDeparture,
                                JourneyStageCodeSpecified = true,
                                Fee = new ChangeOrCancelFeeType
                                {
                                    Amount = new AmountType
                                    {
                                        CurCode = baseCurrency,
                                        Value = string.IsNullOrEmpty(penalty.MinPrice.Amount.MonetaryAmount) ? zero : decimal.Parse(string.Format(setPricision, "{0:N2}", decimal.Parse(penalty.MinPrice.Amount.MonetaryAmount)))
                                    }
                                }
                            },
                            new CancelRestrictionsType
                            {
                                AllowedModificationInd = true,
                                AllowedModificationIndSpecified = true,
                                JourneyStageCode = JourneyStageTypeCodeType.AfterDeparture,
                                JourneyStageCodeSpecified = true,
                                Fee = new ChangeOrCancelFeeType
                                {
                                    Amount = new AmountType
                                    {
                                        CurCode = baseCurrency,
                                        Value = string.IsNullOrEmpty(penalty.MinPrice.Amount.MonetaryAmount) ? zero: decimal.Parse(string.Format(setPricision, "{0:N2}", decimal.Parse(penalty.MinPrice.Amount.MonetaryAmount)))
                                    }
                                }
                            }
                        ];
                }
            }
        }

        private static JourneyStageTypeCodeType GetJourneyStageCode(string descText)
        {
            if (descText.IndexOf(Constants.Application.AfterDeparture, StringComparison.OrdinalIgnoreCase) >= 0)
                return JourneyStageTypeCodeType.AfterDeparture;
            else
                return JourneyStageTypeCodeType.PriorToDeparture;
        }

        public static string GetOrderItemStatus(IEnumerable<OrderService> services, OrderDocument orderDocument, bool isOrderChange, IEnumerable<Segment> segments, IEnumerable<OrderItems> orderItems, IEnumerable<ServiceDefinitionType> serviceDefinitionList, IEnumerable<BaggageAllowanceType> baggageAllowanceList, DataList dataList)
        {
            int removedServices = 0;
            var serviceList = GetServices(services, orderDocument, isOrderChange, segments, orderItems, serviceDefinitionList, baggageAllowanceList, dataList);
            foreach (var service in serviceList)
            {
                if (service?.StatusCode == Constants.Application.ServiceStatusCodeTransferred)
                {
                    return Constants.Application.OrderItemStatusCodeCancel;
                }

                if (service?.DeliveryStatusCode == Constants.Application.DeliveryStatusCodeRequested || service?.DeliveryStatusCode == Constants.Application.DeliveryStatusCodeConfirmed || service?.DeliveryStatusCode == Constants.Application.DeliveryStatusCodeReadyToProceed || service?.DeliveryStatusCode == Constants.Application.DeliveryStatusCodeReadyToDeliver)
                    return Constants.Application.OrderItemStatusCodeActive;

                else if (service?.DeliveryStatusCode == Constants.Application.DeliveryStatusCodeRemoved)
                    removedServices++;

                else if (service?.DeliveryStatusCode == null) return null;
            }
            if (removedServices == serviceList.Count())
            {
                var isHoldBooking = orderItems != null && orderItems.All(oi => oi.AccountableDocumentRefs == null);
                var isOcnTriggered = orderItems != null && orderItems.Any(oi => !string.IsNullOrEmpty(oi.OCNTimestamp));
                var serviceSegmentIds = new HashSet<string>(services?.SelectMany(s => s.SegmentRefIds ?? Enumerable.Empty<string>()) ?? Enumerable.Empty<string>());
                var hasCancelledSegment = segments != null && segments.Any(s => serviceSegmentIds.Contains(s.SegmentId) && s.Status?.Code == Constants.Application.FlightStatusCodeCancelled);

                if (isHoldBooking && isOcnTriggered && hasCancelledSegment)
                    return Constants.Application.OrderItemStatusCodeActive;

                return Constants.Application.OrderItemStatusCodeCancel;
            }

            else
                return Constants.Application.OrderItemStatusCodeClosed;
        }

        private static string GetOrderStatus(OrderedOrderItemType[] orderItems, bool isOrderChange)
        {
            if (isOrderChange)
            {
                return orderItems.Where(x => x.ExistingOrderItemID != null).Last(item => item.StatusCode != null).StatusCode == Constants.Application.OrderItemStatusCodeActive ? Constants.Application.OrderItemStatusCodeOpened : Constants.Application.OrderItemStatusCodeClosed;
            }
            return orderItems.First(item => item.StatusCode != null).StatusCode == Constants.Application.OrderItemStatusCodeActive ? Constants.Application.OrderItemStatusCodeOpened : Constants.Application.OrderItemStatusCodeClosed;
        }

        private static PenaltyType[] GetpenaltyList(IEnumerable<Penalty> penalties)
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

        private static PenaltyType[] GetOrderChangepenaltyList(IEnumerable<Penalty> penalties)
        {
            var changePenalty = penalties.Where(item => item.Text.Equals("Change Penalty"));

            return changePenalty.Any() ? changePenalty.Select(p => new PenaltyType()
            {
                DescText = [p.Text],
                PenaltyID = p.Id,
                TypeCode = Enum.Parse<PenaltyTypeCodeType>(p.Type),
                Price = new()
                {
                    TotalAmount = new()
                    {
                        CurCode = p.MaxPrice.Amount.BaseCurrency,
                        Value = Convert.ToDecimal(p.MaxPrice.Amount.MonetaryAmount)
                    }
                }
            }).ToArray() : null;
        }

        private static decimal ConvertToDecimal(decimal number)
        {
            var monetaryHolder = Convert.ToDecimal(number);
            string correctedMonetaryAmount = monetaryHolder.ToString("#.00", System.Globalization.CultureInfo.InvariantCulture);

            return Convert.ToDecimal(correctedMonetaryAmount);
        }

        private static ReceiptPaymentCardType GetPaymentProcessingSummaryPaymentMethod(OrderItems orderItem, string transactionId)
        {
            var paymentDetail = orderItem.PaymentDetails.FirstOrDefault(x => x.PaymentReferenceId != null && x.PaymentReferenceId == transactionId);

            if (paymentDetail != null)
            {
                return new ReceiptPaymentCardType()
                {
                    CardBrandCode = paymentDetail?.CardMethod?.CardBrandCode,
                    MaskedCardID = paymentDetail?.CardMethod?.CardNumber,
                    ExpirationDate = paymentDetail?.CardMethod?.ExpiryDate?.Replace("/", ""),

                };
            }

            return null;
        }

    }
}
