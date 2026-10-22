using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.RefundTicket;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.VoidTransaction;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Mapper.VoidTransaction
{
    public class VoidOrderDataMapper
    {
        public static VoidOrderUpdateData MapToVoidOrderData(VoidTransactionDetails voidTransactionDetails)
        {
            if (voidTransactionDetails != null && voidTransactionDetails.OrderData != null)
            {
                return new VoidOrderUpdateData()
                {
                    FulfilmentId = MapToFulfilmentData(voidTransactionDetails),
                    OrderRef = voidTransactionDetails.OrderData.OrderRef,
                    Payments = MapToPayments(voidTransactionDetails)
                };
            }
            else return null;
        }
        public static List<FulfilmentId> MapToFulfilmentData(VoidTransactionDetails voidTransactionDetails)
        {
            var fulfillmentDetails = new List<FulfilmentId>();
            if (voidTransactionDetails.OrderData.ETicket != null)
            {
                voidTransactionDetails.OrderData.ETicket.ForEach(e =>
                {
                    fulfillmentDetails.Add(new FulfilmentId()
                    {
                        FulfillmentId = e
                    });
                });
                return fulfillmentDetails;
            }
            else return null;
        }

        public static List<Domain.VoidTransaction.Payment> MapToPayments(VoidTransactionDetails voidTransactionDetails)
        {
            var paymentDetails = new List<Domain.VoidTransaction.Payment>();
            if (voidTransactionDetails.OrderData.Payments != null)
            {
                voidTransactionDetails.OrderData.Payments.ForEach(p =>
             {
                 paymentDetails.Add(new Domain.VoidTransaction.Payment()
                 {
                     PaymentId = p.PaymentId,
                     TransactionId = p.TransactionId
                 });
             });
                return paymentDetails;
            }
            else return null;
        }
    }
}
