using System.Linq;
using System.Xml.Serialization;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    [XmlRoot(ElementName = "root")]
    public class RefundPaymentRequest
    {
        [XmlElement(ElementName = "refund")]
        public Refund Refund { get; set; }

        public static explicit operator RefundPaymentRequest(Domain.CPD.RefundPaymentRequest request)
        {
            int amount = (int)(request.PaymentDetails.First().RefundAmount.MonetaryAmount.Parse<decimal>() * 100);
            return new()
            {
                Refund = new()
                {
                    ClientId = ConfigReader.Get(Constants.Environment.CPDClientId),
                    Transaction = new()
                    {
                        Id = request.PaymentDetails.First().PaymentTransactionId,
                        OrderNo = request.OrderId,
                        Amount = new()
                        {   
                            Value = amount
                        },
                    }
                }
            };
        }
    }
}
