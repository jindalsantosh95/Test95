using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.PaymentUpdate;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Mapper.PaymentUpdate
{
    public class PaymentUpdatePnrSearchDocMapper
    {
        public static PnrSearch MaptoPnrSeacrhDoc(PaymentUpdateRepoCommand request, PnrSearch pnrSearch, string paymentId)
        {
            var paymentRequest = request.PaymentUpdateRequestData.PaymentRequest.FirstOrDefault();

            List<PnrSearchPayment> payment = new()
            {
                new()
                {
                    PaymentId = paymentId,
                    ClientId = paymentRequest.ClientId,
                    ChannelId = request.PaymentUpdateRequestData.Channel,
                    TransactionId = paymentRequest.TransactionId,
                    TransactionDateTime = DateTime.Parse(paymentRequest.TransactionDateTime).ToString(Constants.Application.ISODateTimeFormat),
                    TransactionType = TransactionType.Payment.ToString(),
                    TransactionAmount = paymentRequest.TransactionAmount,
                    Status = paymentRequest.Status.ToString(),
                    Currency = paymentRequest.Currency,
                    CountryId = paymentRequest.CountryId,
                    AuthorisationCode = paymentRequest.AuthorisationCode,
                    RefundReason = string.Empty
                }
            };

            return new PnrSearch()
            {
                Channel = pnrSearch?.Channel,
                DocType = pnrSearch?.DocType,
                ETicket = pnrSearch?.ETicket,
                Id = pnrSearch?.Id,
                OrderCreationDate = pnrSearch?.OrderCreationDate,
                OrderCreationTime = DateTime.Parse(pnrSearch?.OrderCreationTime).ToString(Constants.Application.ISODateTimeFormat),
                OrderRef = pnrSearch?.OrderRef,
                OrderStatus = OrderCreateStatus.Ordered.ToString(),
                PartitionKey = pnrSearch?.PartitionKey,
                SchemaVersion = pnrSearch?.SchemaVersion,
                CreatedBy = pnrSearch?.CreatedBy,
                CreatedDateTime = DateTime.Parse(pnrSearch?.CreatedDateTime).ToString(Constants.Application.ISODateTimeFormat),
                UpdateBy = pnrSearch?.CreatedBy,
                Payments = payment,
                UpdatedDateTime = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat)
            };

        }
    }
}
