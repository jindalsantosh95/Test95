using Gremlin.Net.Process.Traversal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.NDC.Order.Domain;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Model;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS;
using static VAA.EAPI.Domain.Virgin.Models.Payment.PaymentDetails;

namespace VAA.EAPI.NDC.Order.Mapper.Order
{
    public static class OrderDocumentMapper
    {
        private static readonly string[] ptcOrder = ["ADT", "GBE", "CHD", "INF"];

        public static OrderDocument UpdateOrderDocument(OrderDocument orderDoc, OrderTicketDocument issueTicketResponse, SeatFulfilmentResponse seatFulfilmentResponse, [Optional] List<TransactionIdStatus> transactionIdStatus, [Optional] IEnumerable<PriceClass> priceClass)
        {
            if (issueTicketResponse?.AccountableDocumentRef != null)
            {
                orderDoc.TimeLimits = null;

                foreach (var item in issueTicketResponse.AccountableDocumentRef)
                {
                    var orderItem = orderDoc.OrderItems.FirstOrDefault(x => x.Services.First().ServiceBeneficiaries.Any(y => y == item.CustomerRefId));


                    if (orderItem != null)
                    {
                        if (orderItem.AccountableDocumentRefs != null)

                            orderItem.AccountableDocumentRefs = orderItem.AccountableDocumentRefs.Append(item);
                        else
                            orderItem.AccountableDocumentRefs = [item];
                    }

                    if (orderItem != null && transactionIdStatus != null)
                    {
                        if (orderItem.PaymentDetails == null)
                        {
                            orderItem.PaymentDetails = new List<EAPI.Domain.Virgin.Models.Payment.PaymentDetails>() { new EAPI.Domain.Virgin.Models.Payment.PaymentDetails()
                            {
                                PaymentReferenceId = issueTicketResponse.TransactionId ,
                                Amount = new()
                                {
                                    BaseCurrency = orderItem.PriceDetails.First().Amount.BaseCurrency,
                                    MonetaryAmount = transactionIdStatus.First(item => item.TransactionId == issueTicketResponse.TransactionId).Amount.ToString()
                                }
                            }};
                            orderItem.PaymentDetails.First().PaymentProcessingStatus = transactionIdStatus.First(item => item.TransactionId == issueTicketResponse.TransactionId).PaymentStatus ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.SUCCESSFUL : EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.FAILED;
                        }
                        else
                        {
                            orderItem.PaymentDetails.First().PaymentReferenceId = issueTicketResponse.TransactionId;
                            orderItem.PaymentDetails.First().PaymentProcessingStatus = transactionIdStatus.First(item => item.TransactionId == issueTicketResponse.TransactionId).PaymentStatus ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.SUCCESSFUL : EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.FAILED;

                            orderItem.PaymentDetails.First().Amount = new()
                            {
                                BaseCurrency = orderItem.PriceDetails.First().Amount.BaseCurrency,
                                MonetaryAmount = transactionIdStatus.First(item => item.TransactionId == issueTicketResponse.TransactionId).Amount.ToString()
                            };
                        }
                    }
                }
            }

            if (issueTicketResponse?.OrderItems != null)
            {
                foreach (var oi in issueTicketResponse.OrderItems)
                {
                    oi.OrderItemId = $"{orderDoc.OrderItems.Last().OrderItemId.Split('-').First()}-{Convert.ToInt32(orderDoc.OrderItems.Last().OrderItemId.Split('-').Last()) + 1}";
                    orderDoc.OrderItems = orderDoc.OrderItems.Append(oi);
                }
            }

            if (issueTicketResponse != null)
            {
                orderDoc.DataList.OrderInstructions = issueTicketResponse.OrderInstructions;
            }

            if (issueTicketResponse?.Warnings != null)
            {
                orderDoc.Warnings = orderDoc.Warnings != null ? orderDoc.Warnings.Concat(issueTicketResponse.Warnings) : issueTicketResponse.Warnings;
            }

            if (seatFulfilmentResponse != null)
            {

                foreach (var seatOrderItem in seatFulfilmentResponse.OrderItems.OrderBy(x => Array.IndexOf(ptcOrder, orderDoc.DataList.CustomerDetails.First(c => c.CustId == x.CustomerRefId).Ptc)).ThenBy(x => x.CustomerRefId))
                {
                    orderDoc.OrderItems = orderDoc.OrderItems.Append(new EAPI.Domain.Virgin.Models.Order.OrderItems()
                    {
                        OrderItemId = $"{orderDoc.OrderItems.Last().OrderItemId.Split('-').First()}-{Convert.ToInt32(orderDoc.OrderItems.Last().OrderItemId.Split('-').Last()) + 1}",
                        OwnerAirline = Constants.Application.OwnerAirline,
                        AccountableDocumentRefs = [ new()
                    {
                        DocumentType = AccountableDocumentRef.DocumentTypes.YEnum,
                        DocumentNumber = seatOrderItem.ElectronicMiscellaneousDocumentNumber,
                        CustomerRefId = seatOrderItem.CustomerRefId,
                        Coupons = [ new() { CouponStatus = Constants.Application.SearchOpenCouponStatus, SegmentRefId = $"SEG{seatOrderItem.FlightIdRef.TrimStart('0')}" }],
                        IssueDate = DateTime.UtcNow.ToString("yyyy-MM-dd")
                    }],
                        PriceDetails = [ new()
                    {
                        Amount = new()
                        {
                            BaseCurrency = seatOrderItem.Amount.BaseCurrency,
                            MonetaryAmount = seatOrderItem.Amount.MonetaryAmount.Contains('.') || seatOrderItem.Amount.DecimalPrecisionCount == 0 ? seatOrderItem.Amount.MonetaryAmount : seatOrderItem.Amount.MonetaryAmount.Insert(seatOrderItem.Amount.MonetaryAmount.Length - seatOrderItem.Amount.DecimalPrecisionCount , ".")
                        },
                        PriceComponentType = EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum
                    }],
                        Services = [ new() {
                        ServiceId = "SV1",
                        CurrentSeatAssignment =  seatOrderItem.SeatNumber,
                        CurrentSeatAssignmentType = "PrimeSale",
                        ServiceBeneficiaries = [seatOrderItem.CustomerRefId],
                        ProductSegmentAssociation = new()
                        {
                            flightSegmentRefIds = [$"SEG{seatOrderItem.FlightIdRef.TrimStart('0')}"],
                        },
                        Product = new()
                        {
                           ProductType = EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum,
                           ProductOwner = Constants.Application.OwnerAirline
                        },
                        SegmentRefIds = [$"SEG{seatOrderItem.FlightIdRef.TrimStart('0')}"]
                    }],
                        PaymentDetails = seatOrderItem.PaymentDetails.Where(x => !x.PaymentReferenceId.IsNullOrEmpty()).Select(p =>
                        {
                            p.Amount = new()
                            {
                                BaseCurrency = seatOrderItem.Amount.BaseCurrency,
                                MonetaryAmount = transactionIdStatus.First(item => item.TransactionId == seatOrderItem.PaymentDetails.First().PaymentReferenceId).Amount.ToString()
                            };

                            return p;
                        }),
                    });

                    if (transactionIdStatus != null)
                    {
                        if (!seatOrderItem.PaymentDetails.First().PaymentReferenceId.IsNullOrEmpty() && orderDoc.OrderItems.Any(item => item.PaymentDetails != null && item.PaymentDetails.Any(k => k.PaymentReferenceId == seatOrderItem.PaymentDetails.First().PaymentReferenceId)))
                        {
                            orderDoc.OrderItems.First(item => item.PaymentDetails != null && item.PaymentDetails.Any(k => k.PaymentReferenceId == seatOrderItem.PaymentDetails.First().PaymentReferenceId)).PaymentDetails.First().PaymentProcessingStatus = transactionIdStatus.First(item => item.TransactionId == seatOrderItem.PaymentDetails.First().PaymentReferenceId).PaymentStatus ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.SUCCESSFUL : EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.FAILED;
                        }

                    }
                }
            }

            if (issueTicketResponse?.SegmentDetails != null)
            {
                orderDoc.DataList.SegmentDetails = issueTicketResponse.SegmentDetails;
            }

            if (priceClass != null && orderDoc.DataList?.PriceClass != null)
            {
                foreach (var offerPriceClass in priceClass)
                {
                    if (offerPriceClass?.TicketDesigCode == null)
                        continue;

                    var matchingOrderPriceClass = orderDoc.DataList.PriceClass.FirstOrDefault(p => p.Id == offerPriceClass.Id);
                    if (matchingOrderPriceClass != null)
                    {
                        matchingOrderPriceClass.TicketDesigCode = offerPriceClass.TicketDesigCode;
                    }
                }
            }


            return orderDoc;
        }

        public static void UpdateOrderDocument(OrderDocument orderDocument, Infrastructure.Model.Air4OrderManagementMS.OrderChangeResponse orderChangeResponse, IEnumerable<SeatDetails> seatDetails, IEnumerable<SeatTransactionId> transactionIds, [Optional] List<TransactionIdStatus> transactionIdStatus)
        {
            orderChangeResponse.OrderDetails.OrderItems = orderChangeResponse.OrderDetails.OrderItems.Select(a =>
            {
                a.AccountableDocuments.First().CustomerRefId = seatDetails.First(x => x.SeatNumber == a.CurrentSeatAssignment && x.AssociatedSegment.SegmentId == a.SegmentRefId).Beneficiary.CustId;
                return a;
            });

            foreach (var doc in orderChangeResponse.OrderDetails.OrderItems.OrderBy(x => x.AccountableDocuments.First().CustomerRefId))
            {
                var seatDetail = seatDetails.FirstOrDefault(x => x.SeatNumber == doc.CurrentSeatAssignment && x.AssociatedSegment.SegmentId == doc.SegmentRefId);

                orderDocument.OrderItems = orderDocument.OrderItems.Append(new()
                {
                    OrderItemId = $"{orderDocument.OrderItems.Last().OrderItemId.Split('-').First()}-{Convert.ToInt32(orderDocument.OrderItems.Last().OrderItemId.Split('-').Last()) + 1}",
                    OwnerAirline = Constants.Application.OwnerAirline,
                    AccountableDocumentRefs = [ new()
                    {
                        DocumentType = AccountableDocumentRef.DocumentTypes.YEnum,
                        DocumentNumber = doc.AccountableDocuments.First().DocumentNumber,
                        CustomerRefId = seatDetail.Beneficiary.CustId,
                        Coupons = [ new() { CouponStatus = Constants.Application.SearchOpenCouponStatus, SegmentRefId = $"SEG{doc.SegmentRefId.TrimStart('0')}" }],
                        IssueDate = DateTime.UtcNow.ToString("yyyy-MM-dd")
                    }],
                    PriceDetails = [ new()
                    {
                        Amount = new()
                        {
                            BaseCurrency = seatDetail.Currency,
                            MonetaryAmount = seatDetail.SeatPrice
                        },
                        PriceComponentType = EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum
                    }],
                    Services = [ new() {
                        ServiceId = "SV1",
                        CurrentSeatAssignment =  doc.CurrentSeatAssignment,
                        CurrentSeatAssignmentType = "PostSale",
                        ServiceBeneficiaries = [seatDetail.Beneficiary.CustId],
                        ProductSegmentAssociation = new()
                        {
                            flightSegmentRefIds = [$"SEG{doc.SegmentRefId.TrimStart('0')}"],
                        },
                        Product = new()
                        {
                           ProductType = EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum,
                           ProductOwner = Constants.Application.OwnerAirline
                        },
                        SegmentRefIds = [$"SEG{doc.SegmentRefId.TrimStart('0')}"]
                    }],
                    PaymentDetails =
                    [
                        new EAPI.Domain.Virgin.Models.Payment.PaymentDetails
                        {
                            Amount = new ()
                            {
                                BaseCurrency = seatDetail.Currency,
                                MonetaryAmount = transactionIdStatus?.FirstOrDefault(t => t.TransactionId == transactionIds.FirstOrDefault(td => td.SegmentId == doc.SegmentRefId).TransactionId)?.Amount?.ToString()
                            },
                            PaymentReferenceId = transactionIds?.FirstOrDefault(t => t.TransactionId == transactionIds.FirstOrDefault(td => td.SegmentId == doc.SegmentRefId).TransactionId)?.TransactionId,
                            PaymentProcessingStatus = EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.SUCCESSFUL,
                            PaymentCommitmentDateTime = doc.PaymentDetails.PaymentCommitmentDateTime,
                        }
                    ],
                });
            }
        }

        public static void UpdateSSRAPISGSTToOrderDoc(OrderDocument orderDoc, OrderDocument newOrderDoc)
        {
            foreach (var cust in orderDoc.DataList.CustomerDetails)
            {
                var customer = newOrderDoc.DataList.CustomerDetails?.FirstOrDefault(x => x.FirstName.Equals(cust.FirstName, StringComparison.OrdinalIgnoreCase) && x.LastName.Equals(cust.LastName, StringComparison.OrdinalIgnoreCase));

                if (customer?.TravelDocuments is { Count: > 0 })
                    if (cust.TravelDocuments == null)
                    {
                        cust.TravelDocuments = customer.TravelDocuments;
                        cust.IsApisAdded = true;
                    }
                    else
                    {
                        cust.TravelDocuments.AddRange(customer.TravelDocuments);
                        cust.IsApisAdded = true;
                    }
            }

            if (newOrderDoc.DataList.OrderInstructions != null && newOrderDoc.DataList.OrderInstructions.Any())
                orderDoc.DataList.OrderInstructions = orderDoc.DataList.OrderInstructions?.Concat(newOrderDoc.DataList.OrderInstructions);

            var lastOrderItemId = orderDoc.OrderItems.Last().OrderItemId.Split('-').Last();
            var numericPart = char.IsLetter(lastOrderItemId.Length > 1 ? lastOrderItemId[1] : '0') ? lastOrderItemId[0].ToString() : lastOrderItemId;

            var orderItemIdCounter = Convert.ToInt32(numericPart) + 1;

            foreach (var item in newOrderDoc.OrderItems)
                item.OrderItemId = $"{orderDoc.OrderNumber.OrderId}-{orderItemIdCounter++}";

            orderDoc.OrderItems = orderDoc.OrderItems.Concat(newOrderDoc.OrderItems);

        }

