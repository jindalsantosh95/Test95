using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.RefundUpdate;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Mapper.RefundUpdate
{
    public class RefundUpdateOrderDocMapper
    {
        public static OrderDetails MapToOrderDoc(RefundUpdateRepoCommand request, OrderDetails order, string paymentId)
        {
            var refundrequest = request.VoidOrderUpdateData.Payments.FirstOrDefault();

            List<Payment> payments = new();
            order?.Payments?.ForEach(p =>
            {
                Payment payment = new()
                {
                    PaymentId = p.PaymentId,
                    ClientId = p.ClientId,
                    ChannelId = p.ChannelId,
                    TransactionId = p.TransactionId,
                    TransactionDateTime = DateTime.Parse(p.TransactionDateTime).ToString(Constants.Application.ISODateTimeFormat),
                    TransactionAmount = p.TransactionAmount,
                    TransactionType = p.TransactionType,
                    PaymentMethod = p.PaymentMethod.ToString(),
                    PaymentType = p.PaymentType.ToString(),
                    PaymentSubType = p.PaymentSubType.ToString(),
                    CardName = p.CardName,
                    CardExpiry = p.CardExpiry,
                    CardNumber = p.CardNumber,
                    Status = p.Status.ToString(),
                    CountryId = p.CountryId,
                    Currency = p.Currency,
                    AuthorisationCode = p.AuthorisationCode,
                    RefundReason = p.RefundReason,
                    FraudCode = p.FraudCode,
                    FraudStatus = p.FraudStatus,
                    PspName = p.PspName,
                    PspId = p.PspId,
                    CreatedBy = p.CreatedBy,
                    CreatedDateTime = DateTime.Parse(p.CreatedDateTime).ToString(Constants.Application.ISODateTimeFormat),
                    UpdateBy = Constants.Application.CreatedBy,
                    UpdatedDateTime = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat)
                };
                payments.Add(payment);
            });

            foreach (var payment in order?.Payments)
            {
                if (payment.TransactionId == refundrequest.TransactionId)
                {
                    Payment refund = new()
                    {
                        PaymentId = paymentId,
                        ClientId = payment.ClientId,
                        ChannelId = payment.ChannelId,
                        TransactionId = refundrequest.TransactionId,
                        TransactionDateTime = DateTime.Parse(payment.TransactionDateTime).ToString(Constants.Application.ISODateTimeFormat),
                        TransactionAmount = payment.TransactionAmount,
                        TransactionType = TransactionType.Refund.ToString(),
                        PaymentMethod = payment.PaymentMethod,
                        PaymentType = payment.PaymentType,
                        PaymentSubType = payment.PaymentSubType,
                        CardName = payment.CardName,
                        CardExpiry = payment.CardExpiry,
                        CardNumber = payment.CardNumber,
                        Status = payment.Status,
                        CountryId = payment.CountryId,
                        Currency = payment.Currency,
                        AuthorisationCode = payment.AuthorisationCode,
                        RefundReason = ConfigReader.Get(Constants.Environment.RefundReason),
                        FraudCode = payment.FraudCode,
                        FraudStatus = payment.FraudStatus,
                        PspName = payment.PspName,
                        PspId = payment.PspId,
                        CreatedBy = Constants.Application.CreatedBy,
                        CreatedDateTime = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                        UpdateBy = Constants.Application.CreatedBy,
                        UpdatedDateTime = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat)
                    };
                    payments.Add(refund);
                }
            }

            return new OrderDetails()
            {
                Id = order.Id,
                OrderRef = order.OrderRef,
                SchemaVersion = order.SchemaVersion,
                DocType = order.DocType,
                PartitionKey = order.PartitionKey,
                OrderCreationDate = order.OrderCreationDate,
                OrderCreationTime = DateTime.Parse(order.OrderCreationTime).ToString(Constants.Application.ISODateTimeFormat),
                OrderStatus = OrderCreateStatus.RiskFreeCancelled.ToString(),
                Channel = order.Channel,
                FulfilmentId = order.FulfilmentId,
                Payments = payments,
                CreatedBy = order.CreatedBy,
                CreatedDateTime = DateTime.Parse(order.CreatedDateTime).ToString(Constants.Application.ISODateTimeFormat),
                UpdateBy = order.CreatedBy,
                UpdatedDateTime = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat)
            };
        }
    }
}
