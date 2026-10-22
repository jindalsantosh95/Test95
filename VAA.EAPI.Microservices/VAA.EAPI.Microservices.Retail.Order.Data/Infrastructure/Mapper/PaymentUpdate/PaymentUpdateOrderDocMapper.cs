using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.PaymentUpdate;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Mapper.PaymentUpdate
{
    public class PaymentUpdateOrderDocMapper
    {
        public static OrderDetails MapToOrderDoc(PaymentUpdateRepoCommand request, OrderDetails order, string paymentId)
        {
            var paymentRequest = request.PaymentUpdateRequestData.PaymentRequest.FirstOrDefault();

            List<Payment> payments = new()
            {
                new()
                {
                    PaymentId = paymentId,
                    ClientId = paymentRequest.ClientId,
                    ChannelId = request.PaymentUpdateRequestData?.Channel,
                    TransactionId = paymentRequest.TransactionId,
                    TransactionDateTime = DateTime.Parse(paymentRequest.TransactionDateTime).ToString(Constants.Application.ISODateTimeFormat),
                    TransactionAmount = paymentRequest.TransactionAmount,
                    TransactionType = TransactionType.Payment.ToString(),
                    PaymentMethod = paymentRequest.PaymentMethod.ToString(),
                    PaymentType = paymentRequest.PaymentType.ToString(),
                    PaymentSubType = paymentRequest.PaymentSubType.ToString(),
                    CardName = paymentRequest.CardName,
                    CardExpiry = paymentRequest.CardExpiry,
                    CardNumber = paymentRequest.CardNumber,
                    Status = paymentRequest.Status.ToString(),
                    CountryId = paymentRequest.CountryId,
                    Currency = paymentRequest.Currency,
                    AuthorisationCode = paymentRequest.AuthorisationCode,
                    RefundReason = "",
                    FraudCode = paymentRequest.FraudCode,
                    FraudStatus = paymentRequest.FraudStatus,
                    PspName = paymentRequest.PspName,
                    PspId = paymentRequest.PspId,
                    CreatedBy = Constants.Application.CreatedBy,
                    CreatedDateTime = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                    UpdateBy = Constants.Application.CreatedBy,
                    UpdatedDateTime = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat)

                }
            };

            return new OrderDetails()
            {
                Id = order.Id,
                OrderRef = order.OrderRef,
                SchemaVersion = order.SchemaVersion,
                DocType = order.DocType,
                PartitionKey = order.PartitionKey,
                OrderCreationDate = order.OrderCreationDate,
                OrderCreationTime = DateTime.Parse(order?.OrderCreationTime).ToString(Constants.Application.ISODateTimeFormat),
                OrderStatus = OrderCreateStatus.Ordered.ToString(),
                Channel = order.Channel,
                FulfilmentId = order.FulfilmentId,
                Payments = payments,
                CreatedBy = order.CreatedBy,
                CreatedDateTime = DateTime.Parse(order?.CreatedDateTime).ToString(Constants.Application.ISODateTimeFormat),
                UpdateBy = order.CreatedBy,
                UpdatedDateTime = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat)
            };
        }
    }
}