        public static void UpdateCancelFlowOrderDocument(OrderDocument orderDoc, CancelReshopResponse offerDoc, OrderDocument sabreOrderDoc, bool isCancelledAndRebooked, bool isCancelDisruption)
        {
            if (isCancelledAndRebooked)
            {
                orderDoc.DataList.SegmentDetails = sabreOrderDoc.DataList.SegmentDetails;
                orderDoc.DataList.OriginDestinationAssociations = sabreOrderDoc.DataList.OriginDestinationAssociations;
                orderDoc.DataList.Journey = sabreOrderDoc.DataList.Journey;
                orderDoc.DataList.PriceClass = sabreOrderDoc.DataList.PriceClass;
            }

            orderDoc.DataList.Penalty = offerDoc.DataList.Penalty;
            orderDoc.OrderItems = orderDoc.OrderItems.Where(x => x.AccountableDocumentRefs != null).Select(oi =>
            {
                var isFreeSeat = oi.PriceDetails?.All(pd => pd.Amount?.MonetaryAmount == "0.00") == true;
                if (!isFreeSeat && oi.AccountableDocumentRefs != null)
                {
                    oi.AccountableDocumentRefs = oi.AccountableDocumentRefs.Select(a =>
                    {
                        a.Coupons = a.Coupons.Select(c => { c.CouponStatus = c.CouponStatus == Constants.Application.SearchUsedCouponStatus || c.CouponStatus == Constants.Application.FlownEMDCouponStatus ? c.CouponStatus : offerDoc.IsRfc && !isCancelDisruption ? "VOID" : "RFND"; return c; });

                        return a;
                    });
                }

                return oi;
            });
            foreach (var item in offerDoc.Offers.First().OfferItems)
            {
                var servicBenefList = item.Services.First().ServiceBeneficiaries;

                var orderItem = orderDoc.OrderItems.FirstOrDefault(x => x.Services.First().ServiceBeneficiaries.Any(a => servicBenefList.Contains(a)));

                var offerItemPricing = item.OfferitemPricing.FirstOrDefault();
                orderItem.PriceDetails = GetPriceDetails(item.OfferitemPricing.First().SalePrice, item.OfferitemPricing.First().FareCalculationText);
                orderItem.SalePriceDifference = GetPriceDetails(item.OfferitemPricing.First().SalePriceDifference, item.OfferitemPricing.First().FareCalculationText);
                orderItem.OriginalSalePrice = GetPriceDetails(item.OfferitemPricing.First().OriginalSalePrice, item.OfferitemPricing.First().FareCalculationText);
                orderItem.RefundPrice = item.OfferitemPricing.First()?.RefundPrice;

                orderItem.Services.First().PenaltyRef = item.Services.First().PenaltyRef;

                orderItem.Services.First().Status = new State()
                {
                    Status = Constants.Application.ServiceStatusCodeCancelled
                };
            }

            foreach (var item in orderDoc.DataList.SegmentDetails.Where(seg => seg.Status.DeliveryStatusCode != "Delivered"))
            {
                item.Status = new EAPI.Domain.Virgin.Models.Products.Flight.Status()
                {
                    Code = "HX"
                };
            }

            orderDoc.OrderStatus = Constants.Application.ServiceStatusCodeCancelled;
            
            orderDoc.ChangeType = offerDoc.IsRfc && !isCancelDisruption ? ChangeTypeEnum.DocumentVoided : ChangeTypeEnum.DocumentRefunded;
            orderDoc.Version = Convert.ToInt32(orderDoc.Version) + 1;
            orderDoc.ChangeDateTime = new() { UtcDateTime = DateTime.UtcNow.ToString(Constants.Application.ZuluDateTimeFormat) };
        }

        public static void UpdateCancelHoldBookingOrderDocument(OrderDocument orderDoc)
        {
            orderDoc.OrderItems = orderDoc.OrderItems?.Select(orderItem =>
            {
                if (orderItem.SellerFollowUpAction != null) orderItem.SellerFollowUpAction = null;
                orderItem.Services = orderItem.Services?.Select(service =>
                {
                    service.Status = new State
                    {
                        Status = Constants.Application.ServiceStatusCodeCancelled
                    };

                    return service;
                });

                return orderItem;
            });

            foreach (var segment in orderDoc.DataList?.SegmentDetails?.Where(segment => segment.Status?.DeliveryStatusCode != Constants.Application.DeliveryStatusCodeDelivered) ?? Enumerable.Empty<VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment>())
            {
                segment.Status = new VAA.EAPI.Domain.Virgin.Models.Products.Flight.Status
                {
                    Code = Constants.Application.FlightStatusCodeRemoved
                };
            }

            orderDoc.OrderStatus = Constants.Application.ServiceStatusCodeCancelled;
            orderDoc.ChangeType = ChangeTypeEnum.DocumentVoided;
            orderDoc.Version++;
            orderDoc.ChangeDateTime = new() { UtcDateTime = DateTime.UtcNow.ToString(Constants.Application.ZuluDateTimeFormat) };
        }

        public static void ReconcileCancelHoldBookingRebookedOrderDocument(OrderDocument orderDoc, OrderDocument reservationOrderDoc)
        {
            if (orderDoc.OrderItems.Any(orderItem => !string.IsNullOrEmpty(orderItem.OrderItemId) && char.IsLetter(orderItem.OrderItemId.Last())))
            {
                MapOrderItems(orderDoc, reservationOrderDoc);
            }

            foreach (var orderItem in orderDoc.OrderItems)
            {
                var reservationOrderItem = reservationOrderDoc.OrderItems.FirstOrDefault(item => item.OrderItemId == orderItem.OrderItemId);
                if (reservationOrderItem is not null)
                {
                    orderItem.Services = reservationOrderItem.Services;
                }
            }

            orderDoc.DataList.SegmentDetails = reservationOrderDoc.DataList.SegmentDetails;
            orderDoc.DataList.OriginDestinationAssociations = reservationOrderDoc.DataList.OriginDestinationAssociations;
            orderDoc.DataList.Journey = reservationOrderDoc.DataList.Journey;
            orderDoc.DataList.PriceClass = reservationOrderDoc.DataList.PriceClass;
        }

        public static void UpdateCancelHoldBookingPriceDetails(OrderDocument orderDoc)
        {
            foreach (var orderItem in orderDoc.OrderItems?.Where(orderItem => orderItem.Services?.Any(service => service.Product?.ProductType == Product.ProductTypes.FlightEnum) == true) ?? Enumerable.Empty<VAA.EAPI.Domain.Virgin.Models.Order.OrderItems>())
            {
                var originalPriceDetails = orderItem.PriceDetails?.ToList();
                if (originalPriceDetails?.Any() != true)
                {
                    continue;
                }

                var zeroPriceDetails = originalPriceDetails.Select(price => new Price
                {
                    PriceComponentType = price.PriceComponentType,
                    PriceComponentName = price.PriceComponentName,
                    Amount = new()
                    {
                        BaseCurrency = price.Amount?.BaseCurrency,
                        MonetaryAmount = "0.00"
                    },
                    TaxBreakup = price.TaxBreakup?.Select(tax => new Tax
                    {
                        Code = tax.Code,
                        Name = tax.Name,
                        CountryCode = tax.CountryCode,
                        Refundable = tax.Refundable,
                        Amount = new()
                        {
                            BaseCurrency = tax.Amount?.BaseCurrency,
                            MonetaryAmount = "0.00"
                        }
                    }).ToList(),
                    FareCalculation = price.FareCalculation
                }).ToList();

                orderItem.OriginalSalePrice = originalPriceDetails;
                orderItem.PriceDetails = zeroPriceDetails;
                orderItem.SalePriceDifference = zeroPriceDetails;
                orderItem.RefundPrice = new[] { new RefundPrice
                {
                    Amount = new ()
                    {
                        BaseCurrency = originalPriceDetails.FirstOrDefault(price => price.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum)?.Amount?.BaseCurrency ?? originalPriceDetails.First().Amount?.BaseCurrency,
                        MonetaryAmount = "0.00"
                    }
                }};
            }
        }

        private static IEnumerable<Price> GetPriceDetails(List<Price> salePrice, string farecalculationtext)
        {
            return salePrice.Select(sp =>
                new Price()
                {
                    Amount = sp.Amount,
                    PriceComponentType = sp.PriceComponentType,
                    BaseCurrencyAmount = sp.BaseCurrencyAmount,
                    TaxBreakup = sp.TaxBreakup,
                    FareCalculation = farecalculationtext,
                });
        }

        public static void UpdateOrderDocument(OrderDocument orderDoc, PaymentRS paymentRS)
        {
            orderDoc.OrderItems = orderDoc.OrderItems.Select(x =>
            {
                if (x.PaymentDetails == null)
                {
                    x.PaymentDetails = [new() {
                        CardMethod = new()
                    {
                        CardBrandCode = paymentRS.PaymentDocument.CardType,
                        CardNumber = paymentRS.PaymentDocument.MaskedCardNumber
                    }
                    }];
                }
                else
                {
                    x.PaymentDetails = x.PaymentDetails.Select(p =>
                    {
                        p.PaymentCommitmentDateTime = p.PaymentReferenceId == paymentRS.PaymentDocument.TransactionId ? paymentRS.PaymentDocument.PaymentCommitedDateTime : p.PaymentCommitmentDateTime;
                        p.CardMethod = new()
                        {
                            CardBrandCode = paymentRS.PaymentDocument.CardType,
                            CardNumber = paymentRS.PaymentDocument.MaskedCardNumber
                        };
                        return p;
                    });
                }
                return x;
            });
        }

        public static OrderDocument UpdatePaymentDetails(IEnumerable<PaymentTransactionDocument> paymentDocuments, OrderDocument orderDocument)
        {
            orderDocument.OrderItems = orderDocument.OrderItems.Select(oi =>
            {
                var freePaymentDetails = oi.PaymentDetails?.Where(x => x.Amount?.MonetaryAmount == "0.00").Select(x => new EAPI.Domain.Virgin.Models.Payment.PaymentDetails()
                {
                    PaymentReferenceId = x.PaymentReferenceId,
                    Amount = x.Amount,
                    PaymentCommitmentDateTime = x.PaymentCommitmentDateTime,
                    PaymentProcessingStatus = x.PaymentProcessingStatus
                });

                foreach (var doc in paymentDocuments.Where(p => p.Id.Contains("Refund")))
                {
                    var capturedDoc = paymentDocuments.FirstOrDefault(pd => pd.Id == doc.Id.Split("-").First());

                    doc.OrderItems = capturedDoc?.OrderItems;
                    doc.PaymentDetails.First().CardMethod = capturedDoc?.PaymentDetails?.FirstOrDefault()?.CardMethod;
                }

                var paymentDetails = paymentDocuments.Where(p => p.OrderItems != null && p.OrderItems.Select(o => o.OrderItemId).Contains(oi.OrderItemId));

                if (paymentDetails.Any())
                {
                    oi.PaymentDetails = paymentDetails.Select(p =>
                    {
                        return new EAPI.Domain.Virgin.Models.Payment.PaymentDetails()
                        {
                            PaymentReferenceId = p.Id,
                            CardMethod = new CardDetails()
                            {
                                CardNumber = p.PaymentDetails.First().CardMethod.CardNumber,
                                CardBrandCode = p.PaymentDetails.First().CardMethod.CardType
                            },
                            PaymentProcessingStatus = p.Id.Contains("Refund") && p.Status.Contains("Refund") ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.SUCCESSFUL : p.Status == "Payment Captured" ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.SUCCESSFUL : EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.FAILED,
                            Amount = p.Id.Contains("Refund") ? new() { MonetaryAmount = "-" + p.PaymentDetails.First().Amount.MonetaryAmount } : p.PaymentDetails.First().Amount,
                            PaymentCommitmentDateTime = p.Id.Contains("Refund") ? p.CreatedOn : p.ModifiedOn
                        };
                    }).Concat(freePaymentDetails ?? []);
                }
                else
                {
                    oi.PaymentDetails = freePaymentDetails?.Any() == true ? freePaymentDetails : null;
                }
                return oi;
            });

            return orderDocument;
        }

