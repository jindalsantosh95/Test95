using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System;
using System.Globalization;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS
{
    public class CardInfo
    {
       
        public string CardNumber { get; set; }
        [OpenApiProperty(Description = "Is the card number provided already tokenised. This means that the airline has key to de-tokenise in order to take payment.")]      
        public string ExpiryDate { get; set; }
        public string CardholderName { get; set; }
        public string Cvv { get; set; } 
        public string CardType { get; set; }


        public static explicit operator CardInfo(Domain.OrderCreateRQ.PaymentProcessingPaymentCardType cardType)
        {

            var expDate = cardType.ExpirationDate?.Substring(0, 2) + "/" + cardType.ExpirationDate?.Substring(2, 2);           

            return new CardInfo()            {

                CardNumber = string.IsNullOrEmpty(cardType?.CardNumber) ? null : cardType?.CardNumber,
                ExpiryDate = expDate,
                CardholderName = cardType?.CardHolderName,
                Cvv = cardType?.CardSecurityCode,
                CardType = cardType?.CardTypeText
            };

        }

        public static explicit operator CardInfo(Domain.OrderChangeRQ.PaymentProcessingPaymentCardType cardType)
        {

            var expDate = cardType.ExpirationDate?.Substring(0, 2) + "/" + cardType.ExpirationDate?.Substring(2, 2);

            return new CardInfo()
            {

                CardNumber = string.IsNullOrEmpty(cardType?.CardNumber) ? null : cardType?.CardNumber,
                ExpiryDate = expDate,
                CardholderName = cardType?.CardHolderName,
                Cvv = cardType?.CardSecurityCode,
                CardType = cardType?.CardTypeText
            };

        }
    }
}
