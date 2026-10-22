using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Baggage;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.NDC.Order.Domain.OrderHistoryRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using static VAA.EAPI.Domain.Virgin.Models.Common.Price;
using LengthUnitCodeType = VAA.EAPI.Domain.Virgin.Models.Products.Baggage.LengthUnitCodeType;
using OrderItems = VAA.EAPI.Domain.Virgin.Models.Order.OrderItems;
using WeightUnitCodeType = VAA.EAPI.NDC.Order.Domain.OrderHistoryRS.WeightUnitCodeType;

namespace VAA.EAPI.NDC.Order.Mapper.Order
{
    public static class OrderHistoryResponseMapper
    {
        public static IATA_OrderHistoryRS Map(OrderDocument historyDocument, IEnumerable<EAPI.Domain.Entities.Common.NDCWarning> warnings)
        {
            // Build the response object in one place
            return new IATA_OrderHistoryRS
            {
                Items = GetOrderHistoryResponse(historyDocument, warnings)
            };
        }

        private static OrderHistoryResponseType[] GetOrderHistoryResponse(OrderDocument historyDocument, IEnumerable<EAPI.Domain.Entities.Common.NDCWarning> warnings)
        {
            return new OrderHistoryResponseType[]
            {
                new OrderHistoryResponseType()
                {
                    ChangeGroup = GetChangeOperationGroup(historyDocument, warnings),
                    OrderID = historyDocument?.OrderNumber?.OrderId
                }
            };
        }