        public static OrderDocument UpdateChangeOrderDocument(OrderDocument orderDoc, OrderDocument orderDocMgmt)
        {
            orderDoc.DataList.SegmentDetails = orderDoc.DataList.SegmentDetails.Select(oldSegment =>
            {
                bool matchFound = orderDocMgmt.DataList.SegmentDetails.Any(newSegment =>
                    newSegment.OriginAirportCode == oldSegment.OriginAirportCode &&
                    newSegment.DestinationAirportCode == oldSegment.DestinationAirportCode &&
                    Convert.ToDateTime(newSegment.ScheduledDepartureTime.LocalDateTime) == Convert.ToDateTime(oldSegment.ScheduledDepartureTime.LocalDateTime) &&
                    newSegment.MarketingFlightNumber == oldSegment.MarketingFlightNumber && newSegment.ClassOfService == oldSegment.ClassOfService
                );

                var flownSegmentId = orderDoc.DataList.SegmentDetails.Where(x => x.Status?.DeliveryStatusCode == "Delivered").Select(x => x.SegmentId);
                bool isPartialFlown = flownSegmentId?.Any(id => id.Contains(oldSegment.SegmentId)) ?? false;
                if (!isPartialFlown) oldSegment.Status.Code = matchFound ? "TR" : "HX";

                if (!matchFound && !oldSegment.SegmentId.EndsWith("OLD"))
                {
                    oldSegment.SegmentId = $"{oldSegment.SegmentId}OLD";
                }

                return oldSegment;
            }).ToList();

            orderDoc.DataList.BaggageAllowances = orderDoc.DataList.BaggageAllowances.Select(oldBaggage =>
            {
                var filteredBaggages = orderDocMgmt.DataList.BaggageAllowances.Where(x => x.BaggageAllowanceTypeCode == oldBaggage.BaggageAllowanceTypeCode && x.FreeBaggageAllowance.AllowedBagCnt == oldBaggage.FreeBaggageAllowance.AllowedBagCnt);
                bool matchFound = filteredBaggages.Any(newBaggages =>
                {
                    var newBag = newBaggages.FreeBaggageAllowance;
                    var oldBag = oldBaggage.FreeBaggageAllowance;

                    bool dimensionMatch = (newBag.DimensionAllowance == null && oldBag.DimensionAllowance == null) ||
                                          (newBag.DimensionAllowance != null && oldBag.DimensionAllowance != null && newBag.DimensionAllowance.All(nd =>
                                          oldBag.DimensionAllowance.Any(od =>
                                             nd.HeightMeasure == od.HeightMeasure &&
                                             nd.LengthMeasure == od.LengthMeasure &&
                                             nd.WidthMeasure == od.WidthMeasure &&
                                             nd.LinearMeasure == od.LinearMeasure
                                          )));

                    bool weightMatch =  (newBag.WeightAllowance == null && oldBag.WeightAllowance == null) ||
                                        (newBag.WeightAllowance != null && oldBag.WeightAllowance != null && newBag.WeightAllowance.All(nw =>
                                             oldBag.WeightAllowance.Any(ow =>
                                                 nw.MaximumWeightMeasure == ow.MaximumWeightMeasure
                                             )));

                    bool bagDescMatch =  (newBag.BagDescriptors == null && oldBag.BagDescriptors == null) || (newBag.BagDescriptors != null && oldBag.BagDescriptors != null && newBag.BagDescriptors.First().BagDescriptorName.Equals(oldBag.BagDescriptors.First().BagDescriptorName));

                    return dimensionMatch && weightMatch && bagDescMatch;
                });

                if (!matchFound && !oldBaggage.BaggageAllowanceId.EndsWith("OLD"))
                {
                    oldBaggage.BaggageAllowanceId = $"{oldBaggage.BaggageAllowanceId}OLD";
                }

                return oldBaggage;
            }).ToList();

            orderDoc.DataList.ServiceDefinitions = orderDoc.DataList.ServiceDefinitions.Select(oldService =>
            {
                bool matchFound = orderDocMgmt.DataList.ServiceDefinitions.Any(newService => string.Join(",", (newService.Description ?? Enumerable.Empty<string>()).OrderBy(x => x)) == string.Join(",", (oldService.Description ?? Enumerable.Empty<string>()).OrderBy(x => x)));
                if (!matchFound && !oldService.ServiceDefinitionId.EndsWith("OLD"))
                {
                    oldService.ServiceDefinitionId = $"{oldService.ServiceDefinitionId}OLD";
                }
                return oldService;
            }).ToList();

            var retainedJourneySegments = orderDoc.DataList.SegmentDetails.Where(oldSeg => orderDocMgmt.DataList.SegmentDetails.Any(seg =>
                                                             seg.OriginAirportCode == oldSeg.OriginAirportCode &&
                                                             seg.DestinationAirportCode == oldSeg.DestinationAirportCode &&
                                                             seg.MarketingFlightNumber == oldSeg.MarketingFlightNumber &&
                                                             seg.ClassOfService == oldSeg.ClassOfService &&
                                                             Convert.ToDateTime(seg.ScheduledDepartureTime.LocalDateTime) == Convert.ToDateTime(oldSeg.ScheduledDepartureTime.LocalDateTime)
                                                             )).Select(x => x.SegmentId);

            var filteredRetainedOrderItems = orderDoc.OrderItems.Where(item => !retainedJourneySegments.Any()|| item.Services != null && item.Services.Any(srv => srv.Product?.ProductType == Product.ProductTypes.FlightEnum || (srv.Product?.ProductType == Product.ProductTypes.SeatEnum && !srv.SegmentRefIds.Any(segId => retainedJourneySegments.Contains(segId)))));
            orderDoc.OrderItems = filteredRetainedOrderItems;
            foreach (var item in filteredRetainedOrderItems)
            {
                item.Services = item.Services.Select(service =>
                {
                    service.SegmentRefIds = service.SegmentRefIds.Select(id =>
                    {
                        if (!id.EndsWith("OLD") && orderDoc.DataList.SegmentDetails.Any(seg => seg.SegmentId == $"{id}OLD"))
                        {
                            return $"{id}OLD";
                        }
                        return id;
                    });

                    if(service.Product != null && service.Product.ProductType == Product.ProductTypes.FlightEnum && service.BaggageRefIds != null && service.ServiceDefinitionRefIds != null)
                    {
                        service.BaggageRefIds = service.BaggageRefIds.Select(id =>
                        {
                            if (!id.EndsWith("OLD") && orderDoc.DataList.BaggageAllowances.Any(bag => bag.BaggageAllowanceId == $"{id}OLD"))
                            {
                                return $"{id}OLD";
                            }
                            return id;
                        });

                        service.ServiceDefinitionRefIds = service.ServiceDefinitionRefIds.Select(id =>
                        {
                            if (!id.EndsWith("OLD") && orderDoc.DataList.ServiceDefinitions.Any(servDef => servDef.ServiceDefinitionId == $"{id}OLD"))
                            {
                                return $"{id}OLD";
                            }
                            return id;
                        });
                    }

                    return service;
                });

                item.AccountableDocumentRefs = item.AccountableDocumentRefs?.Select(doc =>
                {
                    doc.Coupons = doc.Coupons.Select(coupon =>
                    {
                        if (!coupon.SegmentRefId.EndsWith("OLD") && orderDoc.DataList.SegmentDetails.Any(seg => seg.SegmentId == $"{coupon.SegmentRefId}OLD"))
                        {
                            coupon.SegmentRefId = $"{coupon.SegmentRefId}OLD";
                        }
                        return coupon;
                    });
                    return doc;
                });
            }

            var segmentIds = orderDoc.DataList.SegmentDetails.Select(s => s.SegmentId).ToList();

            orderDoc.DataList.OriginDestinationAssociations = orderDoc.DataList.OriginDestinationAssociations.Select(od =>
            {
                od.FlightSegmentRefIds = od.FlightSegmentRefIds.Select(refId => !segmentIds.Contains(refId) && segmentIds.Contains(refId + "OLD") ? refId + "OLD" : refId);
                return od;
            });

            orderDoc.DataList.Journey = orderDoc.DataList.Journey.Select(journey =>
            {
                journey.FlightSegmentRefIds = journey.FlightSegmentRefIds.Select(refId => !segmentIds.Contains(refId) && segmentIds.Contains(refId + "OLD") ? refId + "OLD" : refId);
                return journey;
            });

            if (orderDoc.OrderHistory != null)
            {
                orderDocMgmt.OrderHistory = orderDoc.OrderHistory;
                var history = orderDocMgmt.OrderHistory?.OrderBy(x => x.Version).Last();
                orderDocMgmt.OrderHistory = orderDocMgmt.OrderHistory.Append(new()
                {
                    DataList = orderDoc.DataList,
                    OrderItems = orderDoc.OrderItems,
                    TimeLimits = orderDoc?.TimeLimits,
                    Version = Convert.ToString(Convert.ToInt32(history.Version) + 1),
                });
            }
            else
            {
               orderDocMgmt.OrderHistory = [new ()
               {
                    DataList = orderDoc.DataList,
                    OrderItems = orderDoc.OrderItems,
                    TimeLimits = orderDoc?.TimeLimits,
                    Version = "1"
                }];
            }

            orderDocMgmt.FareTypeRequest = orderDoc.FareTypeRequest;

            orderDocMgmt.Parties = orderDoc.Parties;

            return orderDocMgmt;
        }

        public static OrderDocument UpdateOrderItemId(OrderDocument orderDocMgmt, OrderDocument oldOrderDoc, bool isDisrupted = false)
        {
            orderDocMgmt.OrderItems = orderDocMgmt.OrderItems.Select(orderItem =>
            {
                var orderMgmtOrderItem = orderItem.Services.SelectMany(x => x.ServiceBeneficiaries).Distinct();
                var oldOrderItem = oldOrderDoc.OrderItems.FirstOrDefault(x => x.Services != null && x.Services.Any(service => service.Product.ProductType == orderItem.Services.First().Product.ProductType && service.ServiceBeneficiaries.Any(srv => orderMgmtOrderItem.Contains(srv))));

                if (oldOrderItem != null)
                {
                    orderItem.ExistingOrderItemId = oldOrderItem.OrderItemId;
                    orderItem.OrderItemId = GerOrderItemID(oldOrderItem.OrderItemId);
                }
                return orderItem;
            });
            if(!isDisrupted) UpdateRetainedJourneySeatOrderItems(orderDocMgmt, oldOrderDoc);

            return orderDocMgmt;
        }

        private static string GerOrderItemID(string orderItemId)
        {
            var lastPart = orderItemId.Split('-').Last();
            var prefix = orderItemId.Substring(0, orderItemId.LastIndexOf('-') + 1);

            if (char.IsLetter(lastPart.Last()))
            {
                var numberPart = lastPart.Substring(0, lastPart.Length - 1);
                var letter = lastPart.Last();
                if (letter < 'Z')
                {
                    char nextLetter = (char)(letter + 1);
                    return prefix + numberPart + nextLetter;
                }
            }
            else
            {
                return orderItemId + "A";
            }

            return orderItemId;
        }

        public static OrderDocument UpdateOrderDocumentHistory(OrderDocument oldOrderDoc, OrderDocument newOrderDoc, string changeDateTime)
        {
            var latestHistory = newOrderDoc.OrderHistory?.FirstOrDefault();
            var latestHistorySeg = latestHistory.DataList.SegmentDetails.First();

            if (oldOrderDoc.DataList.SegmentDetails.Any(s => latestHistorySeg.OriginAirportCode == s.OriginAirportCode && s.DestinationAirportCode == latestHistorySeg.DestinationAirportCode && Convert.ToDateTime(s.ScheduledDepartureTime.LocalDateTime) == Convert.ToDateTime(latestHistorySeg.ScheduledDepartureTime.LocalDateTime) && Convert.ToDateTime(s.ScheduledArrivalTime.LocalDateTime) == Convert.ToDateTime(latestHistorySeg.ScheduledArrivalTime.LocalDateTime) && s.MarketingFlightNumber == latestHistorySeg.MarketingFlightNumber))
            {
                newOrderDoc.OrderHistory = [new OrderHistory()
                {
                    DataList = oldOrderDoc.DataList,
                    OrderItems = oldOrderDoc.OrderItems,
                    Version = (Convert.ToInt32(oldOrderDoc.Version) + 1).ToString(),
                    ChangeType = latestHistory.ChangeType,
                    ChangeDateTime = changeDateTime
                }];

            }

            return newOrderDoc;
        }

