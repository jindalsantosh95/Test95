using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.RefundUpdate;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Mapper.RefundUpdate
{
    public class RefundUpdatePnrSearchDocMapper
    {
        public static PnrSearch MaptoPnrSearchDoc(RefundUpdateRepoCommand request, PnrSearch pnrSearch, string paymentId)
        {
            var refundrequest = request.VoidOrderUpdateData.Payments.FirstOrDefault();

            List<PnrSearchPayment> payments = new();
            pnrSearch?.Payments?.ForEach(p =>
            {
                PnrSearchPayment payment = new()
                {
                    PaymentId = p.PaymentId,
                    ClientId = p.ClientId,
                    ChannelId = p.ChannelId,
                    TransactionId = p.TransactionId,
                    TransactionDateTime = DateTime.Parse(p.TransactionDateTime).ToString(Constants.Application.ISODateTimeFormat),
                    TransactionType = p.TransactionType,
                    TransactionAmount = p.TransactionAmount,
                    Status = p.Status,
                    Currency = p.Currency,
                    CountryId = p.CountryId,
                    AuthorisationCode = p.AuthorisationCode,
                    RefundReason = string.Empty
                };
                payments.Add(payment);
            });

            foreach (var payment in pnrSearch?.Payments)
            {
                if (payment.TransactionId == refundrequest.TransactionId)
                {
                    PnrSearchPayment refund = new()
                    {
                        PaymentId = paymentId,
                        ClientId = payment.ClientId,
                        ChannelId = payment.ChannelId,
                        TransactionId = refundrequest.TransactionId,
                        TransactionDateTime = DateTime.Parse(payment.TransactionDateTime).ToString(Constants.Application.ISODateTimeFormat),
                        TransactionType = TransactionType.Refund.ToString(),
                        TransactionAmount = payment.TransactionAmount,
                        Status = payment.Status,
                        Currency = payment.Currency,
                        CountryId = payment.CountryId,
                        AuthorisationCode = payment.AuthorisationCode,
                        RefundReason = ConfigReader.Get(Constants.Environment.RefundReason)
                    };
                    payments.Add(refund);
                }
            }


            return new PnrSearch()
            {
                Channel = pnrSearch?.Channel,
                DocType = pnrSearch?.DocType,
                ETicket = pnrSearch?.ETicket,
                Id = pnrSearch?.Id,
                OrderCreationDate = pnrSearch?.OrderCreationDate,
                OrderCreationTime = DateTime.Parse(pnrSearch?.OrderCreationTime).ToString(Constants.Application.ISODateTimeFormat),
                OrderRef = pnrSearch?.OrderRef,
                OrderStatus = OrderCreateStatus.RiskFreeCancelled.ToString(),
                PartitionKey = pnrSearch?.PartitionKey,
                SchemaVersion = pnrSearch?.SchemaVersion,
                CreatedBy = pnrSearch?.CreatedBy,
                CreatedDateTime = DateTime.Parse(pnrSearch?.CreatedDateTime).ToString(Constants.Application.ISODateTimeFormat),
                UpdateBy = pnrSearch?.CreatedBy,
                Payments = payments,
                UpdatedDateTime = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat)
            };
        }
    }
}