        private static ChangeOperationGroupType[] GetChangeOperationGroup(OrderDocument historyDocument, IEnumerable<EAPI.Domain.Entities.Common.NDCWarning> warnings)
        {
            var changeGroups = new List<ChangeOperationGroupType>();
            var groupedByVersion = historyDocument.OrderHistory.GroupBy(h => h.Version);
            var index = 1;

            foreach (var group in groupedByVersion)
            {
                IEnumerable<PaxType> paxList = Enumerable.Empty<PaxType>();
                IEnumerable<PaxSegmentType> paxSegmentList = Enumerable.Empty<PaxSegmentType>();
                IEnumerable<ContactInfoType> contactInfoList = Enumerable.Empty<ContactInfoType>();
                IEnumerable<OptionalOriginDestType> originDestList = Enumerable.Empty<OptionalOriginDestType>();
                IEnumerable<PaxJourneyType> paxJourneyList = Enumerable.Empty<PaxJourneyType>();
                IEnumerable<BaggageAllowanceType> baggageAllowanceList = Enumerable.Empty<BaggageAllowanceType>();
                IEnumerable<DatedMarketingSegmentType> datedMarketingSegmentList = Enumerable.Empty<DatedMarketingSegmentType>();
                IEnumerable<DatedOperatingSegmentType> datedOperatingSegmentList = Enumerable.Empty<DatedOperatingSegmentType>();
                IEnumerable<DatedOperatingLegType> datedOperatingLegList = Enumerable.Empty<DatedOperatingLegType>();
                IEnumerable<ServiceDefinitionType> serviceDefinitionList = Enumerable.Empty<ServiceDefinitionType>();
                IEnumerable<PriceClassType> priceClassList = Enumerable.Empty<PriceClassType>();

                var splitHistories = group.Where(h => h.IsSplit).ToList();
                if (splitHistories.Count > 1)
                {
                    // Case 1: Split histories
                    var firstHistory = splitHistories.First();
                   
                    // Build PaxList from all split histories
                    var combinedPaxList = splitHistories?.SelectMany(h => GetPax(h.DataList.CustomerDetails, h.DataList.OrderInstructions, ref contactInfoList)).ToList();
                    
                    // Other lists only from the first history
                    baggageAllowanceList = GetBaggageAllowance(firstHistory.DataList.BaggageAllowances); 
                    priceClassList = GetpriceClass(firstHistory.DataList.PriceClass); 
                    paxSegmentList = GetPaxSegment(firstHistory.DataList.SegmentDetails, ref datedOperatingSegmentList, ref datedMarketingSegmentList, ref datedOperatingLegList); 
                    originDestList = GetOriginDest(firstHistory.DataList.OriginDestinationAssociations); 
                    paxJourneyList = GetPaxJourney(firstHistory.DataList.Journey); 
                    serviceDefinitionList = GetServiceDefinition(firstHistory.DataList.BaggageAllowances, firstHistory.DataList.ServiceDefinitions, firstHistory.DataList.OrderInstructions, ref serviceDefinitionList);
                    
                    var changeOperation = new ChangeOperationType
                    {
                        ChangeDateTime = Convert.ToDateTime(firstHistory.ChangeDateTime).ToUniversalTime(),
                        ChangeTypeCode = GetChangeTypeCode(firstHistory.ChangeType.Value),
                        ReasonCode = "112",
                        New = new ChangeDetailsType
                        {
                            DataLists = new OptionalDataListsType
                            {
                                BaggageAllowanceList = baggageAllowanceList?.ToArray(),
                                PriceClassList = priceClassList?.ToArray(),
                                PaxList = combinedPaxList?.ToArray(),
                                PaxSegmentList = paxSegmentList?.ToArray(),
                                ContactInfoList = firstHistory.DataList.CustomerDetails.First().Contacts != null ? contactInfoList?.ToArray() : null,
                                DatedMarketingSegmentList = datedMarketingSegmentList?.ToArray(),
                                DatedOperatingSegmentList = datedOperatingSegmentList?.ToArray(),
                                DatedOperatingLegList = datedOperatingLegList?.ToArray(),
                                OriginDestList = originDestList?.ToArray(),
                                PaxJourneyList = paxJourneyList?.ToArray(),
                                ServiceDefinitionList = serviceDefinitionList?.ToArray(),
                            },
                            Order = splitHistories.Where(h => h.IsSplit).SelectMany(h => GetOrder(h, historyDocument, ref serviceDefinitionList, ref baggageAllowanceList)).ToArray(),
                            PaymentFunctions = firstHistory.OrderItems.Any(x => x.PaymentDetails != null && x.PaymentDetails.Where(x => x.FormOfPayments != null).Select(x => x.FormOfPayments).Any(x => x == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum || x == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay)) ? GetPaymentFunction(firstHistory, historyDocument) : null,
                            TicketDocInfo = splitHistories.SelectMany(h => GetTicketDoc(h, historyDocument)).ToArray(),
                        }
                    };

                    if (int.TryParse(group.Key, out var versionNumber) && versionNumber > 1)
                    {
                        var previous = historyDocument.OrderHistory.Where(h => int.TryParse(h.Version, out var v) && v < versionNumber).OrderByDescending(h => int.Parse(h.Version)).FirstOrDefault();

                        if (previous != null)
                        {
                            changeOperation.Old = new ChangeDetailsType
                            {
                                Order = new[]
                                {
                                    new ChangesToOrderType
                                    {
                                        LastModifiedDateTime = Convert.ToDateTime(previous.ChangeDateTime).ToUniversalTime(),
                                        LastModifiedDateTimeSpecified = true,
                                         OrderID = previous.IsSplit? previous.OrderItems.First().OrderItemId.Split('-')[0]: historyDocument?.OrderNumber?.OrderId,
                                        OrderVersionNumber = (index - 1).ToString()
                                    }
                                }
                            };
                        }
                    }

                    changeGroups.Add(new ChangeOperationGroupType
                    {
                        ChangeOperation = new[] { changeOperation },
                        OrderVersionNumber = index.ToString()
                    });

                }
                else
                {
                    // Case 2: Normal histories
                    foreach (var history in group)
                    {
                        var historyWarningCodes = history?.Warnings?.Select(h => h.Code);
                        var warningsForHistory = (warnings != null && historyWarningCodes != null) ? warnings.Where(w => w != null && historyWarningCodes.Contains(w.Code)) : Enumerable.Empty<NDCWarning>();

                        var changeOperation = new ChangeOperationType
                        {
                            ChangeDateTime = Convert.ToDateTime(history.ChangeDateTime).ToUniversalTime(),
                            ChangeTypeCode = GetChangeTypeCode(history.ChangeType.Value),
                            ReasonCode = (history.ChangeType == ChangeTypeEnum.PaymentFailure || history.ChangeType == ChangeTypeEnum.SpecialServiceRequestAdded) ? "116" : history.ReasonCode ?? "112",
                            New = new ChangeDetailsType
                            {
                                DataLists = MapDataLists(history.DataList, ref paxList, ref paxSegmentList, ref contactInfoList, ref originDestList, ref paxJourneyList, ref baggageAllowanceList, ref datedMarketingSegmentList, ref datedOperatingSegmentList, ref datedOperatingLegList, ref serviceDefinitionList, ref priceClassList),
                                Order = GetOrder(history, historyDocument, ref serviceDefinitionList, ref baggageAllowanceList),
                                PaymentFunctions = GetPaymentFunction(history, historyDocument),
                                TicketDocInfo = GetTicketDoc(history, historyDocument),
                                Warning = warningsForHistory.Any() ? GetWarnings(warningsForHistory) : null

                            }
                        };

                        // Add Old section if version > 1
                        if (int.TryParse(history.Version, out var versionNumber) && versionNumber > 1)
                        {
                            var previous = historyDocument.OrderHistory.Where(h => int.TryParse(h.Version, out var v) && v < versionNumber).OrderByDescending(h => int.Parse(h.Version)).FirstOrDefault();
                            
                            if (previous != null)
                            {
                                changeOperation.Old = new ChangeDetailsType
                                {
                                    Order = new[]
                                    {
                                     new ChangesToOrderType
                                     {
                                         LastModifiedDateTime = Convert.ToDateTime(previous.ChangeDateTime).ToUniversalTime(),
                                         LastModifiedDateTimeSpecified = true,
                                         OrderID =  historyDocument?.OrderNumber?.OrderId,
                                         OrderVersionNumber = (index - 1).ToString()
                                     }
                                 }
                                };
                            }
                        }
                        changeGroups.Add(new ChangeOperationGroupType
                        {
                            ChangeOperation = new[] { changeOperation },
                            OrderVersionNumber = index.ToString()
                        });
                    }
                }
                index++;
            }

            return changeGroups.OrderByDescending(cg => int.Parse(cg.OrderVersionNumber)).ToArray();
        }