        public static OrderDocument UpdateOrderHistoryDocument(OrderDocument oldOrderDoc, OrderDocument historyOrderDoc, OrderDocument getReservationOrderDoc)
        {
            var latestGetReservationSabreHistory = getReservationOrderDoc.OrderHistory?.OrderBy(x => x.Version).LastOrDefault();
            bool updateHistory = false;
            bool isHistoryDocExists = historyOrderDoc == null;
            if (isHistoryDocExists)
            {
                historyOrderDoc = new OrderDocument()
                {
                    DocType = oldOrderDoc.DocType,
                    FareTypeRequest = oldOrderDoc.FareTypeRequest,
                    PointOfSale = oldOrderDoc.PointOfSale,
                    CreationDateTime = oldOrderDoc.CreationDateTime,
                    BookingReference = oldOrderDoc.BookingReference,
                    OrderNumber = oldOrderDoc.OrderNumber,
                    Parties = oldOrderDoc.Parties,
                    TimeLimits = oldOrderDoc.TimeLimits,
                    OrderHistory = [new OrderHistory()
                    {
                        DataList = oldOrderDoc.DataList,
                        OrderItems = oldOrderDoc.OrderItems,
                        Version = "1",
                        OrderStatus = oldOrderDoc.OrderStatus,
                        ChangeType = ChangeTypeEnum.OrderItemAdded,
                        ChangeDateTime = oldOrderDoc.CreationDateTime.UtcDateTime
                    }],
                    Id = $"{oldOrderDoc.Id}-History",
                    PartitionKey = $"{oldOrderDoc.Id}-History"
                };

            }

            if (getReservationOrderDoc.OrderHistory.Any(x => x.DataList.CustomerDetails?.Any(y => y.TravelDocuments?.Any() ?? false) ?? false))
            {
                var historyDocList = historyOrderDoc.OrderHistory.OrderBy(h => Convert.ToInt32(h.Version));

                foreach (var history in historyDocList)
                {
                    var reservationHistory = getReservationOrderDoc.OrderHistory.FirstOrDefault(x => x.ChangeType == history.ChangeType && x.Version == history.Version);

                    if (reservationHistory == null)
                        continue;

                    foreach (var historyCustomer in history.DataList.CustomerDetails)
                    {
                        var reservationCustomer = reservationHistory.DataList.CustomerDetails.FirstOrDefault(x => x.FirstName == historyCustomer.FirstName && x.LastName == historyCustomer.LastName);

                        if (reservationCustomer == null || historyCustomer.IsNameChanged)
                            continue;

                        var previousReservationHistory = getReservationOrderDoc.OrderHistory.Where(x => Convert.ToInt32(x.Version) < Convert.ToInt32(reservationHistory.Version) && x.DataList.CustomerDetails.Any(cd => cd.TravelDocuments != null && cd.TravelDocuments.Any())).OrderByDescending(x => Convert.ToInt32(x.Version)).FirstOrDefault();
                        var previousReservationCustomer = previousReservationHistory?.DataList.CustomerDetails.FirstOrDefault(x => x.FirstName == historyCustomer.FirstName && x.LastName == historyCustomer.LastName);

                        if (reservationCustomer.TravelDocuments != null && reservationCustomer.TravelDocuments.Any())
                        {
                            if (previousReservationCustomer?.TravelDocuments != null)
                            {
                                foreach (var prevDoc in previousReservationCustomer.TravelDocuments)
                                {
                                    bool exists = reservationCustomer.TravelDocuments.Any(d => d.DocumentType == prevDoc.DocumentType);

                                    if (!exists)
                                        reservationCustomer.TravelDocuments.Add(prevDoc);
                                }
                            }

                            historyCustomer.TravelDocuments = reservationCustomer.TravelDocuments;
                        }
                        else
                        {
                            historyCustomer.TravelDocuments = previousReservationCustomer?.TravelDocuments;
                            reservationCustomer.TravelDocuments = previousReservationCustomer?.TravelDocuments;
                        }

                        history.ChangeDateTime = reservationHistory?.ChangeDateTime;
                        history.ChangeType = reservationHistory?.ChangeType;
                    }
                }
            }

            if (!isHistoryDocExists)
            {
                var latestHistory = historyOrderDoc.OrderHistory?.OrderBy(x => x.Version).LastOrDefault();
                var latestHistorySegs = latestHistory.DataList.SegmentDetails.Select(x => new { x.MarketingFlightNumber, x.OriginAirportCode, x.DestinationAirportCode, dep = x.ScheduledDepartureTime.LocalDateTime, arr = x.ScheduledArrivalTime.LocalDateTime, x.Status.Code, x.ClassOfService });

                //LatestPreviousHistory
                var latestPreviousSabreHistory = getReservationOrderDoc.OrderHistory?.OrderBy(x => x.Version).Take(Convert.ToInt32(latestGetReservationSabreHistory.Version) - 1).LastOrDefault();

                // Merging Ticketing and Apis Histories for Change Flows
                var isIdentityDocumentDetailsUpdated = latestGetReservationSabreHistory.ChangeType == ChangeTypeEnum.IdentityDocumentDetailsUpdated;
                var lastPreviousSabreHistory = latestPreviousSabreHistory?.ChangeType == ChangeTypeEnum.DocumentExchanged || latestPreviousSabreHistory?.ChangeType == ChangeTypeEnum.DocumentIssuance;
                var isApisAddedForCustomer = oldOrderDoc.DataList.CustomerDetails.Any(x => x.IsApisAdded);
                var orderDocChangeType = oldOrderDoc.ChangeType == ChangeTypeEnum.DocumentExchanged || oldOrderDoc.ChangeType == ChangeTypeEnum.DocumentIssuance;

                if (isIdentityDocumentDetailsUpdated && lastPreviousSabreHistory && isApisAddedForCustomer && orderDocChangeType)
                {
                    var finalHistories = new List<OrderHistory>();

                    finalHistories = getReservationOrderDoc.OrderHistory.Where(x => Convert.ToInt32(x.Version) < Convert.ToInt32(latestPreviousSabreHistory.Version)).OrderBy(x => Convert.ToInt32(x.Version)).ToList();
                    var lastFinalHistoriesVersion = historyOrderDoc.OrderHistory?.OrderBy(x => x.Version).LastOrDefault();

                    var combinedHistories = new OrderHistory()
                    {
                        DataList = latestGetReservationSabreHistory.DataList,
                        OrderItems = latestGetReservationSabreHistory.OrderItems,
                        Version = Convert.ToString(Convert.ToInt32(lastFinalHistoriesVersion.Version) + 1),
                        ChangeType = latestPreviousSabreHistory.ChangeType,
                        ChangeDateTime = latestPreviousSabreHistory.ChangeDateTime
                    };

                    finalHistories.Add(combinedHistories);

                    getReservationOrderDoc.OrderHistory = finalHistories;
                }

                var latestSabreHistory = getReservationOrderDoc.OrderHistory?.OrderBy(x => x.Version).LastOrDefault();

                var latestSabreHistorySegs = latestSabreHistory.DataList.SegmentDetails.Select(x => new { x.MarketingFlightNumber, x.OriginAirportCode, x.DestinationAirportCode, dep = x.ScheduledDepartureTime.LocalDateTime, arr = x.ScheduledArrivalTime.LocalDateTime, x.Status.Code, x.ClassOfService });

                var orderDocSegs = oldOrderDoc.DataList.SegmentDetails.Select(x => new { x.MarketingFlightNumber, x.OriginAirportCode, x.DestinationAirportCode, dep = x.ScheduledDepartureTime.LocalDateTime, arr = x.ScheduledArrivalTime.LocalDateTime, x.Status.Code, x.ClassOfService });

                var orderDocCustomerDetails = oldOrderDoc.DataList.CustomerDetails.Select(x => new { x.FirstName, x.FirstNameNumber, x.LastName, x.LastNameNumber });

                //match segment details with original order doc
                if (!latestSabreHistorySegs.Except(orderDocSegs).Any() && !orderDocSegs.Except(latestSabreHistorySegs).Any())
                {
                    if (latestSabreHistory.ChangeType == ChangeTypeEnum.OrderItemAdded)
                    {
                        //match ticket details
                        var sabreTickets = getReservationOrderDoc.OrderItems.Where(x => x.AccountableDocumentRefs != null).SelectMany(x => x.AccountableDocumentRefs).Where(x => x.DocumentType == AccountableDocumentRef.DocumentTypes.TEnum).Select(x => x.DocumentNumber);
                        var historyTickets = latestHistory.OrderItems.Where(x => x.AccountableDocumentRefs != null).SelectMany(x => x.AccountableDocumentRefs).Where(x => x.DocumentType == AccountableDocumentRef.DocumentTypes.TEnum).Select(x => x.DocumentNumber);

                        //match seat details
                        var sabreSeats = getReservationOrderDoc.OrderItems.Where(x => x.AccountableDocumentRefs != null).SelectMany(x => x.AccountableDocumentRefs).Where(x => x.DocumentType == AccountableDocumentRef.DocumentTypes.YEnum)?.Select(x => x.DocumentNumber);
                        var historySeats = latestHistory.OrderItems.Where(x => x.AccountableDocumentRefs != null).SelectMany(x => x.AccountableDocumentRefs).Where(x => x.DocumentType == AccountableDocumentRef.DocumentTypes.YEnum)?.Select(x => x.DocumentNumber);

                        //match SSR details
                        var sabreSSRs = getReservationOrderDoc.DataList.OrderInstructions ?? [];
                        var historySSRs = latestHistory.DataList.OrderInstructions ?? [];

                        if ((sabreTickets.Except(historyTickets).Any() || historyTickets.Except(sabreTickets).Any()) || (sabreSeats.Except(historySeats).Any() || historySeats.Except(sabreSeats).Any()) || (sabreSSRs.Except(historySSRs).Any() || historySSRs.Except(sabreSSRs).Any()))
                            updateHistory = true;
                    }
                    else if (latestSabreHistory.ChangeType == ChangeTypeEnum.DocumentIssuance)
                    {
                        //match ticket details
                        var sabreTickets = getReservationOrderDoc.OrderItems.Where(x => x.AccountableDocumentRefs != null).SelectMany(x => x.AccountableDocumentRefs).Where(x => x.DocumentType == AccountableDocumentRef.DocumentTypes.TEnum).Select(x => x.DocumentNumber);
                        var historyTickets = latestHistory.OrderItems.Where(x => x.AccountableDocumentRefs != null).SelectMany(x => x.AccountableDocumentRefs).Where(x => x.DocumentType == AccountableDocumentRef.DocumentTypes.TEnum).Select(x => x.DocumentNumber);

                        if (sabreTickets.Except(historyTickets).Any() || historyTickets.Except(sabreTickets).Any())
                            updateHistory = true;
                    }
                    else if (latestSabreHistory.ChangeType == ChangeTypeEnum.DocumentVoided || latestSabreHistory.ChangeType == ChangeTypeEnum.DocumentRefunded || latestSabreHistory.ChangeType == ChangeTypeEnum.DocumentExchanged)
                    {
                        if (latestSabreHistorySegs.Except(latestHistorySegs).Any() || latestHistorySegs.Except(latestSabreHistorySegs).Any())
                            updateHistory = true;
                    }
                    else if (latestSabreHistory.ChangeType == ChangeTypeEnum.SplitPNR)
                    {
                        updateHistory = false;
                    }
                    else if (latestSabreHistory.ChangeType == ChangeTypeEnum.NameCorrection)
                    {
                        var latestsabreCustomersDetails = latestSabreHistory.DataList.CustomerDetails.Select(c => new { c.FirstName, c.LastName, c.FirstNameNumber, c.LastNameNumber });

                        // join on number fields
                        var matched = from sabre in latestsabreCustomersDetails
                                      join order in orderDocCustomerDetails
                                      on new { sabre.FirstNameNumber, sabre.LastNameNumber }
                                      equals new { order.FirstNameNumber, order.LastNameNumber }
                                      select new { sabre, order };

                        // check if record already exists in history doc
                        if (latestHistory.ChangeType == latestSabreHistory.ChangeType)
                        {
                            IEnumerable<Customer> historyDocCustomersDetails = latestHistory?.DataList.CustomerDetails;

                            bool alreadyExists = historyDocCustomersDetails.All(hist => matched.Any(m => hist.FirstName.Is(m.order.FirstName) && hist.LastName.Is(m.order.LastName) && hist.FirstNameNumber == m.order.FirstNameNumber && hist.LastNameNumber == m.order.LastNameNumber));

                            if (alreadyExists)
                            {
                                updateHistory = false;
                            }

                            else if (matched.All(m => m.sabre.FirstName.Is(m.order.FirstName) || m.sabre.LastName.Is(m.order.LastName)))
                            {
                                updateHistory = true;
                            }
                        }

                        // check if any matched customers differ in names                        
                        else if (matched.Any(m => m.sabre.FirstName.Is(m.order.FirstName) || m.sabre.LastName.Is(m.order.LastName)))
                        {
                            updateHistory = true;
                        }
                    }
                    else if (latestSabreHistory.ChangeType == ChangeTypeEnum.FlightDateTimeChange || latestSabreHistory.ChangeType == ChangeTypeEnum.FlightCancellation)
                    {
                        if (latestSabreHistorySegs.Except(latestHistorySegs).Any() || latestHistorySegs.Except(latestSabreHistorySegs).Any())
                            updateHistory = true;
                    }
                    else if (latestGetReservationSabreHistory.ChangeType == ChangeTypeEnum.SpecialServiceRequestAdded && oldOrderDoc.IsPostSaleSSR)
                    {
                        var latestCount = latestGetReservationSabreHistory.DataList?.OrderInstructions?.Count();
                        var oldCount = latestHistory.DataList?.OrderInstructions?.Count();

                        bool latestHasWarnings = latestHistory?.Warnings?.Any() == true || latestHistory?.Warnings?.Any(x => x?.Code == "182" || x?.Code == "185") == true;
                        bool oldHasWarnings = oldOrderDoc?.Warnings?.Any() == true || oldOrderDoc?.Warnings?.Any(x => x?.Code == "182" || x?.Code == "185") == true;
                        bool oldWarningsNullOrEmpty = oldOrderDoc?.Warnings == null || oldOrderDoc?.Warnings?.Any() == false;
                        bool latestWarningsNull = latestHistory?.Warnings == null;

                        bool countChanged = latestCount != oldCount;
                        bool countSame = latestCount == oldCount;

                        if (countChanged && oldHasWarnings) updateHistory = true;

                        if ((countSame || countChanged) && latestHasWarnings && oldWarningsNullOrEmpty)
                            updateHistory = true;

                        if (countChanged && latestWarningsNull && oldWarningsNullOrEmpty)
                            updateHistory = true;
                    }

                    else if (latestSabreHistory.ChangeType == ChangeTypeEnum.IdentityDocumentDetailsUpdated)
                    {
                        var latestSabreHistoryTravelDocuments = latestSabreHistory.DataList.CustomerDetails.SelectMany(c => c.TravelDocuments ?? Enumerable.Empty<TravelDocument>()).Select(d => new { d.DocumentType, d.DocumentNumber, d.ExpirationDate, d.IssueDate, d.CountryOfIssue, d.CountryOfResidence, d.Nationality, d.RegionOfIssuance, d.DateOfBirth, d.Gender });
                        var latestHistoryTravelDocuments = latestHistory.DataList.CustomerDetails.SelectMany(c => c.TravelDocuments ?? Enumerable.Empty<TravelDocument>()).Select(d => new { d.DocumentType, d.DocumentNumber, d.ExpirationDate, d.IssueDate, d.CountryOfIssue, d.CountryOfResidence, d.Nationality, d.RegionOfIssuance, d.DateOfBirth, d.Gender });

                        if (latestSabreHistoryTravelDocuments.Except(latestHistoryTravelDocuments).Any() || latestHistoryTravelDocuments.Except(latestSabreHistoryTravelDocuments).Any())
                            updateHistory = true;
                    }

                    if (updateHistory)
                    {
                        var updatedHistory = new OrderHistory()
                        {
                            DataList = getReservationOrderDoc.DataList?.CustomerDetails?.Any(x => x?.TravelDocuments?.Any() ?? false) ?? false ? getReservationOrderDoc.DataList : oldOrderDoc.DataList,
                            OrderItems = oldOrderDoc.OrderItems,
                            OrderStatus = oldOrderDoc.OrderStatus,
                            Version = latestHistory != null ? Convert.ToString(Convert.ToInt32(latestHistory.Version) + 1) : "1",
                            ChangeType = latestSabreHistory.ChangeType,
                            ChangeDateTime = latestSabreHistory.ChangeDateTime,
                            ReasonCode = latestSabreHistory.ReasonCode,
                            Warnings = oldOrderDoc?.Warnings?.Any(x => x.Code == "182") == true ? oldOrderDoc?.Warnings : null

                        };

                        historyOrderDoc.OrderHistory = historyOrderDoc.OrderHistory.Append(updatedHistory);
                    }
                }
                else
                {
                    //order doc is out of sync - to be handled

                }
            }
           
            foreach(var historyDocCust in historyOrderDoc.OrderHistory.SelectMany(h => h.DataList.CustomerDetails))
            {
                var sabreCust = getReservationOrderDoc.OrderHistory.Where(h => h.DataList?.CustomerDetails != null).SelectMany(h => h.DataList.CustomerDetails)?.FirstOrDefault(s => s.FirstName == historyDocCust.FirstName && s.LastName == historyDocCust.LastName);

                if (sabreCust != null)
                {
                    historyDocCust.DateOfBirth = sabreCust.DateOfBirth;
                    historyDocCust.Gender = sabreCust.Gender;
                }
            }

            return historyOrderDoc;
        }

        public static OrderDocument UpdateOrderDocumentAmendApis(OrderDocument orderManagementAmendApisResponse, OrderDocument orderDocument)
        {
            var matchingCust = orderDocument.DataList.CustomerDetails.First(x => x.CustId == orderManagementAmendApisResponse.DataList.CustomerDetails.First().CustId);

            var amendApisTravelDocs = orderManagementAmendApisResponse.DataList.CustomerDetails.First().TravelDocuments;

            matchingCust.TravelDocuments = (matchingCust.TravelDocuments ?? Enumerable.Empty<TravelDocument>()).Select(doc => amendApisTravelDocs.FirstOrDefault(a => a.DocumentType == doc.DocumentType) ?? doc).ToList();

            var existingDocs = matchingCust.TravelDocuments?.Select(docs => docs.DocumentType) ?? Enumerable.Empty<string>();

            var additionalApisDocs = amendApisTravelDocs.Where(x => !existingDocs.Contains(x.DocumentType));

            matchingCust.TravelDocuments.AddRange(additionalApisDocs);
            matchingCust.IsApisAdded = true;
            orderDocument.ChangeType = ChangeTypeEnum.IdentityDocumentDetailsUpdated;
            orderDocument.Version = orderDocument.Version + 1;
            orderDocument.ChangeDateTime = orderManagementAmendApisResponse.ChangeDateTime;

            return orderDocument;
        }

        public static OrderDocument UpdateOrderDocumentNameCorrection(OrderDocument orderManagementNameCorrectionResponse, OrderDocument orderDocument)
        {
            var accountableDocRef = orderManagementNameCorrectionResponse.OrderItems.First().AccountableDocumentRefs?.First();
            var matchingCust = orderDocument.DataList.CustomerDetails.First(x => x.CustId == orderManagementNameCorrectionResponse.DataList.CustomerDetails.First().CustId);
            var matchingOrderItem = orderDocument.OrderItems.First(x => x.Services.Any(x => x.ServiceBeneficiaries.Contains(matchingCust.CustId)));

            var nameCorrectionTravelDocs = orderManagementNameCorrectionResponse.DataList.CustomerDetails.First().TravelDocuments;

            matchingCust.FirstName = orderManagementNameCorrectionResponse.DataList.CustomerDetails.First().FirstName;
            matchingCust.LastName = orderManagementNameCorrectionResponse.DataList.CustomerDetails.First().LastName;
            matchingCust.TravelDocuments = nameCorrectionTravelDocs;
            matchingCust.IsNameChanged = true;

            var orderDocCoupons = orderDocument.OrderItems.Select(x => x.AccountableDocumentRefs?.FirstOrDefault(docs => docs.CustomerRefId == accountableDocRef.CustomerRefId)?.Coupons).Where(coupons => coupons != null).SelectMany(coupons => coupons);
            matchingOrderItem.AccountableDocumentRefs = accountableDocRef == null ? matchingOrderItem.AccountableDocumentRefs : matchingOrderItem.AccountableDocumentRefs.Select(docRef => docRef.CustomerRefId == accountableDocRef.CustomerRefId ? new AccountableDocumentRef
            {
                DocumentType = accountableDocRef?.DocumentType,
                DocumentNumber = accountableDocRef?.DocumentNumber,
                IssueDate = accountableDocRef?.IssueDate,
                CustomerRefId = accountableDocRef?.CustomerRefId,
                FareCalculation = accountableDocRef?.FareCalculation,
                Coupons = accountableDocRef?.Coupons?.Select(x =>
                 new Coupon()
                 {
                     SegmentRefId = orderDocCoupons?.FirstOrDefault(doc => doc.CouponNumber == x.CouponNumber)?.SegmentRefId,
                     CouponNumber = x.CouponNumber,
                     TicketNumber = x.TicketNumber,
                     CouponStatus = x.CouponStatus,
                     FareBasis = x.FareBasis,
                     TicketExpirationDate = x.TicketExpirationDate
                 }).ToArray(),
                OriginalDocumentNumber = accountableDocRef?.OriginalDocumentNumber,
                OriginalIssueDate = accountableDocRef?.OriginalIssueDate
            } : docRef).ToArray();

            return orderDocument;
        }

