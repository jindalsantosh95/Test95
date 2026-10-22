using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS
{
    public class PaymentCardAuthentication
    {
        [OpenApiProperty(Description = "Electronic Commerce Indicator is part of the 2 data elements that indicate the transaction was processed electronically.")]
        public string Eci { get; set; }
        [OpenApiProperty(Description = "Server Transaction Identifier is the universally unique transaction identifier assigned by the 3DS Server to identify a single transaction.")]
        public string ServerTransactionId { get; set; }
        [OpenApiProperty(Description = "Cardholder Authentication Verification Value (CAVV) is the value generated from algorithm for payment card authentication.")]
        public string Cavv { get; set; }

        public static explicit operator PaymentCardAuthentication (Domain.OrderCreateRQ.PaymentProcessingPaymentCardType cardType)
        {
            return new PaymentCardAuthentication()
            {
                Eci = cardType.SecurePaymentVersion2.ElectronicCommerceInd,
                ServerTransactionId = cardType.SecurePaymentVersion2.DirectoryServerTrxID,
                Cavv = cardType.SecurePaymentVersion2.AuthenticationValue
            };
        }

        public static explicit operator PaymentCardAuthentication(Domain.OrderChangeRQ.PaymentProcessingPaymentCardType cardType)
        {
            return new PaymentCardAuthentication()
            {
                Eci = cardType.SecurePaymentVersion2.ElectronicCommerceInd,
                ServerTransactionId = cardType.SecurePaymentVersion2.DirectoryServerTrxID,
                Cavv = cardType.SecurePaymentVersion2.AuthenticationValue
            };
        }

    }
}
