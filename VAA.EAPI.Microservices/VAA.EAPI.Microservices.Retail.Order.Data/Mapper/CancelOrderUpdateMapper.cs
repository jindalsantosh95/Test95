using System;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Mapper
{
    public class CancelOrderUpdateMapper
    {
        public static OrderDocument UpdateCancelOrderStatus(OrderDocument orderDoc, string cancelStatus, bool isNonRFC, bool isCancelDisruption)
        {
            if(orderDoc.ChangeType == ChangeTypeEnum.PaymentFailure) 
            {
                orderDoc.ChangeType = ChangeTypeEnum.PaymentFailure;
                orderDoc.ChangeDateTime = orderDoc.ChangeDateTime;
            }
            else
            {
                orderDoc.OrderStatus = cancelStatus;
                orderDoc.ChangeType = isNonRFC || isCancelDisruption ? ChangeTypeEnum.DocumentRefunded : ChangeTypeEnum.DocumentVoided;
                orderDoc.Version = orderDoc.Version + 1;
                orderDoc.ChangeDateTime = new() { UtcDateTime = DateTime.UtcNow.ToString(Constants.Application.ZuluDateTimeFormat) };
            }

            return orderDoc;
        }
    }
}