        public static OrderDocument UpdateOrderItemNameCorrection(OrderDocument orderManagementNameCorrectionResponse, OrderDocument orderDocument)
        {
            var matchingCust = orderDocument.DataList.CustomerDetails.First(x => x.CustId == orderManagementNameCorrectionResponse.DataList.CustomerDetails.First().CustId);
            var orderItemId = orderDocument.OrderItems.First(x => x.Services.Any(x => x.ServiceBeneficiaries.Contains(matchingCust.CustId))).OrderItemId;
            var paymentDetails = orderManagementNameCorrectionResponse.OrderItems.First().PaymentDetails.First();

            orderDocument.OrderItems = orderDocument.OrderItems.Append(new()
            {
                OrderItemId = $"{orderItemId}NEW",
                OwnerAirline = orderDocument.OrderNumber.OwnerAirline,
                PriceDetails = [new()
                {
                     Amount = new()
                     {
                         MonetaryAmount = "0.00",
                         BaseCurrency = orderDocument.PointOfSale.CurrencyCode
                     }
                }],
                Services = [new()
                {
                    Product = new()
                    {
                        ProductType = Product.ProductTypes.NameCorrection
                    },
                    CustomerRefIds = new[] { orderManagementNameCorrectionResponse.DataList.CustomerDetails.First().CustId }
                }],
                PaymentDetails = [new()
                {
                      Amount =  new ()
                      {
                          MonetaryAmount = paymentDetails.Amount.MonetaryAmount,
                          BaseCurrency = paymentDetails.Amount.BaseCurrency ?? orderDocument.PointOfSale.CurrencyCode,
                      },
                      PaymentProcessingStatus = paymentDetails.PaymentProcessingStatus,
                      PaymentReferenceId = paymentDetails.PaymentReferenceId,
                      PaymentCommitmentDateTime = paymentDetails.PaymentCommitmentDateTime
                }]
            });

            return orderDocument;
        }

        public static OrderDocument MapFlownSegmentPenalty(OrderDocument orderdoc, OrderDocument sabredoc)
        {
            var flownSegmentId = sabredoc.DataList.SegmentDetails.Where(x => x.Status?.DeliveryStatusCode == "Delivered").Select(x => x.SegmentId);

            var flownSegments = orderdoc.DataList.SegmentDetails.Where(oldSeg => sabredoc.DataList.SegmentDetails.Where(x => flownSegmentId.Any(segId => segId.Contains(x.SegmentId))).Any(seg =>
                                                               seg.OriginAirportCode == oldSeg.OriginAirportCode &&
                                                               seg.DestinationAirportCode == oldSeg.DestinationAirportCode &&
                                                               seg.MarketingFlightNumber == oldSeg.MarketingFlightNumber &&
                                                               seg.ClassOfService == oldSeg.ClassOfService &&
                                                               Convert.ToDateTime(seg.ScheduledDepartureTime.LocalDateTime) == Convert.ToDateTime(oldSeg.ScheduledDepartureTime.LocalDateTime)
                                                               )).Select(x => x.SegmentId);
            var flownPenaltyRef = orderdoc.OrderItems.First().Services.Where(x => x.SegmentRefIds.Any(segRef => flownSegments.Contains(segRef))).Select(x => x.PenaltyRef);
            var penaltyIds = flownPenaltyRef.SelectMany(p => p.ChangePenaltyRefIds ?? Enumerable.Empty<string>()).Concat(flownPenaltyRef.SelectMany(p => p.CancellationPenaltyRefIds ?? Enumerable.Empty<string>()));
            var flownPenaltyList = orderdoc.DataList.Penalty.Where(p => penaltyIds.Contains(p.Id) &&!(p.Text?.Contains("Change Penalty") ?? false));
            var penaltyIndex = sabredoc.DataList.Penalty.Select(p => int.Parse(p.Id.Replace("PEN", string.Empty))).Max();

            var penaltiesToAdd = flownPenaltyList.Where(flownPenalty => !sabredoc.DataList.Penalty.Any(newPen =>
                                                        newPen.Type == flownPenalty.Type && newPen.Text == flownPenalty.Text &&
                                                        newPen.MaxPrice.Amount.MonetaryAmount == flownPenalty.MaxPrice.Amount.MonetaryAmount &&
                                                        newPen.MaxPrice.Amount.BaseCurrency == flownPenalty.MaxPrice.Amount.BaseCurrency ));

            IEnumerable<Penalty> newPenaltyList = Enumerable.Empty<Penalty>();
            if (penaltiesToAdd != null && penaltiesToAdd.Any())
            {
                foreach(var pen in penaltiesToAdd)
                {
                    newPenaltyList = newPenaltyList.Append(new Penalty
                    {
                        Id = $"PEN{++penaltyIndex}",
                        Type = pen.Type,
                        MaxPrice = pen.MaxPrice,
                        MinPrice = pen.MinPrice,
                        Text = pen.Text
                    });
                }
            }

            foreach(var oi in sabredoc.OrderItems.Where(oi => oi.Services.Any(srv => srv.Product != null && srv.Product.ProductType == Product.ProductTypes.FlightEnum)))
            {
                var penalties = newPenaltyList?.Any() == true ? newPenaltyList : sabredoc.DataList.Penalty;
                var matchingFlownServices = oi.Services.First(x => x.SegmentRefIds.Any(seg => flownSegments.Contains(seg)));
                matchingFlownServices.PenaltyRef = new PenaltyRef
                {
                    ChangePenaltyRefIds = penalties.Where(x => x.Type == Constants.Application.ChangePenaltyType).Select(x => x.Id).ToList(),
                    CancellationPenaltyRefIds = penalties.Where(x => x.Type == Constants.Application.CancellationPenaltyType).Select(x => x.Id).ToList()
                };
            }

            sabredoc.DataList.Penalty = sabredoc.DataList.Penalty.Concat(newPenaltyList);

            return sabredoc;
        }

        public static OfferResponse MapFlownSegmentOrderChangePenalty(OrderDocument orderdoc, OfferResponse offerDoc)
        {
            var flownSegmentId = orderdoc.DataList.SegmentDetails.Where(x => x.Status?.DeliveryStatusCode == "Delivered").Select(x => x.SegmentId);

            var flownSegment = offerDoc.DataList.FlightSegments.Where(flightSeg => orderdoc.DataList.SegmentDetails.Where(x => flownSegmentId.Any(segId => segId.Contains(x.SegmentId))).Any(seg =>
                                                               seg.OriginAirportCode == flightSeg.OriginAirportCode &&
                                                               seg.DestinationAirportCode == flightSeg.DestinationAirportCode &&
                                                               seg.MarketingFlightNumber == flightSeg.MarketingFlightNumber &&
                                                               Convert.ToDateTime(seg.ScheduledDepartureTime.LocalDateTime) == Convert.ToDateTime(flightSeg.ScheduledDepartureTime.LocalDateTime)
                                                               )).Select(x => x.SegmentId);

            var matchingOfferSegments = offerDoc.DataList.OfferSegments.Where(x => flownSegment.Contains(x.FlightSegmentRefId)).Select(x => x.OfferSegmentId); 
            var flownPenaltyRef = orderdoc.OrderItems.First().Services.Where(x => x.Product != null && x.Product.ProductType == Product.ProductTypes.FlightEnum && x.SegmentRefIds.Any(segRef => flownSegmentId.Contains(segRef))).Select(x => x.PenaltyRef);
            var penaltyIds = flownPenaltyRef.SelectMany(p => p.ChangePenaltyRefIds ?? Enumerable.Empty<string>()).Concat(flownPenaltyRef.SelectMany(p => p.CancellationPenaltyRefIds ?? Enumerable.Empty<string>()));
            var flownPenaltyList = orderdoc.DataList.Penalty.Where(p => penaltyIds.Contains(p.Id));
            var penaltyIndex = offerDoc.DataList.Penalty.Select(p => int.Parse(p.Id.Split('-')[0][3..])).Max();

            var penaltiesToAdd = flownPenaltyList.Where(flownPenalty => !offerDoc.DataList.Penalty.Any(newPen =>
                                                        newPen.Type == flownPenalty.Type && newPen.Text == flownPenalty.Text &&
                                                        newPen.MaxPrice.Amount.MonetaryAmount == flownPenalty.MaxPrice.Amount.MonetaryAmount &&
                                                        newPen.MaxPrice.Amount.BaseCurrency == flownPenalty.MaxPrice.Amount.BaseCurrency));

            IEnumerable<Penalty> newPenaltyList = Enumerable.Empty<Penalty>();
            if (penaltiesToAdd != null && penaltiesToAdd.Any())
            {
                foreach (var pen in penaltiesToAdd)
                {
                    newPenaltyList = newPenaltyList.Append(new Penalty
                    {
                        Id = $"PEN{++penaltyIndex}",
                        Type = pen.Type,
                        MaxPrice = pen.MaxPrice,
                        MinPrice = pen.MinPrice,
                        Text = pen.Text
                    });
                }
            }

            foreach (var oi in offerDoc.Offers.First().OfferItems)
            {
                var penalties = newPenaltyList?.Any() == true ? newPenaltyList : offerDoc.DataList.Penalty;
                var matchingFlownServices = oi.Services.First(x => x.ProductSegmentAssociation.OfferSegmentRefIds.Any(seg => matchingOfferSegments.Contains(seg)));
                matchingFlownServices.PenaltyRef = new PenaltyRef
                {
                    ChangePenaltyRefIds = penalties.Where(x => x.Type == Constants.Application.ChangePenaltyType).Select(x => x.Id).ToList(),
                    CancellationPenaltyRefIds = penalties.Where(x => x.Type == Constants.Application.CancellationPenaltyType).Select(x => x.Id).ToList()
                };
            }

            offerDoc.DataList.Penalty = offerDoc.DataList.Penalty.Concat(newPenaltyList);

            return offerDoc;
        }

        public static OrderDocument MapOrderItems(OrderDocument orderdoc, OrderDocument sabredoc)
        {
            foreach (var orderItem in orderdoc.OrderItems)
            {
                var orderProductType = orderItem.Services.Select(s => s.Product.ProductType).FirstOrDefault();
                var orderBeneficiaries = orderItem.Services.SelectMany(s => s.ServiceBeneficiaries).Distinct();

                var matchingSabreItem = sabredoc.OrderItems.Where(sabreItem => sabreItem.Services.Any(s => s.Product?.ProductType == orderProductType)).FirstOrDefault(sabreItem =>
                {
                    var sabreBeneficiaries = sabreItem.Services.SelectMany(s => s.ServiceBeneficiaries).Distinct();

                    return orderBeneficiaries.All(b => sabreBeneficiaries.Contains(b)) && sabreBeneficiaries.All(b => orderBeneficiaries.Contains(b));
                });

                if (matchingSabreItem != null)
                {
                    matchingSabreItem.OrderItemId = orderItem.OrderItemId;
                    matchingSabreItem.ExistingOrderItemId = orderItem.ExistingOrderItemId;
                }
            }

            return sabredoc;
        }

        public static OrderDocument CreateChildOrderDocument(SplitPnrRequest request, ChildRLOCDetails childRLOCDetails)
        {
            var childOrderDoc = Json.Deserialize<OrderDocument>(Json.Serialize(request.OrderDocument));

            childOrderDoc.DataList.CustomerDetails = childOrderDoc.DataList.CustomerDetails.Where(cd => request.splitPaxId.Contains(cd.CustId));

            if (childOrderDoc.DataList.OrderInstructions.Any())
            {
                childOrderDoc.DataList.OrderInstructions = childOrderDoc.DataList.OrderInstructions.Where(oi => request.splitPaxId.Contains(oi.ItemText.Split('/')[1]));
            }

            var childPnr = childRLOCDetails.ChildRLOC;
            var childDeltaPnr = childRLOCDetails.ChildExternalRLOC;

            string orderId = $"VS932{childPnr}{DateTime.UtcNow:yy}";
            childOrderDoc.OrderNumber.OrderId = orderId;
            childOrderDoc.Id = orderId;
            childOrderDoc.PartitionKey = orderId;
            childOrderDoc.BookingReference = new BookingReference()
            {
                RLOC = childPnr,
                ExternalRecordLocator = childDeltaPnr,
                ExternalSystem = "1S"
            };

            var parentPaxIds = request.OrderDocument.DataList.CustomerDetails?.Where(cd => !request.splitPaxId.Contains(cd.CustId)).Select(cd => cd.CustId);

            foreach (var parentPax in parentPaxIds)
            {
                foreach (var orderItem in childOrderDoc.OrderItems.Where(x => x.Services.SelectMany(x => x.ServiceBeneficiaries).Contains(parentPax)))
                {
                    foreach (var service in orderItem.Services)
                    {
                        if (service.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.FlightEnum)
                        {
                            if (service.ServiceBeneficiaries.Count() > 1)
                            {
                                service.ServiceBeneficiaries = service.ServiceBeneficiaries.Where(sb => !sb.Is(parentPax));
                            }
                            else
                            {
                                childOrderDoc.OrderItems = childOrderDoc.OrderItems.Where(oi => !oi.OrderItemId.Is(orderItem.OrderItemId));
                            }
                        }

                        else if (service.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum)
                        {
                            childOrderDoc.OrderItems = childOrderDoc.OrderItems.Where(oi => !oi.OrderItemId.Is(orderItem.OrderItemId));
                        }

                        else
                        {
                            orderItem.Services = orderItem.Services.Where(s => !s.ServiceBeneficiaries.Contains(parentPax));
                        }
                    }

                    orderItem.AccountableDocumentRefs = orderItem.AccountableDocumentRefs?.Where(a => !a.CustomerRefId.Contains(parentPax));
                }

            }

            var baggageIds = childOrderDoc.OrderItems.SelectMany(oi => oi.Services).Where(s => s?.BaggageRefIds != null).SelectMany(service => service.BaggageRefIds).Distinct();

            childOrderDoc.DataList.BaggageAllowances = childOrderDoc.DataList.BaggageAllowances?.Where(ba => baggageIds.Contains(ba.BaggageAllowanceId));

            int index = 1;

            foreach (var orderItem in childOrderDoc.OrderItems)
            {
                orderItem.OrderItemId = $"{orderId}-{index++}";

            }

            return childOrderDoc;
        }

