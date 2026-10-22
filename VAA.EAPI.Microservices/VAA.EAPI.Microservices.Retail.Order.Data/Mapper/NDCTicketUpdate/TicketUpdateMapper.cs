using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.NDCTicketUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;
namespace VAA.EAPI.Microservices.Retail.Order.Data.Mapper.NDCTicketUpdate
{
    public static class TicketUpdateMapper
    {
        public static OrderDocument MapOrderDocument(OrderDocument orderDoc, IssueTicketResponse issueTicketResponse)
        {
            if (issueTicketResponse.AccountableDocumentRef != null)
            {
                foreach (var item in issueTicketResponse.AccountableDocumentRef)
                {
                    var orderItem = orderDoc.OrderItems.FirstOrDefault(x => x.Services.First().ServiceBeneficiaries.Any(y => y == item.CustomerRefId));

                    if (orderItem is not null)
                    {
                        if (orderItem.AccountableDocumentRefs != null)

                            orderItem.AccountableDocumentRefs = orderItem.AccountableDocumentRefs.Append(item);
                        else
                            orderItem.AccountableDocumentRefs = [item];
                    }

                    if(orderItem is not null)
                    {
                        if(orderItem.PaymentDetails is null)
                        {
                            orderItem.PaymentDetails = new List<PaymentDetails>() { new PaymentDetails() { PaymentReferenceId = issueTicketResponse.TransactionId } };
                        }
                        else
                        {
                            orderItem.PaymentDetails.First().PaymentReferenceId = issueTicketResponse.TransactionId;
                        }
                    }

                }
            }

            if (issueTicketResponse?.PriceClass != null && orderDoc?.DataList?.PriceClass != null)
            {
                foreach (var responsePriceClass in issueTicketResponse.PriceClass)
                {
                    if (string.IsNullOrWhiteSpace(responsePriceClass?.FareBasisCode) || string.IsNullOrWhiteSpace(responsePriceClass.TicketDesigCode))
                        continue;

                    foreach (var orderPriceClass in orderDoc.DataList.PriceClass)
                    {
                        if (orderPriceClass?.FareBasisCode == responsePriceClass.FareBasisCode)
                        {
                            orderPriceClass.TicketDesigCode = responsePriceClass.TicketDesigCode;
                        }
                    }
                }
            }
            orderDoc.BookingReference.ExternalRecordLocator = issueTicketResponse.BookingReference.ExternalRecordLocator;
            orderDoc.ChangeType = issueTicketResponse.ChangeType;
            orderDoc.Version = issueTicketResponse.ChangeType == ChangeTypeEnum.DocumentIssuance ? orderDoc.Version + 1 : orderDoc.Version;
            orderDoc.ChangeDateTime =  new UtcAndLocalDateTime() { UtcDateTime = DateTime.UtcNow.ToString(Constants.Application.ZuluDateTimeFormat) };

            return orderDoc;
        }
    }
}