        private static TicketDocInfoType[] GetTicketDoc(OrderHistory history, OrderDocument historyDocument)
        {
            var orderCancelTicketInfo = history.OrderItems.Where(x => x.AccountableDocumentRefs != null).SelectMany(y => y.AccountableDocumentRefs).Where(coupon => coupon.Coupons.Any(x => x.CouponStatus == "VOID" || x.CouponStatus == "RFND"));
            var ticketInfo = history.OrderItems.Where(x => x.AccountableDocumentRefs != null && Convert.ToDecimal(x.PriceDetails?.FirstOrDefault(x => x.PriceComponentType == PriceComponentTypes.BasePriceEnum)?.Amount?.MonetaryAmount) != 0)?.SelectMany(y => y.AccountableDocumentRefs.Where(c => c.Coupons.Any(x => x.CouponStatus != "EXCH")));
            var countryCode = historyDocument?.PointOfSale?.CountryCode;
            bool isCashPayment = history.OrderItems.FirstOrDefault()?.PaymentDetails?.FirstOrDefault()?.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum;
            bool isEasyPay = history.OrderItems.FirstOrDefault()?.PaymentDetails?.FirstOrDefault()?.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay;

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
                    Ticket = [new TicketType()
                    {
                        TicketNumber = ticketItem.DocumentNumber,
                        TicketDocTypeCode = ticketItem.DocumentType == AccountableDocumentRef.DocumentTypes.TEnum ? Constants.Application.TicketTypeT : Constants.Application.DocTypePaidSeats,
                         ReportingTypeCode = GetReportingTypeCode(countryCode, isCashPayment, isEasyPay),
                        PrimaryDocInd = ticketItem.Primary,
                        PrimaryDocIndSpecified = ticketItem.Primary,
                        Coupon = GetCouponInfo(ticketItem, history.DataList.SegmentDetails)
                    }],
                    PaymentRefID = history.OrderItems.FirstOrDefault(item => item.AccountableDocumentRefs != null && item.AccountableDocumentRefs.Any(val => val.DocumentNumber == ticketItem.DocumentNumber) && item.PaymentDetails != null)?.PaymentDetails?.Select(value => value.PaymentReferenceId)?.ToArray(),
                    IssueInfo = string.IsNullOrEmpty(ticketItem.IssueDate) ? null : GetIssueInfo(ticketItem.IssueDate)
                }).ToArray();
            }
            return null;
        }

        private static ReportingTypeCodeType GetReportingTypeCode(string countryCode, bool isCashPayment, bool isEasyPay)
        {
            if (!isCashPayment && !isEasyPay)
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

            return ReportingTypeCodeType.Airline;
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
                Constants.Application.SearchEMDOpenCouponStatus => Constants.Application.CouponStatusCodeOpen,
                _ => couponStatus,
            };
        }

        private static ChangesToOrderType[] GetOrder(OrderHistory history, OrderDocument historyDocument, ref IEnumerable<ServiceDefinitionType> serviceDefinitionList, ref IEnumerable<BaggageAllowanceType> baggageAllowanceList)
        {
            var orderItems = GetOrderItems(history, historyDocument, serviceDefinitionList, baggageAllowanceList);

            return new ChangesToOrderType[]
            {
                new ChangesToOrderType()
                {
                    OrderID = history.IsSplit? history.OrderItems.First().OrderItemId.Split('-')[0]: historyDocument?.OrderNumber?.OrderId,
                    OwnerCode = historyDocument?.OrderNumber?.OwnerAirline,
                    OrderItem = orderItems,
                    StatusCode= GetOrderStatus(orderItems),
                    TotalPrice = GetTotalPrice(history)
                }
            };
        }

        private static PriceType GetTotalPrice(OrderHistory history)
        {
            PriceType priceType = new();
            decimal totalAmount = decimal.Zero;
            string currency = string.Empty;

            foreach (var orderItem in history.OrderItems.Where(x => x.PriceDetails != null))
            {
                foreach (var p in orderItem.PriceDetails)
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

        private static ChangesToOrderItemType[] GetOrderItems(OrderHistory history, OrderDocument historyDocument, IEnumerable<ServiceDefinitionType> serviceDefinitionList, IEnumerable<BaggageAllowanceType> baggageAllowanceList)
        {
            return history.OrderItems.Select(orderItem =>
            new ChangesToOrderItemType()
            {
                OrderItemID = orderItem.OrderItemId,
                OwnerCode = orderItem.OwnerAirline,
                Price = GetPrice(orderItem.PriceDetails),
                FareDetail = orderItem.Services.Any(x => x.Product?.ProductType == Product.ProductTypes.FlightEnum) ? GetFareDetails(historyDocument, orderItem.Services, history.DataList, orderItem.PriceDetails.FirstOrDefault(x => x.PriceComponentType == PriceComponentTypes.BasePriceEnum)?.Amount?.BaseCurrency) : null,
                Service = GetServices(orderItem.Services, historyDocument, history.DataList.SegmentDetails, history.OrderItems, serviceDefinitionList, baggageAllowanceList, history.DataList),
                StatusCode = GetOrderItemStatus(orderItem.Services, historyDocument, history.DataList.SegmentDetails, history.OrderItems, serviceDefinitionList, baggageAllowanceList, history.DataList),
            }).ToArray();
        }

        public static string GetOrderItemStatus(IEnumerable<OrderService> services, OrderDocument orderDocument, IEnumerable<Segment> segments, IEnumerable<OrderItems> orderItems, IEnumerable<ServiceDefinitionType> serviceDefinitionList, IEnumerable<BaggageAllowanceType> baggageAllowanceList, DataList dataList)
        {
            int removedServices = 0;

            var serviceList = GetServices(services, orderDocument, segments, orderItems, serviceDefinitionList, baggageAllowanceList, dataList);
            foreach (var service in serviceList)
            {
                if (service?.StatusCode == Constants.Application.ServiceStatusCodeTransferred || service?.StatusCode == Constants.Application.ServiceStatusCodeCancelled)
                {
                    return Constants.Application.OrderItemStatusCodeCancel;
                }

                if (service?.StatusCode == Constants.Application.DeliveryStatusCodeRequested || service?.StatusCode == Constants.Application.DeliveryStatusCodeConfirmed || service?.StatusCode == Constants.Application.DeliveryStatusCodeReadyToProceed || service?.StatusCode == Constants.Application.DeliveryStatusCodeReadyToDeliver)
                    return Constants.Application.OrderItemStatusCodeActive;

                else if (service?.StatusCode == Constants.Application.DeliveryStatusCodeRemoved)
                    removedServices++;

                else if (service?.DescText == null) return null;
            }
            if (removedServices == serviceList.Count())
                return Constants.Application.OrderItemStatusCodeCancel;

            else
                return Constants.Application.OrderItemStatusCodeClosed;
        }

        private static FareDetailType[] GetFareDetails(OrderDocument historyDocument, IEnumerable<OrderService> services, DataList dataList, string basePriceCurrency)
        {
            var penaltyRef = services?.FirstOrDefault()?.PenaltyRef;

            return new FareDetailType[]
            {
                new FareDetailType
                {
                    PaxRefID = services?.FirstOrDefault()?.ServiceBeneficiaries.ToArray(),
                    FareComponent = penaltyRef is not null ? GetFareComponenet(historyDocument, penaltyRef, dataList, services, basePriceCurrency) : Array.Empty<FareComponentType>()
                }
            };
        }

        private static FareComponentType[] GetFareComponenet(OrderDocument orderDocument, PenaltyRef penaltyRef, DataList dataList, IEnumerable<OrderService> services, string basePriceCurrency)
        {
            return dataList.OriginDestinationAssociations.Select(ond =>
            {
                var segments = dataList.Journey.FirstOrDefault(x => x.JourneyId == ond.JourneyRefIds.FirstOrDefault())?.FlightSegmentRefIds;

                var penalties = dataList.Penalty?.Where(x => penaltyRef.CancellationPenaltyRefIds.Contains(x.Id) || penaltyRef.ChangePenaltyRefIds.Contains(x.Id)).Select(y => y);
                var priceClassId = services.FirstOrDefault(serv => serv.SegmentRefIds.FirstOrDefault() == segments.FirstOrDefault())?.PriceClassRefIds;
                var priceclass = dataList.PriceClass?.FirstOrDefault(x => x?.Id == priceClassId?.FirstOrDefault());

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

        private static JourneyStageTypeCodeType GetJourneyStageCode(string descText)
        {
            if (descText.IndexOf(Constants.Application.AfterDeparture, StringComparison.OrdinalIgnoreCase) >= 0)
                return JourneyStageTypeCodeType.AfterDeparture;
            else
                return JourneyStageTypeCodeType.PriorToDeparture;
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

        private static PriceType GetPrice(IEnumerable<Price> priceDetails)
        {
            if (!string.IsNullOrEmpty(priceDetails?.FirstOrDefault()?.PriceComponentType.ToString()) && priceDetails.Count() > 1)
            {
                return new PriceType()
                {
                    TotalAmount = MapAmountToCurrencyAmount(priceDetails.FirstOrDefault(x => x.PriceComponentType == PriceComponentTypes.BasePriceEnum)?.Amount),
                    BaseAmount = MapAmountToCurrencyAmount(priceDetails.FirstOrDefault(x => x.PriceComponentType == PriceComponentTypes.BaseFareEnum)?.Amount),
                    TaxSummary = new TaxSummaryType[]
                     {
                        new TaxSummaryType()
                        {
                            Tax = GetTaxType(priceDetails.FirstOrDefault(x => x.PriceComponentType == PriceComponentTypes.TotalTaxEnum)?.TaxBreakup),
                            TotalTaxAmount = MapAmountToCurrencyAmount(priceDetails.FirstOrDefault(x => x.PriceComponentType == PriceComponentTypes.TotalTaxEnum)?.Amount)
                        }
                     }
                };
            }
            else
            {
                return new PriceType()
                {
                    TotalAmount = MapAmountToCurrencyAmount(priceDetails?.FirstOrDefault()?.Amount)
                };
            }
        }

        public static AmountType MapAmountToCurrencyAmount(CurrencyAmount currencyAmount)
        {
            var monetaryHolder = Convert.ToDecimal(currencyAmount?.MonetaryAmount);
            string correctedMonetaryAmount = monetaryHolder.ToString("#.00", System.Globalization.CultureInfo.InvariantCulture);
            return new AmountType()
            {
                CurCode = currencyAmount?.BaseCurrency,
                Value = Convert.ToDecimal(correctedMonetaryAmount)
            };
        }

        private static TaxType[] GetTaxType(IEnumerable<Tax> taxBreakUp)
        {
            return taxBreakUp?.Select(x => new TaxType()
            {
                Amount = MapAmountToCurrencyAmount(x?.Amount),
                TaxCode = x?.Code,
                DescText = x?.Name
            }).ToArray();
        }

        private static ChangeToServiceType[] GetServices(IEnumerable<OrderService> services, OrderDocument orderDocument, IEnumerable<Segment> segments, IEnumerable<OrderItems> orderItems, IEnumerable<ServiceDefinitionType> serviceDefinitionList, IEnumerable<BaggageAllowanceType> baggageAllowanceList, DataList dataList)
        {
            int serviceId = 0;
            var serviceList = new List<ChangeToServiceType>();

            if (!services.IsNullOrEmpty())
            {
                foreach (var pax in services.SelectMany(x => x.ServiceBeneficiaries ?? Enumerable.Empty<String>()).Distinct())
                {
                    foreach (var service in services.Where(x => x.ServiceBeneficiaries.Contains(pax)))
                    {
                        //Seat Services
                        if (service.Product?.ProductType == Product.ProductTypes.SeatEnum)
                        {
                            serviceList.Add(new ChangeToServiceType()
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
                                StatusCode = OrderItemStatusMapper.GetStatusCode(service.SegmentRefIds.FirstOrDefault(), orderItems, false, segments)
                            });
                        }
                        else if (service.Product?.ProductType == Product.ProductTypes.FlightEnum)
                        {
                            serviceList.Add(new ChangeToServiceType
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
                                StatusCode = OrderItemStatusMapper.GetStatusCode(service.SegmentRefIds.FirstOrDefault(), orderItems, false, segments),
                            });

                        }
                        else
                        {
                            serviceList.Add(new ChangeToServiceType()
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
                                        ServiceDefinitionRefID = serviceDefinitionList.FirstOrDefault(x => x.ServiceCode == dataList.OrderInstructions?.FirstOrDefault(x => x.InstructionId == service.OrderInstructionRefs?.FirstOrDefault())?.ItemCode)?.ServiceDefinitionID
                                    }
                                },
                                ServiceRefID = serviceList.FirstOrDefault(s => s?.OrderServiceAssociation?.Item is PaxSegmentRefType paxSegment && paxSegment.PaxSegmentRefID == service.SegmentRefIds?.FirstOrDefault())?.ServiceID,
                                PaxRefID = service.ServiceBeneficiaries.FirstOrDefault(),
                                StatusCode = OrderItemStatusMapper.GetStatusCode(service.SegmentRefIds.FirstOrDefault(), orderItems, false, segments)
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

                                var baggageId = baggageAllowanceList.FirstOrDefault(x =>
                                {
                                    var maximumDimensionAllowance = x.MaximumDimensionAllowance.FirstOrDefault(y => y.LengthUnitOfMeasurement == Domain.OrderHistoryRS.LengthUnitCodeType.CMT);
                                    var dimensionAllowance = baggageInfo?.FreeBaggageAllowance?.DimensionAllowance?.FirstOrDefault(z => z.LengthUnitOfMeasurement == LengthUnitCodeType.CMT);
                                    var maximumWeightAllowance = x.WeightAllowance.FirstOrDefault(y => y.WeightUnitOfMeasurement == Domain.OrderHistoryRS.WeightUnitCodeType.KGM);
                                    var weightAllowance = baggageInfo?.FreeBaggageAllowance?.WeightAllowance?.FirstOrDefault(z => z.WeightUnitOfMeasurement == VAA.EAPI.Domain.Virgin.Models.Products.Baggage.WeightUnitCodeType.KGM);
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

                                    serviceList.Add(new ChangeToServiceType
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
                                        StatusCode = OrderItemStatusMapper.GetStatusCode(service.SegmentRefIds.FirstOrDefault(), orderItems, false, segments),
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

                            var serviceDefDescription = orderDocServiceDef != null ? string.Join(",", orderDocServiceDef?.Description?.OrderBy(x => x)) : string.Empty;

                            var matchedItem = serviceDefinitionList.Where(a => a.Desc != null)
                                .Select(a => new
                                {
                                    a.ServiceDefinitionID,
                                    Description = string.Join(",", a.Desc.Select(d => d.DescText).OrderBy(x => x))
                                }).FirstOrDefault(x => serviceDefDescription == x.Description);

                            var serviceDefinitionRefID = matchedItem?.ServiceDefinitionID ?? string.Empty;

                            serviceList.Add(new ChangeToServiceType()
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
                                StatusCode = OrderItemStatusMapper.GetStatusCode(service.SegmentRefIds.FirstOrDefault(), orderItems, false, segments)
                            });
                        }
                    }
                }
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

        private static string GetOrderStatus(ChangesToOrderItemType[] orderItems)
        {
            return orderItems.First(item => item.StatusCode != null).StatusCode == Constants.Application.OrderItemStatusCodeActive ? Constants.Application.OrderItemStatusCodeOpened : Constants.Application.OrderItemStatusCodeClosed;
        }

        private static PaymentFunctionsOrderType[] GetPaymentFunction(OrderHistory history, OrderDocument orderDocument)
        {
            var transactionIds = history.OrderItems.Where(item => item.PaymentDetails != null)?.SelectMany(id => id.PaymentDetails)?.Select(x => x.PaymentReferenceId)?.Distinct()?.ToList();
            var agencyId = decimal.Parse(orderDocument.Parties.Participants.FirstOrDefault(x => x.PartyType == Infrastructure.Model.Participant.PartyTypeEnum.Seller)?.AgencyID).ToString("G29");

            var paymentFunction = transactionIds.Where(x => x != null)?.Select(item =>
            {
                var orderItems = history.OrderItems.Where(x => x.PaymentDetails != null && x.PaymentDetails.Any(x => x.PaymentReferenceId == item));
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
                               : paymentDetail.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay ? MapEasyPayPaymentSummary(agencyId) :
                             orderItems.First().PaymentDetails is not null ? GetPaymentProcessingSummaryPaymentMethod(orderItems.First()) : null,
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
                            CurCode = history.OrderItems.First().PriceDetails.First().Amount.BaseCurrency,
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

        private static ReceiptPaymentCardType GetPaymentProcessingSummaryPaymentMethod(OrderItems orderItem)
        {
            return new ReceiptPaymentCardType()
            {
                CardBrandCode = orderItem.PaymentDetails?.FirstOrDefault()?.CardMethod?.CardBrandCode,
                MaskedCardID = orderItem.PaymentDetails?.FirstOrDefault()?.CardMethod?.CardNumber,
                ExpirationDate = orderItem.PaymentDetails?.FirstOrDefault()?.CardMethod?.ExpiryDate?.Replace("/", ""),

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

        private static decimal ConvertToDecimal(decimal number)
        {
            var monetaryHolder = Convert.ToDecimal(number);
            string correctedMonetaryAmount = monetaryHolder.ToString("#.00", System.Globalization.CultureInfo.InvariantCulture);

            return Convert.ToDecimal(correctedMonetaryAmount);
        }

        private static string GetChangeTypeCode(ChangeTypeEnum changeType)
        {
            if (changeType == ChangeTypeEnum.OrderItemAdded)
                return "18";
            else if (changeType == ChangeTypeEnum.DocumentVoided)
                return "14";
            else if (changeType == ChangeTypeEnum.DocumentRefunded)
                return "15";
            else if (changeType == ChangeTypeEnum.NameCorrection)
                return "9";
            else if (changeType == ChangeTypeEnum.DocumentExchanged)
                return "16";
            else if (changeType == ChangeTypeEnum.IdentityDocumentDetailsUpdated)
                return "20";
            else if (changeType == ChangeTypeEnum.SplitPNR || changeType == ChangeTypeEnum.PaymentFailure)
                return "21";
            else if (changeType == ChangeTypeEnum.SpecialServiceRequestAdded)
                return "6";
            else
                return "10";
        }

        private static OptionalDataListsType MapDataLists(DataList dataList, ref IEnumerable<PaxType> paxList, ref IEnumerable<PaxSegmentType> paxSegmentList, ref IEnumerable<ContactInfoType> contactInfoList, ref IEnumerable<OptionalOriginDestType> originDestList, ref IEnumerable<PaxJourneyType> paxJourneyList, ref IEnumerable<BaggageAllowanceType> baggageAllowanceList, ref IEnumerable<DatedMarketingSegmentType> datedMarketingSegmentList, ref IEnumerable<DatedOperatingSegmentType> datedOperatingSegmentList, ref IEnumerable<DatedOperatingLegType> datedOperatingLegList, ref IEnumerable<ServiceDefinitionType> serviceDefinitionList, ref IEnumerable<PriceClassType> priceClassList)
        {
            baggageAllowanceList = GetBaggageAllowance(dataList.BaggageAllowances);
            priceClassList = GetpriceClass(dataList.PriceClass);
            paxList = GetPax(dataList.CustomerDetails, dataList.OrderInstructions, ref contactInfoList);
            paxSegmentList = GetPaxSegment(dataList.SegmentDetails, ref datedOperatingSegmentList, ref datedMarketingSegmentList, ref datedOperatingLegList);
            originDestList = GetOriginDest(dataList.OriginDestinationAssociations);
            paxJourneyList = GetPaxJourney(dataList.Journey);
            serviceDefinitionList = GetServiceDefinition(dataList.BaggageAllowances, dataList.ServiceDefinitions, dataList.OrderInstructions, ref serviceDefinitionList);

            return new OptionalDataListsType
            {
                BaggageAllowanceList = baggageAllowanceList?.ToArray(),
                PriceClassList = priceClassList?.ToArray(),
                PaxList = paxList?.ToArray(),
                PaxSegmentList = paxSegmentList?.ToArray(),
                ContactInfoList = dataList.CustomerDetails.First().Contacts != null ? contactInfoList?.ToArray() : null,
                DatedMarketingSegmentList = datedMarketingSegmentList?.ToArray(),
                DatedOperatingSegmentList = datedOperatingSegmentList?.ToArray(),
                DatedOperatingLegList = datedOperatingLegList?.ToArray(),
                OriginDestList = originDestList?.ToArray(),
                PaxJourneyList = paxJourneyList?.ToArray(),
                ServiceDefinitionList = serviceDefinitionList?.ToArray(),
            };
        }

        private static BaggageAllowanceType[] GetBaggageAllowance(IEnumerable<BaggageAllowances> baggageAllowances)
        {
            var checkedBagCount = 0;
            var carryOnBagCount = 0;

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

                return new BaggageAllowanceType()
                {
                    BaggageAllowanceID = bag.BaggageAllowanceTypeCode == "Checked" ? $"BAGA{++checkedBagCount}" : $"CAB{++carryOnBagCount}",
                    TypeCode = bag.BaggageAllowanceTypeCode,
                    PieceAllowance = new PieceAllowanceType()
                    {
                        TotalQty = bag.FreeBaggageAllowance?.AllowedBagCnt ?? 0
                    },
                    MaximumDimensionAllowance = new[]
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
                            LengthUnitOfMeasurement = (Domain.OrderHistoryRS.LengthUnitCodeType)LengthUnitCodeType.CMT
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
                            LengthUnitOfMeasurement = (Domain.OrderHistoryRS.LengthUnitCodeType)LengthUnitCodeType.INH
                        }
                    },
                    WeightAllowance = new[]
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
                    ContactInfoRefID = cust.Contacts != null ? GetContactInfoRefId(cust, index, ref contactInfoList) : null,
                    Individual = new IndividualType()
                    {
                        Birthdate = Convert.ToDateTime(cust.DateOfBirth),
                        BirthdateSpecified = !String.IsNullOrEmpty(cust.DateOfBirth),
                        GenderCode = GetGenderCode(cust.Gender?.Split("I").First()),
                        GenderCodeSpecified = cust.Gender != null,
                        GivenName = new string[] { cust.FirstName },
                        Surname = cust.LastName
                    },
                    IdentityDoc = (cust.TravelDocuments != null) ? GetIdentityDocuments(cust) : null,
                    RedressCase = (cust.TravelDocuments != null) ? GetRedressInfo(cust.TravelDocuments) : null,
                    PaxID = cust.CustId,
                    PTC = cust.Ptc,
                    PaxRefID = null,
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
            if (!contactInfoList.Any(x => x.EmailAddress?.FirstOrDefault()?.EmailAddressText == customer.Contacts?.Email))
            {
                Constants.Application.ContactTypeMap.TryGetValue(customer.Contacts.PhoneType?.FirstOrDefault() ?? 'H', out Constants.ContactType contactType);
                contactInfoList = contactInfoList.Append(new ContactInfoType()
                {
                    ContactInfoID = $"CID_{++index}",
                    EmailAddress = [
                        new EmailAddressType() {
                            EmailAddressText = customer.Contacts.Email
                    }],
                    Phone = [
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
            }

            return contactInfoList?.FirstOrDefault(x => x.EmailAddress?.FirstOrDefault()?.EmailAddressText == customer.Contacts?.Email).ContactInfoID;
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
        private static IEnumerable<PaxJourneyType> GetPaxJourney(IEnumerable<Journey> journeys)
        {
            return journeys?.Select(journey => new PaxJourneyType()
            {
                Duration = GetDuration(TimeSpan.Parse(journey?.Duration)),
                PaxSegmentRefID = journey?.FlightSegmentRefIds.ToArray(),
                PaxJourneyID = journey?.JourneyId
            });
        }

        private static IEnumerable<OptionalOriginDestType> GetOriginDest(IEnumerable<OriginDestinationAssociation> originDestinationAssociations)
        {
            return originDestinationAssociations?.Select((od, index) =>
            new OptionalOriginDestType()
            {
                DestCode = od.OriginDestination?.DestinationAirport,
                OriginCode = od?.OriginDestination?.OriginAirport,
                OriginDestID = od?.OdLabel,
                PaxJourneyRefID = od?.JourneyRefIds?.ToArray(),
            });
        }

        private static IEnumerable<PaxSegmentType> GetPaxSegment(IEnumerable<Segment> segmentDetails, ref IEnumerable<DatedOperatingSegmentType> datedOperatingSegmentList, ref IEnumerable<DatedMarketingSegmentType> datedMarketingSegmentList, ref IEnumerable<DatedOperatingLegType> datedOperatingLegList)
        {
            List<PaxSegmentType> paxSegmentList = new List<PaxSegmentType>();
            int index = 0;
            foreach (var seg in segmentDetails)
            {
                var paxSegment = new PaxSegmentType
                {
                    PaxSegmentID = $"SEG{++index}",
                    CabinTypeAssociationChoice = new CabinTypeAssociationChoiceType
                    {
                        Item = new CabinTypeType
                        {
                            CabinTypeCode = seg.CabinType?.CabinCode,
                            CabinTypeName = seg.CabinType?.CabinName
                        }
                    },
                    MarketingCarrierRBD_Code = seg.ClassOfService,
                    DatedMarketingSegmentRefId = GetDatedMarketingSegmentRefId(seg, index, ref datedOperatingSegmentList, ref datedMarketingSegmentList, ref datedOperatingLegList)
                };
                paxSegmentList.Add(paxSegment);
            }
            return paxSegmentList.AsEnumerable();
        }

        private static string GetDatedMarketingSegmentRefId(Segment seg, int index, ref IEnumerable<DatedOperatingSegmentType> datedOperatingSegmentList, ref IEnumerable<DatedMarketingSegmentType> datedMarketingSegmentList, ref IEnumerable<DatedOperatingLegType> datedOperatingLegList)
        {
            var dmsId = $"DMS{index}";

            datedMarketingSegmentList = datedMarketingSegmentList.Append(new DatedMarketingSegmentType
            {
                DatedMarketingSegmentId = dmsId,
                DatedOperatingSegmentRefId = GetDatedOperatingSegmentRefId(seg, index, ref datedOperatingLegList, ref datedOperatingSegmentList),
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

        private static string GetDatedOperatingSegmentRefId(Segment seg, int index, ref IEnumerable<DatedOperatingLegType> datedOperatingLegList, ref IEnumerable<DatedOperatingSegmentType> datedOperatingSegmentList)
        {
            List<string> datedOperatingLegRefIDs = new List<string>();
            if (seg.Legs != null)
            {
                foreach (var leg in seg.Legs)
                {
                    var legRefID = GetDatedOperatingLegRefID(leg, seg?.AirCraft, index, seg.SegmentId, ref datedOperatingLegList);
                    datedOperatingLegRefIDs.Add(legRefID);
                }
            }
            datedOperatingSegmentList = datedOperatingSegmentList.Append(new DatedOperatingSegmentType()
            {
                DatedOperatingSegmentId = $"DOS{index}",
                CarrierDesigCode = seg.OperatingCarrier ?? seg.MarketingCarrier,
                CarrierName = seg.OperatingCarrier == null ? GetCarrierName(seg.MarketingCarrier) : GetCarrierName(seg.OperatingCarrier),
                OperatingCarrierFlightNumberText = seg.OperatingFlightNumber ?? seg.MarketingFlightNumber,
                Duration = GetDuration(TimeSpan.Parse(seg.FlightDuration)),
                DatedOperatingLegRefID = datedOperatingLegRefIDs.ToArray()
            });

            return $"DOS{index}";
        }

        private static string GetDatedOperatingLegRefID(Legs legs, string equipmentCode, int index, string segId, ref IEnumerable<DatedOperatingLegType> datedOperatingLegList)
        {
            datedOperatingLegList = datedOperatingLegList.Append(new DatedOperatingLegType
            {
                DatedOperatingLegID = $"DOL{index}",
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

            return $"DOL{index}";
        }

        public static string GetDuration(TimeSpan timeSpan)
        {
            return string.Format("{0}{1}{2}{3}{4}{5}{6}", Constants.Application.PT, timeSpan.Days * 24 + timeSpan.Hours, Constants.Application.H, timeSpan.Minutes, Constants.Application.M, timeSpan.Seconds, Constants.Application.S);
        }

        private static IEnumerable<ServiceDefinitionType> GetServiceDefinition(IEnumerable<BaggageAllowances> baggageAllowances, IEnumerable<ServiceDefinition> serviceDefinitions, IEnumerable<OrderInstructions> orderInstructions, ref IEnumerable<ServiceDefinitionType> serviceDefinitionList)
        {
            var serviceDefId = 0;
            var cabinBaggageCount = 0;
            var checkedInBaggageCount = 0;

            var baggageDistinct = baggageAllowances.Select(b => new
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
                }).FirstOrDefault() ?? 0

            }).DistinctBy(b => new { Weight = Math.Round(b.WeightKg, 2), Dimension = Math.Round(b.DimensionCm, 2) }).Select(b => b.Original);


            serviceDefinitionList = serviceDefinitionList.Concat(from serviceDefinition in serviceDefinitions.DistinctBy(sd => string.Join(",", sd.Description.OrderBy(x => x)))
                                                                 select (new ServiceDefinitionType()
                                                                 {
                                                                     ServiceDefinitionID = $"{Constants.Application.SRV}{++serviceDefId}",
                                                                     OwnerCode = Constants.Application.OwnerAirline,
                                                                     Name = Constants.Application.Service,
                                                                     Desc = serviceDefinition.Description.Select(des => new DescType()
                                                                     {
                                                                         DescText = des
                                                                     }).ToArray()
                                                                 })).ToArray().Concat(
                            from baggage in baggageDistinct
                            select (new ServiceDefinitionType()
                            {
                                ServiceDefinitionID = $"{Constants.Application.SRV}{++serviceDefId}",
                                OwnerCode = Constants.Application.OwnerAirline,
                                Name = baggage.BaggageAllowanceTypeCode == Constants.Application.CarryOnBaggage ? Constants.Application.CarryOnBagService : Constants.Application.CheckedBagService,
                                ServiceDefinitionAssociation = new ServiceDefinitionAssociationType()
                                {
                                    Item = new BaggageAllowanceRefType()
                                    {
                                        BaggageAllowanceRefID = new string[1]
                                        {
                                            baggage.BaggageAllowanceTypeCode == Constants.Application.CheckedBaggage ? $"BAGA{++checkedInBaggageCount}" : $"CAB{++cabinBaggageCount}"
                                        }
                                    }
                                }

                            }
                    )).ToArray();

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

        private static WarningType[] GetWarnings(IEnumerable<NDCWarning> warnings)
        {
            return warnings?.Select(x => new WarningType()
            {
                Code = Convert.ToString(x.Code),
                DescText = x.ShortText,
                LangCode = Constants.Application.LangCode,
                TypeCode = x.Type,
                TagText = x.TagText,
                URL = x?.DocURL?.Href
            }).ToArray();
        }
    }
}