        public static OrderDocument UpdateChildOrderDoc(OrderDocument orderDoc, IEnumerable<PaymentDocument> paymentDocs)
        {
            var paymentDocTransactionIds = paymentDocs.Select(paymentDoc => paymentDoc.TransactionId.Split('-')[0]);

            orderDoc.OrderItems = orderDoc.OrderItems.Select(oi =>
            {
                if (oi.PaymentDetails != null)
                {
                    oi.PaymentDetails = oi.PaymentDetails.Select(pd =>
                    {
                        var matchingTransactionId = paymentDocTransactionIds.FirstOrDefault(tid => pd.PaymentReferenceId != null && pd.PaymentReferenceId.Contains(tid));

                        if (!string.IsNullOrEmpty(matchingTransactionId))
                        {
                            var paymentDoc = paymentDocs.First(doc => doc.TransactionId.Split('-')[0] == matchingTransactionId);
                            
                                pd.PaymentReferenceId = paymentDoc.TransactionId;
                                return pd;                          
                        }
                        return null;
                    }).Where(pd => pd != null);
                }
                return oi;
            });
            return orderDoc;
        }

        public static OrderDocument UpdateExchangeFOPPaymentDetails(OrderDocument orderDocumentRS, [Optional] PaymentRS paymentIFGAuthorizeRS, [Optional] PaymentDocument paymentDocument, [Optional] OrderRS orderDataRS, [Optional] PaymentRS paymentIFGOrderExchangeRS, [Optional] bool isEasyPayment, [Optional] OrderDocument orderManagementNameCorrectionResponse, [Optional] PaymentRS paymentMSCaptureRS, [Optional] bool isCardPayment, [Optional] bool isCashPayment ,[Optional] TransactionServiceResponse transactionServiceResponse, [Optional] CardInfo easyPayCardMethod)
        {
            var paymentResponse = paymentMSCaptureRS?.PaymentDocument ?? paymentDocument;

            var latestSequenceId = Convert.ToInt32(orderDataRS.OrderDocument.OrderItems.FirstOrDefault()?.PaymentDetails.OrderByDescending(x => x.PaymentCommitmentDateTime).FirstOrDefault()?.SequenceId);
            orderDocumentRS.OrderItems = orderDocumentRS.OrderItems.Select(oi =>
            {
                if(oi.Services.Any(srv => srv?.Product?.ProductType == Product.ProductTypes.SeatEnum)) return oi;

                if (oi.PaymentDetails != null && oi.PaymentDetails.Any() && orderManagementNameCorrectionResponse == null)
                {
                    if (paymentResponse != null)
                    {
                        oi.PaymentDetails = orderDataRS.OrderDocument.OrderItems.FirstOrDefault()?.PaymentDetails.Append(new EAPI.Domain.Virgin.Models.Payment.PaymentDetails()
                        {
                            PaymentReferenceId = paymentResponse.TransactionId,
                            PaymentCommitmentDateTime = paymentResponse.PaymentCommitedDateTime,
                            TrackId = !string.IsNullOrEmpty(paymentIFGAuthorizeRS?.PaymentDocument?.TrackId)? paymentIFGAuthorizeRS.PaymentDocument.TrackId: transactionServiceResponse?.RequestId,
                            ApprovalCode = string.IsNullOrEmpty(paymentIFGAuthorizeRS?.PaymentDocument?.ApprovalCode) ? null : paymentIFGAuthorizeRS.PaymentDocument.ApprovalCode,
                            Amount = new EAPI.Domain.Virgin.Models.Common.CurrencyAmount()
                            {
                                MonetaryAmount = paymentResponse.Amount.ToString(),
                                BaseCurrency = oi.PriceDetails.First().Amount.BaseCurrency
                            },
                            CardMethod = paymentMSCaptureRS == null && isEasyPayment ? new() { CardNumber = easyPayCardMethod.CardNumber } : paymentMSCaptureRS == null ? null : new()
                            {
                                CardNumber = paymentResponse.MaskedCardNumber,
                                CardBrandCode = paymentResponse.CardType
                            },
                            PaxRefIds = orderDataRS.OrderDocument.DataList.CustomerDetails.Select(x => x.CustId),
                            SequenceId = Convert.ToString(latestSequenceId + 1),
                            PaymentProcessingStatus = transactionServiceResponse?.ErrorDetails != null ?  EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.PENDING : EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.SUCCESSFUL,
                            FormOfPayments = isEasyPayment ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay : (EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum.ToString() == paymentResponse.FormOfPayment ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum : null)
                        });
                    }
                    else
                    {
                        var freePaymentFuncs = oi.PaymentDetails?.FirstOrDefault(x => x.Amount?.MonetaryAmount == "0.00");

                        oi.PaymentDetails = orderDataRS.OrderDocument.OrderItems.FirstOrDefault()?.PaymentDetails.Append(new EAPI.Domain.Virgin.Models.Payment.PaymentDetails()
                        {
                            PaymentReferenceId = freePaymentFuncs.PaymentReferenceId,
                            Amount = freePaymentFuncs.Amount,
                            PaymentCommitmentDateTime = freePaymentFuncs.PaymentCommitmentDateTime,
                            TrackId = string.IsNullOrEmpty(paymentIFGAuthorizeRS?.PaymentDocument?.TrackId) ? (!string.IsNullOrEmpty(paymentIFGOrderExchangeRS?.PaymentDocument?.TrackId) ? paymentIFGOrderExchangeRS.PaymentDocument.TrackId: transactionServiceResponse?.RequestId) : paymentIFGAuthorizeRS.PaymentDocument.TrackId,
                            ApprovalCode = string.IsNullOrEmpty(paymentIFGAuthorizeRS?.PaymentDocument?.ApprovalCode) ? null : paymentIFGAuthorizeRS?.PaymentDocument?.ApprovalCode,
                            PaymentProcessingStatus = (transactionServiceResponse == null) ? freePaymentFuncs.PaymentProcessingStatus :(transactionServiceResponse?.ErrorDetails == null) ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.SUCCESSFUL : EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.PENDING,
                            PaxRefIds = orderDataRS.OrderDocument.DataList.CustomerDetails.Select(x => x.CustId),
                            SequenceId = Convert.ToString(latestSequenceId + 1),
                            FormOfPayments = isEasyPayment ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay : (isCashPayment ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum : isCardPayment ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum : null)
                        });
                    }
                }
                else
                {
                    oi.PaymentDetails = orderDataRS?.OrderDocument?.OrderItems?.FirstOrDefault().PaymentDetails.Append(new EAPI.Domain.Virgin.Models.Payment.PaymentDetails()
                    {
                        TransactionType = "NameCorrection",
                        TrackId =  (transactionServiceResponse != null) ? transactionServiceResponse.RequestId : paymentIFGOrderExchangeRS?.PaymentDocument?.TrackId ,
                        PaymentCommitmentDateTime = DateTime.UtcNow.ToString(Constants.Application.InputDateTimeFormat),
                        PaxRefIds = orderManagementNameCorrectionResponse.DataList.CustomerDetails.Select(x => x.CustId),
                        SequenceId = Convert.ToString(latestSequenceId + 1)
                    });
                }
                return oi;
            });

            return orderDocumentRS;
        }

        public static OrderDocument UpdateRefundFOPPaymentDetails(OrderDocument orderDocumentRS, [Optional] PaymentDocument paymentDocument, [Optional] OrderRS orderDataRS, [Optional] bool IsEasyPay, [Optional] PaymentRS paymentIFGOrderExchangeRS, [Optional] PaymentRS refundPaymentRS, [Optional] TransactionServiceResponse transactionServiceResponse, [Optional] string easyPayApprovalCode, [Optional] string easyPayCardNumber)
        {
            var paymentResponse = refundPaymentRS?.PaymentDocument ?? paymentDocument;

            var latestCancelSeqeunceId = Convert.ToInt32(orderDocumentRS.OrderItems.First().PaymentDetails.OrderByDescending(x => x.PaymentCommitmentDateTime).FirstOrDefault().SequenceId);
            var latestRefundSeqeunceId = Convert.ToInt32(orderDataRS?.OrderDocument?.OrderItems?.FirstOrDefault()?.PaymentDetails?.OrderByDescending(x => x.PaymentCommitmentDateTime)?.FirstOrDefault()?.SequenceId);

            orderDocumentRS.OrderItems = orderDocumentRS.OrderItems.Select(oi =>
            {
                if (oi.PaymentDetails != null && oi.PaymentDetails.Any())
                {
                    if (orderDataRS != null)
                    {
                        oi.PaymentDetails = orderDataRS.OrderDocument.OrderItems.FirstOrDefault()?.PaymentDetails.Append(new EAPI.Domain.Virgin.Models.Payment.PaymentDetails()
                        {
                            PaymentReferenceId = paymentResponse.TransactionId,
                            PaymentCommitmentDateTime = paymentResponse.PaymentCommitedDateTime,
                            Amount = new EAPI.Domain.Virgin.Models.Common.CurrencyAmount()
                            {
                                MonetaryAmount = refundPaymentRS == null ? paymentResponse.Amount.ToString() : "-" + paymentResponse.Amount.ToString(),
                                BaseCurrency = oi.PriceDetails.First().Amount.BaseCurrency
                            },
                            CardMethod = refundPaymentRS == null ? null : new()
                            {
                                CardNumber = paymentResponse.MaskedCardNumber,
                                CardBrandCode = paymentResponse.CardType
                            },
                            PaxRefIds = orderDataRS.OrderDocument.DataList.CustomerDetails.Select(x => x.CustId),
                            SequenceId = Convert.ToString(latestRefundSeqeunceId + 1),
                            PaymentProcessingStatus = ((refundPaymentRS != null && transactionServiceResponse?.ErrorDetails == null) ||  (paymentResponse.FormOfPayment == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum.ToString()&& transactionServiceResponse?.ErrorDetails == null) )? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.SUCCESSFUL: EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.PENDING,
                            TrackId = transactionServiceResponse?.RequestId ?? paymentIFGOrderExchangeRS?.PaymentDocument?.TrackId?? null,
                            ApprovalCode = IsEasyPay ? oi.PaymentDetails.First().ApprovalCode : null,
                            FormOfPayments = IsEasyPay ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay : EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum.ToString() == paymentResponse.FormOfPayment ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum : (refundPaymentRS != null) ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum : null
                        });
                    }
                    else
                    {
                        oi.PaymentDetails = oi.PaymentDetails.Where(x => x.PaymentReferenceId != paymentResponse.TransactionId).Append(new EAPI.Domain.Virgin.Models.Payment.PaymentDetails()
                        {
                            PaymentReferenceId = paymentResponse.TransactionId,
                            PaymentCommitmentDateTime = paymentResponse.PaymentCommitedDateTime,
                            Amount = new EAPI.Domain.Virgin.Models.Common.CurrencyAmount()
                            {
                                MonetaryAmount = refundPaymentRS == null ? paymentResponse.Amount.ToString() : "-" + paymentResponse.Amount.ToString(),
                                BaseCurrency = oi.PriceDetails.First().Amount.BaseCurrency
                            },
                            CardMethod = IsEasyPay && easyPayCardNumber != null ? new() { CardNumber = easyPayCardNumber } : refundPaymentRS == null ? null : new()
                            {
                                CardNumber = paymentResponse.MaskedCardNumber,
                                CardBrandCode = paymentResponse.CardType
                            },
                            PaxRefIds = orderDocumentRS.DataList.CustomerDetails.Select(x => x.CustId),
                            SequenceId = Convert.ToString(latestCancelSeqeunceId + 1),
                            PaymentProcessingStatus = ((refundPaymentRS != null && transactionServiceResponse?.ErrorDetails == null) || (paymentResponse.FormOfPayment == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum.ToString() && transactionServiceResponse?.ErrorDetails == null)) ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.SUCCESSFUL : EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.PENDING,
                            TrackId = transactionServiceResponse?.RequestId ?? paymentIFGOrderExchangeRS?.PaymentDocument?.TrackId ?? null,
                            ApprovalCode = IsEasyPay && easyPayApprovalCode != null ? easyPayApprovalCode : IsEasyPay ? oi.PaymentDetails.First().ApprovalCode : null,
                            FormOfPayments = IsEasyPay ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay : EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum.ToString() == paymentResponse.FormOfPayment ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum : null
                        });
                    }
                }
                return oi;
            });
            return orderDocumentRS;
        }

        public static OrderDocument UpdateFopPaymentDetails(OrderDocument orderMgmtRS, [Optional] PaymentRS paymentIFGAuthorizeRS, [Optional] PaymentDocument paymentDocument, [Optional] bool isEasyPay, [Optional] PaymentRS capturePaymentRS, [Optional] PaymentRS ifgOrderPaymentRS, [Optional] bool isCardPayment, [Optional] TransactionServiceResponse transactionServiceResponse, [Optional] CardInfo easyPayCardMethod)
        {
            var paymentResponse = capturePaymentRS?.PaymentDocument ?? paymentDocument;

            orderMgmtRS.OrderItems = orderMgmtRS.OrderItems.Select(oi =>
            {
                if (oi.Services.Any(srv => srv.Product?.ProductType == Product.ProductTypes.SeatEnum)) return oi;

                if (paymentResponse != null)
                {
                    oi.PaymentDetails =
                    [
                        new()
                        {
                            PaymentReferenceId = paymentResponse.TransactionId,
                            PaymentCommitmentDateTime = paymentResponse.PaymentCommitedDateTime,
                            TrackId = transactionServiceResponse?.RequestId ?? paymentIFGAuthorizeRS?.PaymentDocument?.TrackId ?? null,                            
                            ApprovalCode = string.IsNullOrEmpty(paymentIFGAuthorizeRS?.PaymentDocument?.ApprovalCode) ? null : paymentIFGAuthorizeRS.PaymentDocument.ApprovalCode,
                            Amount = new EAPI.Domain.Virgin.Models.Common.CurrencyAmount()
                            {
                                MonetaryAmount = paymentResponse.Amount.ToString(),
                                BaseCurrency = oi.PriceDetails.First().Amount.BaseCurrency
                            },
                            CardMethod = capturePaymentRS == null && isEasyPay ? new () { CardNumber = easyPayCardMethod?.CardNumber } : capturePaymentRS == null ? null : new()
                            {
                                CardNumber = paymentResponse.MaskedCardNumber,
                                CardBrandCode = paymentResponse.CardType
                            },
                            PaymentProcessingStatus = (ifgOrderPaymentRS?.Errors != null || transactionServiceResponse?.ErrorDetails !=null) ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.PENDING : EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.SUCCESSFUL,
                            PaxRefIds = orderMgmtRS.DataList.CustomerDetails.Select(x => x.CustId),
                            SequenceId = oi.PaymentDetails?.FirstOrDefault()?.SequenceId == null ? "1" : Convert.ToString(Convert.ToInt32(oi.PaymentDetails.OrderBy(x => x.SequenceId).Last().SequenceId) + 1),
                            FormOfPayments = capturePaymentRS == null && isEasyPay ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay : (paymentResponse.FormOfPayment == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum.ToString() ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum: capturePaymentRS != null ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum : null)
                        }
                        ];
                }
                else
                {
                    var paymentFunc = oi.PaymentDetails?.FirstOrDefault(item => item?.Amount?.MonetaryAmount == "0.00");
                    oi.PaymentDetails =
                    [
                        new()
                        {
                            PaymentReferenceId = paymentFunc.PaymentReferenceId,
                            Amount = paymentFunc.Amount,
                            PaymentCommitmentDateTime = paymentFunc.PaymentCommitmentDateTime,
                            TrackId = string.IsNullOrEmpty(paymentIFGAuthorizeRS?.PaymentDocument?.TrackId) ? null : paymentIFGAuthorizeRS.PaymentDocument.TrackId,
                            ApprovalCode = string.IsNullOrEmpty(paymentIFGAuthorizeRS?.PaymentDocument?.ApprovalCode) ? null : paymentIFGAuthorizeRS.PaymentDocument.ApprovalCode,
                            CardMethod = capturePaymentRS == null ? null : new()
                            {
                                CardNumber = paymentResponse.MaskedCardNumber,
                                CardBrandCode = paymentResponse.CardType
                            },
                            PaymentProcessingStatus = paymentFunc.PaymentProcessingStatus,
                            FormOfPayments = isEasyPay ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay : (paymentResponse?.FormOfPayment == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum.ToString() ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum: isCardPayment ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum : null)
                        }
                    ];
                }

                return oi;
            });
            return orderMgmtRS;
        }

