using System;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Order;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Insurance;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment;
using VAA.EAPI.Common;
using CloudEvent = VAA.EAPI.Domain.Events.CloudEvent;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Mapper
{
    public static class InsuranceResponseMapper
    {
        public static CloudEvent MapInsuranceResponseToOrderDocument(PurchaseRequest purchaseRequest, PurchaseResponse insuranceResponse)
        {
            var orderDocument = new OrderDocument
            {
                Id = purchaseRequest.OrderId,
                PartitionKey = purchaseRequest.OrderId,
                InsurancePurchaseDetails = new()
                {
                    InsurancePurchaseAmount = purchaseRequest.Purchases.First().InsuranceBookingDetails.TotalPrice,
                    InsurancePurchaseStatus = new() { Code = insuranceResponse.Status.Code, Message = insuranceResponse.Status.Message },
                    TransactionId = insuranceResponse.TransactionID,
                    TimeStamp = DateTime.Now
                }
            };

            var cloudEvent = new CloudEvent()
            {
                Id = Guid.NewGuid().ToString(),
                Data = orderDocument,
                Type = Constants.Application.InsurancePurchaseResponseStatusEventType,
                Source = "VAA.EAPI.Microservices.Retail.Order.Payment.InsurancePurchase",
                Subject = "InsurancePurchaseResponseStatus",
                Time = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.EventSpecversion
            };

            return cloudEvent;
           
        }

        public static CloudEvent ClearPaymentDetails(PurchaseRequest purchaseRequest)
        {
            var cloudEvent = new CloudEvent()
            {
                Id = Guid.NewGuid().ToString(),
                Data = new PaymentRequest()
                {
                    PaymentReferenceId = purchaseRequest.PaymentReferenceId
                },
                Type = Constants.Application.ClearCVVDetailsEventType,
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicClearPaymentDetails),
                Subject = Constants.Application.ClearPaymentDetailsEventSubject,
                Time = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.EventSpecversion
            };

            return cloudEvent;
        }
    }
}
