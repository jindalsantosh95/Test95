using System;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order;
using CloudEvent = VAA.EAPI.Domain.Events.CloudEvent;

namespace VAA.EAPI.Microservices.Retail.Order.Mapper.OrderCreate
{
    public class OrderCreateCommandToInsuranceEventMapper
    {
        public static CloudEvent MapOrderToInsurancePurchaseRequest(PurchaseRequest purchaseRequest)
        {
            var cloudEvent = new CloudEvent
            {
                Data = purchaseRequest,
                Type = EnumOperations.GetEnumDescription((OrderEventType)Enum.Parse(typeof(OrderEventType), OrderEventType.InsurancePurchase.ToString())),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = Constants.Application.InsurancePurchaseEventSubject,
                Time = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.EventSpecversion
            };

            return cloudEvent;
        }

        public static PurchaseRequest GetPurchasesRequest(OrderCreateRequest orderCreateRequest, OrderResponse order)
        {
            var requestedPurchase = orderCreateRequest.InsuranceDetails.Purchase;

            return new PurchaseRequest()
            {
                OrderId = order.OrderNumber?.OrderId,
                PaymentReferenceId = orderCreateRequest.PaymentDetails.FirstOrDefault(a => !a.PaymentReferenceId.IsNullOrEmpty())?.PaymentReferenceId ?? requestedPurchase?.PaymentDetails?.PaymentReferenceId,
                BusinessUnit = orderCreateRequest.InsuranceDetails.BusinessUnit,
                TransactionID = orderCreateRequest.InsuranceDetails.TransactionID,
                Purchases = [
                    new Purchase()
                    {
                        PartnerBookingID = order.BookingReference.RLOC,
                        InsuranceBookingDetails  = requestedPurchase.InsuranceBookingDetails,
                        TravelBookingDetails = requestedPurchase.TravelBookingDetails,
                        TravelDetails = requestedPurchase.TravelDetails,
                        ProductDetails = requestedPurchase.ProductDetails,
                    }
                ],
                BrowserInfo = orderCreateRequest.Pos?.BrowserInfo
            };
        }

        public static CloudEvent DecisionRequestMapper(OrderCreateRequest orderCreateRequest, OrderResponse order)
        {
            var cloudEvent = new CloudEvent()
            {
                Data = new DecisionRequest()
                {
                    BookingId = order.BookingReference.RLOC,
                    BusinessUnit = orderCreateRequest.InsuranceDetails.BusinessUnit,
                    CurrencyType = orderCreateRequest.InsuranceDetails.Purchase.InsuranceBookingDetails.Currency,
                    Cxid = orderCreateRequest.InsuranceDetails.Purchase?.CxId,
                    ResponseId = orderCreateRequest.InsuranceDetails.Purchase?.ResponseId,
                    ResponseType = "Purchased",
                    Details = [
                        new Detail(){
                            ProductCode = orderCreateRequest.InsuranceDetails.Purchase.ProductDetails.Product?.FirstOrDefault()?.ProductID,
                            PurchasePrice = orderCreateRequest.InsuranceDetails.Purchase.InsuranceBookingDetails.TotalPrice,
                            SupplierCode = orderCreateRequest.InsuranceDetails.Purchase.SupplierCode,
                            Quantity = 1,
                            ResponseQuoteId = orderCreateRequest.InsuranceDetails.Purchase?.ResponseId,
                            ResponseType = "Declined"
                        }
                    ]
                },
                Type = EnumOperations.GetEnumDescription((OrderEventType)Enum.Parse(typeof(OrderEventType), OrderEventType.InsuranceDecision.ToString())),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = Constants.Application.InsuranceDecisionEventSubject,
                Time = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.EventSpecversion
            };

            return cloudEvent;
        }
    }
}