        public static OrderDocument CreateParentOrderDocument(SplitPnrRequest request, BookingReference bookingReference)
        {
            var parentOrderDoc = Json.Deserialize<OrderDocument>(Json.Serialize(request.OrderDocument));
            var childRlocDetails = bookingReference?.SplitRLOC?.ChildRLOCDetails?.First();

            parentOrderDoc.DataList.CustomerDetails = parentOrderDoc.DataList.CustomerDetails.Where(cd => !request.splitPaxId.Contains(cd.CustId));

            foreach (var splitpax in request.splitPaxId)
            {
                foreach (var orderItem in parentOrderDoc.OrderItems.Where(x => x.Services.SelectMany(x => x.ServiceBeneficiaries).Contains(splitpax)))
                {
                    foreach (var service in orderItem.Services)
                    {
                        if (service.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.FlightEnum)
                        {
                            if (service.ServiceBeneficiaries.Count() > 1)
                            {
                                service.ServiceBeneficiaries = service.ServiceBeneficiaries.Where(sb => !sb.Is(splitpax));
                            }
                            else
                            {
                                parentOrderDoc.OrderItems = parentOrderDoc.OrderItems.Where(oi => !oi.OrderItemId.Is(orderItem.OrderItemId));
                            }
                        }
                        else if (service.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum)
                        {
                            parentOrderDoc.OrderItems = parentOrderDoc.OrderItems.Where(oi => !oi.OrderItemId.Is(orderItem.OrderItemId));
                        }
                        else
                        {
                            orderItem.Services = orderItem.Services.Where(s => !s.ServiceBeneficiaries.Contains(splitpax));
                        }
                    }
                    orderItem.AccountableDocumentRefs = orderItem.AccountableDocumentRefs?.Where(a => !a.CustomerRefId.Contains(splitpax));
                }
            }

            if (parentOrderDoc.DataList.OrderInstructions.Any())
            {
                parentOrderDoc.DataList.OrderInstructions = parentOrderDoc.DataList.OrderInstructions.Where(oi => !request.splitPaxId.Contains(oi.ItemText.Split('/')[1]));
            }

            var airSegment = bookingReference.ExternalRecordLocator;
            var childRlocDetail = new ChildRLOCDetails
            {
                ChildRLOC = childRlocDetails.ChildRLOC,
                ChildExternalRLOC = airSegment,
                ChildRLOCLink = childRlocDetails.ChildRLOCLink,
                SplitDateTime = childRlocDetails.SplitDateTime
            };

            parentOrderDoc.BookingReference.SplitRLOC ??= new SplitRLOC
            {
                ChildRLOCDetails = []
            };

            parentOrderDoc.BookingReference.SplitRLOC.ChildRLOCDetails = parentOrderDoc.BookingReference.SplitRLOC.ChildRLOCDetails.Concat([childRlocDetail]);

            var baggageIds = parentOrderDoc.OrderItems.SelectMany(oi => oi.Services).Where(s => s?.BaggageRefIds != null).SelectMany(service => service.BaggageRefIds).Distinct();
            parentOrderDoc.DataList.BaggageAllowances = parentOrderDoc.DataList.BaggageAllowances?.Where(ba => baggageIds.Contains(ba.BaggageAllowanceId));

            var orderItemId = parentOrderDoc.Id;

            parentOrderDoc.OrderItems = parentOrderDoc.OrderItems.Select((orderItem, i) =>
            {
                orderItem.OrderItemId = $"{orderItemId}-{i + 1}";
                return orderItem;
            }).ToArray();


            return parentOrderDoc;
        }

        public static OrderDocument UpdateEmailDetails(OrderDocument orderDocument, IATA_OrderChangeRQ request, AugmentationPoint augmentationPoint)
        {
            var customers = orderDocument.DataList.CustomerDetails;
            var contactInfoList = request?.Request?.DataLists?.ContactInfoList?.Select(x => new { x.ContactInfoID, x.EmailAddress.First().EmailAddressText });
            var paxDetails = request.Request?.DataLists?.PaxList?.Select(x => new { x.ContactInfoRefID, x.PaxID });

            if (paxDetails != null)
            {
                foreach (var pax in paxDetails)
                {
                    var matchedContactInfo = contactInfoList?.FirstOrDefault(c => c.ContactInfoID == pax.ContactInfoRefID);

                    if (matchedContactInfo != null)
                    {
                        var customer = customers.FirstOrDefault(c => c.CustId == pax.PaxID);

                        if (customer != null && (customer.Contacts == null))
                        {
                            customer.IsEmailUpdates = true;
                            customer.Contacts = new Contacts()
                            {
                                Email = matchedContactInfo.EmailAddressText
                            };
                        }
                    }
                }
            }

            return orderDocument;
        }

        public static OrderDocument UpdateOrderHistoryWithChanges(OrderDocument oldOrderDoc, OrderDocument historyOrderDoc)
        {
            if (historyOrderDoc != null)
            {
                var latestHistory = historyOrderDoc.OrderHistory?.OrderBy(x => x.Version).Last();

                if (Convert.ToInt32(latestHistory.Version) != oldOrderDoc.Version)
                {
                    var updatedHistory = new OrderHistory()
                    {
                        DataList = oldOrderDoc.DataList,
                        OrderItems = oldOrderDoc.OrderItems,
                        OrderStatus = oldOrderDoc.OrderStatus,
                        Version = latestHistory != null ? Convert.ToString(Convert.ToInt32(latestHistory.Version) + 1) : "1",
                        ChangeType = oldOrderDoc.ChangeType,
                        ChangeDateTime = oldOrderDoc.ChangeDateTime.UtcDateTime,
                        Warnings = (oldOrderDoc.Warnings?.Any() ?? false) ? oldOrderDoc.Warnings : null
                    };

                    historyOrderDoc.OrderHistory = historyOrderDoc.OrderHistory.Append(updatedHistory);
                }
            }
            else
            {
                historyOrderDoc = new OrderDocument()
                {
                    DocType = oldOrderDoc.DocType,
                    FareTypeRequest = oldOrderDoc.FareTypeRequest,
                    PointOfSale = oldOrderDoc.PointOfSale,
                    CreationDateTime = oldOrderDoc.CreationDateTime,
                    BookingReference = oldOrderDoc.BookingReference,
                    OrderNumber = oldOrderDoc.OrderNumber,
                    Parties = oldOrderDoc.Parties,
                    TimeLimits = oldOrderDoc.TimeLimits,
                    OrderHistory = [new OrderHistory()
                    {
                        DataList = oldOrderDoc.DataList,
                        OrderItems = oldOrderDoc.OrderItems,
                        Version = "1",
                        OrderStatus = oldOrderDoc.OrderStatus,
                        ChangeType = ChangeTypeEnum.OrderItemAdded,
                        ChangeDateTime = oldOrderDoc.ChangeDateTime.UtcDateTime,
                        Warnings = (oldOrderDoc.Warnings?.Any() ?? false) ? oldOrderDoc.Warnings : null

                    }],
                    Id = $"{oldOrderDoc.Id}-History",
                    PartitionKey = $"{oldOrderDoc.Id}-History",
                };
            }
            return historyOrderDoc;

        }

        public static OrderDocument UpdateSeatProductDetails(OrderDocument orderDocument, SeatRS seatOfferDocument)
        {
            var seatCatalogues = seatOfferDocument?.SeatAvailabilityResponse?.FlightSegmentSeatMap?
                                .SelectMany(f => f.Catalogue ?? []);

            var seatAttributeByIdentifier = seatOfferDocument?.SeatAvailabilityResponse?.FlightSegmentSeatMap?
                                .SelectMany(f => (f.Seatmap ?? [])
                                .SelectMany(s => s.SeatRows?.Where(sr => sr.RowNumber != null) ?? [])
                                .SelectMany(r => r.SeatColumns?.Where(sc => sc.Seat) ?? [])
                                .Where(c => c.Attributes?.Any() == true)
                                .Select(sc => new
                                {
                                    SegmentNumber = f.FlightSegment.SegmentId,
                                    SeatIdentifier = sc.SeatIdentifer,
                                    seatCatalogues?.FirstOrDefault(a => a.CatalogueId == sc.CatalogueRefIds?.First())?.ProductName
                                }))
                                .ToDictionary(
                                    x => (Int32.Parse(x.SegmentNumber), x.SeatIdentifier),
                                    x => x.ProductName
                                );

            foreach (var orderItem in orderDocument.OrderItems.Where(a => a.Services.First().Product != null && a.Services.First().Product.ProductType == Product.ProductTypes.SeatEnum))
            {
                var service = orderItem.Services.First();
                var segmentId = Int32.Parse(service.SegmentRefIds?.FirstOrDefault().Replace("SEG", ""));

                var currentSeatAssignment = service.CurrentSeatAssignment;

                if (segmentId != 0 && currentSeatAssignment != null && seatAttributeByIdentifier.TryGetValue((segmentId, currentSeatAssignment), out var productName))
                {
                    service.Product.ProductName = productName;
                }
            }

            return orderDocument;
        }

        public static OrderDocument UpdateSeatPaymentDetails(OrderDocument orderDocument, PaymentRS paymentMsCaptureRsForSeat)
        {

            foreach (var orderItems in orderDocument.OrderItems.Where(x => x.PaymentDetails != null))
            {
                foreach (var paymentDetails in orderItems.PaymentDetails)
                {
                    if (paymentDetails.PaymentReferenceId == paymentMsCaptureRsForSeat.PaymentDocument.TransactionId)
                    {
                        paymentDetails.PaymentCommitmentDateTime = paymentMsCaptureRsForSeat.PaymentDocument.PaymentCommitedDateTime;
                        paymentDetails.FormOfPayments = EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum;
                        paymentDetails.PaymentProcessingStatus = EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.SUCCESSFUL;
                        paymentDetails.CardMethod = new()
                        {
                            CardNumber = paymentMsCaptureRsForSeat.PaymentDocument.MaskedCardNumber,
                            CardBrandCode = paymentMsCaptureRsForSeat.PaymentDocument.CardType
                        };
                    }
                }
            }

            return orderDocument;
        }

        public static OrderDocument UpdateHoldBookingPaymentDetails(OrderDocument getReservationOrderDoc, PaymentMSAuthorizeCommand paymentAuthorizeReq, PaymentRS paymentAuthorizeRes, PaymentRS paymentCaptureRes)
        {
            var latestSequenceId = Convert.ToInt32(getReservationOrderDoc.OrderItems.FirstOrDefault()?.PaymentDetails?.OrderByDescending(x => x.PaymentCommitmentDateTime)?.FirstOrDefault()?.SequenceId);

            foreach (var orderitems in getReservationOrderDoc.OrderItems)
            {
                var paymentDetails = new EAPI.Domain.Virgin.Models.Payment.PaymentDetails()
                {
                    PaymentReferenceId = paymentAuthorizeRes.PaymentDocument.TransactionId,
                    PaymentCommitmentDateTime = paymentCaptureRes != null ? paymentCaptureRes.PaymentDocument.PaymentCommitedDateTime : DateTime.UtcNow.ToString(Constants.Application.InputDateTimeFormat),
                    PaymentProcessingStatus = paymentCaptureRes == null ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.FAILED : EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.SUCCESSFUL,
                    CardMethod = new()
                    {
                        CardNumber = paymentAuthorizeReq.PaymentDetails.FirstOrDefault()?.CardMethod.CardNumber,
                        CardBrandCode = paymentAuthorizeReq.PaymentDetails.FirstOrDefault()?.CardMethod.CardType
                    },
                    PaxRefIds = getReservationOrderDoc.DataList.CustomerDetails.Select(x => x.CustId),
                    FormOfPayments = EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum,
                    Amount = new EAPI.Domain.Virgin.Models.Common.CurrencyAmount()
                    {
                        MonetaryAmount = paymentAuthorizeReq.PaymentDetails.FirstOrDefault()?.Amount.MonetaryAmount,
                        BaseCurrency = paymentAuthorizeReq.PaymentDetails.FirstOrDefault()?.Amount.BaseCurrency
                    },
                    SequenceId = orderitems.PaymentDetails?.FirstOrDefault()?.SequenceId == null ? "1" : Convert.ToString(latestSequenceId + 1)
                };

                orderitems.PaymentDetails = (orderitems.PaymentDetails ?? Enumerable.Empty<EAPI.Domain.Virgin.Models.Payment.PaymentDetails>()).Append(paymentDetails);
            }
            ;

            if (paymentCaptureRes == null)
            {
                var warningList = new List<Warning>
                {
                    new Warning()
                    {
                        Code = "911",
                        Message = "Capture payment failed due to a processing error"
                    }
                };

                getReservationOrderDoc.Warnings = warningList;
            }
            return getReservationOrderDoc;
        }

        public static CancelReshopResponse CancelFlowCurrencyConversion(CancelReshopResponse offerDoc, string currencyExchangeValue)
        {
            var exchangeObj = JsonConvert.DeserializeObject<dynamic>(currencyExchangeValue);
            decimal exchangeRate = (decimal)exchangeObj.usdToGbpExchangeRate;

            foreach (var offer in offerDoc.Offers.Where(offer => offer != null))
            {
                foreach (var offerItem in offer.OfferItems)
                {
                    foreach (var pricing in offerItem?.OfferitemPricing ?? Enumerable.Empty<VAA.EAPI.Domain.Virgin.Models.Offer.OfferitemPricing>())
                    {
                        OfferDocCurrencyConversion(pricing.SalePrice, exchangeRate);
                        OfferDocCurrencyConversion(pricing.SalePriceDifference, exchangeRate);
                        OfferDocCurrencyConversion(pricing.OriginalSalePrice, exchangeRate);
                        if (pricing.RefundPrice.Any())
                        {
                            if (pricing.RefundPrice.First().Amount != null && pricing.RefundPrice.First().Amount.BaseCurrency == "USD" && decimal.TryParse(pricing.RefundPrice.First().Amount.MonetaryAmount, out var usdAmount))
                            {
                                var amount = Math.Round(usdAmount * exchangeRate, 2).ToString("0.00");

                                pricing.RefundPrice.First().Amount.EquivalentMonetaryAmount = amount;
                                pricing.RefundPrice.First().Amount.EquivalentBaseCurrency = "GBP";
                            }
                        }
                    }
                }
            }

            if(offerDoc.DataList?.Penalty != null)
            {
                foreach(var penalty in offerDoc.DataList.Penalty)
                {
                    penalty.MaxPrice.Amount.EquivalentMonetaryAmount = Math.Round(decimal.Parse(penalty.MaxPrice.Amount.MonetaryAmount) * exchangeRate, 2).ToString("0.00"); 
                    penalty.MaxPrice.Amount.EquivalentBaseCurrency = "GBP";
                }

            }

            return offerDoc;
        }

        public static OfferResponse CurrencyConversion(OfferResponse offerDoc, string currencyExchangeValue)
        {
            var exchangeObj = JsonConvert.DeserializeObject<dynamic>(currencyExchangeValue);
            decimal exchangeRate = (decimal)exchangeObj.usdToGbpExchangeRate;

            foreach (var offer in offerDoc.Offers.Where(offer => offer != null))
            {
                foreach (var offerItem in offer.OfferItems)
                {
                    foreach (var pricing in offerItem?.OfferitemPricing ?? Enumerable.Empty<VAA.EAPI.Domain.Virgin.Models.Offer.OfferitemPricing>())
                    {
                        OfferDocCurrencyConversion(pricing.SalePrice ?? Enumerable.Empty<Price>(), exchangeRate);
                        OfferDocCurrencyConversion(pricing.SalePriceDifference ?? Enumerable.Empty<Price>(), exchangeRate);
                        OfferDocCurrencyConversion(pricing.OriginalSalePrice ?? Enumerable.Empty<Price>(), exchangeRate);
                        if (pricing?.AddCollectPrice != null)
                        {
                            pricing.AddCollectPrice.Amount.EquivalentBaseCurrency = "GBP";
                            pricing.AddCollectPrice.Amount.EquivalentMonetaryAmount = Math.Round(decimal.Parse(pricing.AddCollectPrice.Amount.MonetaryAmount) * exchangeRate, 2).ToString("0.00");
                        }
                        if(pricing?.RefundPrice != null)
                        {
                            if (pricing.RefundPrice.First().Amount != null && pricing.RefundPrice.First().Amount.BaseCurrency == "USD" && decimal.TryParse(pricing.RefundPrice.First().Amount.MonetaryAmount, out var usdAmount))
                            {
                                var amount = Math.Round(usdAmount * exchangeRate, 2).ToString("0.00");
                                pricing.RefundPrice.First().Amount.EquivalentMonetaryAmount = amount;
                                pricing.RefundPrice.First().Amount.EquivalentBaseCurrency = "GBP";
                            }
                        }
                    }
                }
            }

            if (offerDoc?.DataList?.Penalty != null)
            {
                foreach (var penalty in offerDoc.DataList.Penalty)
                {
                    if (penalty?.MaxPrice?.Amount?.MonetaryAmount != null)
                    {
                        penalty.MaxPrice.Amount.EquivalentMonetaryAmount =Math.Round(decimal.Parse(penalty.MaxPrice.Amount.MonetaryAmount) * exchangeRate, 2).ToString("0.00");
                    }
                   
                    if (penalty?.MaxPrice?.Amount != null)
                    {
                        penalty.MaxPrice.Amount.EquivalentBaseCurrency = "GBP";
                    }
                }
            }

            return offerDoc;
        }

        public static void OfferDocCurrencyConversion(IEnumerable<Price> prices, decimal rate)
        {
            foreach (var price in prices.Where(p => p != null))
            {
                if (price.Amount != null && price.Amount.BaseCurrency == "USD" && decimal.TryParse(price.Amount.MonetaryAmount, out var usdAmount))
                {
                    var amount = Math.Round(usdAmount * rate, 2).ToString("0.00");

                    price.Amount.EquivalentMonetaryAmount = amount;
                    price.Amount.EquivalentBaseCurrency = "GBP";
                }
                if (price?.BaseCurrencyAmount?.Amount != null && price?.BaseCurrencyAmount?.Currency == "USD" && decimal.TryParse(price.BaseCurrencyAmount.Amount, out var baseAmount))
                {
                    price.BaseCurrencyAmount.EquivalentAmount = Math.Round(baseAmount * rate, 2).ToString("0.00");
                    price.BaseCurrencyAmount.EquivalentCurrency = "GBP";
                }

                if (price.TaxBreakup != null)
                {
                    price.EquivalentTaxBreakup = price.TaxBreakup.Where(t => t?.Amount != null).Select(tax =>
                    {
                        var newTax = new Tax
                        {
                            Code = tax.Code,
                            Name = tax.Name,
                            Amount = new()
                        };

                        if (decimal.TryParse(tax.Amount.MonetaryAmount, out var originalAmount))
                        {

                            newTax.Amount.MonetaryAmount = tax.Amount.MonetaryAmount;
                            newTax.Amount.BaseCurrency = tax.Amount.BaseCurrency;

                            if (tax.Amount.BaseCurrency == "USD")
                            {
                                var converted = Math.Round(originalAmount * rate, 2).ToString("0.00");
                                newTax.Amount.EquivalentMonetaryAmount = converted;
                                newTax.Amount.EquivalentBaseCurrency = "GBP";
                            }
                        }

                        return newTax;
                    });
                }
            }
        }

        public static OrderDocument UpdateRetainedJourneySeatOrderItems(OrderDocument orderDocMgmt, OrderDocument oldOrderDoc)
        {
            if (oldOrderDoc.OrderItems.Any(x => x.Services.Any(srv => srv.Product != null && srv.Product.ProductType == Product.ProductTypes.SeatEnum)))
            {
                var retainedJourneySegments = oldOrderDoc.DataList.SegmentDetails.Where(oldSeg => orderDocMgmt.DataList.SegmentDetails.Any(seg =>
                                                               seg.OriginAirportCode == oldSeg.OriginAirportCode &&
                                                               seg.DestinationAirportCode == oldSeg.DestinationAirportCode &&
                                                               seg.MarketingFlightNumber == oldSeg.MarketingFlightNumber &&
                                                               seg.ClassOfService == oldSeg.ClassOfService &&
                                                               Convert.ToDateTime(seg.ScheduledDepartureTime.LocalDateTime) == Convert.ToDateTime(oldSeg.ScheduledDepartureTime.LocalDateTime)
                                                               )).Select(x => x.SegmentId);

                if (retainedJourneySegments.Any())
                {
                    var seatOrderItems = oldOrderDoc.OrderItems.Where(oi => oi.Services.Any(srv => srv.Product != null && srv.Product.ProductType == Product.ProductTypes.SeatEnum));
                    foreach (var orderItem in seatOrderItems)
                    {
                        if (orderItem.Services.Any(srv => srv.SegmentRefIds.Any(segId => retainedJourneySegments.Contains(segId))))
                        {
                            var retainedOrderItems = new EAPI.Domain.Virgin.Models.Order.OrderItems()
                            {
                                OrderItemId = orderItem.OrderItemId,
                                OwnerAirline = orderItem.OwnerAirline,
                                Services = orderItem.Services,
                                PriceDetails = orderItem.PriceDetails,
                                AccountableDocumentRefs = orderItem.AccountableDocumentRefs,
                                ItemStatus = orderItem.ItemStatus,
                                PaymentDetails = orderItem.PaymentDetails
                            };
                            orderDocMgmt.OrderItems = orderDocMgmt.OrderItems.Append(retainedOrderItems);
                        }
                    }
                }
            }
            return orderDocMgmt;
        }

        public static OrderDocument UpdateCCPaymentDetails(OrderDocument orderDoc, PaymentTransactionDocument paymentDocument, string trackId, [Optional] bool isDocumentExchanged)
        {
            var paymentDetail = paymentDocument.PaymentDetails.First();

            orderDoc.OrderItems = [.. orderDoc.OrderItems.Select(oi =>
            {
                if (oi.Services.Any(srv => srv.Product?.ProductType == Product.ProductTypes.SeatEnum)) return oi;

                var existingPaymentDetails = oi.PaymentDetails ?? [];
                var newPaymentDetails = new EAPI.Domain.Virgin.Models.Payment.PaymentDetails()
                {
                    PaymentReferenceId = paymentDocument.Id,
                    PaymentCommitmentDateTime = paymentDocument.Id.Contains("Refund") ? paymentDocument.CreatedOn : paymentDocument.ModifiedOn,
                    TrackId = trackId,
                    ApprovalCode = paymentDetail.PaymentApprovalCode,
                    Amount = new EAPI.Domain.Virgin.Models.Common.CurrencyAmount()
                    {
                        MonetaryAmount = paymentDocument.Id.Contains("Refund") ? "-" + paymentDetail.Amount.MonetaryAmount : paymentDetail.Amount.MonetaryAmount,
                        BaseCurrency = paymentDetail.Amount.BaseCurrency
                    },
                    CardMethod = new()
                    {
                        CardNumber = paymentDetail.CardMethod.CardNumber,
                        CardBrandCode = paymentDetail.CardMethod.CardType
                    },
                    PaymentProcessingStatus = EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.SUCCESSFUL,
                    PaxRefIds = orderDoc.DataList.CustomerDetails.Select(x => x.CustId),
                    SequenceId = oi.PaymentDetails?.FirstOrDefault()?.SequenceId == null ? "1" : Convert.ToString(Convert.ToInt32(oi.PaymentDetails.OrderBy(x => x.SequenceId).Last().SequenceId) + 1),
                    FormOfPayments = EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum
                };

                oi.PaymentDetails = isDocumentExchanged ? new[] { newPaymentDetails } : existingPaymentDetails.Append(newPaymentDetails);

                return oi;
            })];

            return orderDoc;
        }

        public static OrderDocument UpdateOTPaymentFunctions(OrderDocument orderDocMgmt, OrderDocument oldOrderDoc,bool isCashPayment, bool isEasyPayment)
        {
            var latestSequenceId = Convert.ToInt32(orderDocMgmt.OrderItems.FirstOrDefault()?.PaymentDetails?.OrderByDescending(x => x.PaymentCommitmentDateTime)?.FirstOrDefault()?.SequenceId ?? "0");

            foreach (var oi in orderDocMgmt.OrderItems)
            {
                if (oi.Services.Any(srv => srv.Product?.ProductType == Product.ProductTypes.SeatEnum)) continue;

                var oTPaymentFunctions = new EAPI.Domain.Virgin.Models.Payment.PaymentDetails()
                {
                    PaymentReferenceId = GenerateTransactionId(),
                    PaymentCommitmentDateTime = DateTime.UtcNow.ToString(Constants.Application.InputDateTimeFormat),
                    Amount = new EAPI.Domain.Virgin.Models.Common.CurrencyAmount()
                    {
                        MonetaryAmount = "0.00",
                        BaseCurrency = oi.PriceDetails.First().Amount.BaseCurrency
                    },
                    PaxRefIds = orderDocMgmt.DataList.CustomerDetails.Select(x => x.CustId),
                    SequenceId = Convert.ToString(latestSequenceId + 1),
                    PaymentProcessingStatus = EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.SUCCESSFUL,
                    FormOfPayments = isEasyPayment ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay : isCashPayment ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum : EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum
                };

                oi.PaymentDetails = (oi.PaymentDetails ?? Enumerable.Empty<EAPI.Domain.Virgin.Models.Payment.PaymentDetails>())
                    .Append(oTPaymentFunctions);
            }

            return orderDocMgmt;
        }

        public static OrderDocument UpdateExchangeCCPaymentDetails(OrderDocument orderDocumentMgmt, PaymentTransactionDocument paymentTransactionDocument , string trackid , OrderDocument orderDoc)
        {
            var latestSequenceId = Convert.ToInt32(orderDoc.OrderItems.FirstOrDefault()?.PaymentDetails?.OrderByDescending(x => x.PaymentCommitmentDateTime)?.FirstOrDefault()?.SequenceId ?? "0");

            var paymentDetails = paymentTransactionDocument.PaymentDetails.First();

            orderDocumentMgmt.OrderItems = orderDocumentMgmt.OrderItems.Select(oi =>
            {
                if (oi.Services.Any(srv => srv.Product.ProductType == Product.ProductTypes.SeatEnum)) return oi;

                if (oi.PaymentDetails != null && oi.PaymentDetails.Any())
                {
                    if (paymentTransactionDocument != null)
                    {
                        oi.PaymentDetails = orderDoc.OrderItems.FirstOrDefault()?.PaymentDetails.Append(new EAPI.Domain.Virgin.Models.Payment.PaymentDetails()
                        {
                            PaymentReferenceId = paymentTransactionDocument.Id,
                            PaymentCommitmentDateTime = paymentTransactionDocument.CreatedOn,
                            TrackId = trackid,
                            Amount = new EAPI.Domain.Virgin.Models.Common.CurrencyAmount()
                            {
                                MonetaryAmount = paymentDetails.Amount.MonetaryAmount.ToString(),
                                BaseCurrency = oi.PriceDetails.First().Amount.BaseCurrency
                            },
                            CardMethod = new()
                            {
                                CardNumber = paymentDetails.CardMethod.CardNumber,
                                CardBrandCode = paymentDetails.CardMethod.CardType
                            },
                            PaxRefIds = orderDoc.DataList.CustomerDetails.Select(x => x.CustId),
                            PaymentProcessingStatus = EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.SUCCESSFUL,
                            FormOfPayments = EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum,
                            SequenceId = Convert.ToString(latestSequenceId + 1),
                        });
                    }                   
                }
                return oi;
            });

            return orderDocumentMgmt;
        }

        private static String GenerateTransactionId()
        {
            long unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            Random rand = new Random();

            int dRandom = rand.Next(00000000, 99999999);
            string strFinalNum = Convert.ToString(unixTimestamp) + Convert.ToString(dRandom);

            return strFinalNum;
        }
    }
}
