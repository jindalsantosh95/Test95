using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderCreateRQ;

namespace VAA.EAPI.NDC.Order.Validations.Common
{
    public static class LuhnCheckValidation
    {
        public static IEnumerable<ErrorDetail> ValidateCard(PaymentFunctionsCreateType[] paymentFunctions)
        {
            if (paymentFunctions.Any(x => x?.PaymentProcessingDetails?.PaymentMethod?.Item is Domain.OrderCreateRQ.PaymentProcessingPaymentCardType card && card.CardholderAddress?.StreetText?.Any(st =>string.IsNullOrWhiteSpace(st)) == true))
                return [new() { IsDynamicMessage = false, ErrorCode = "708_21" }];

            if (paymentFunctions.Any(x => x?.PaymentProcessingDetails?.PaymentMethod?.Item is Domain.OrderCreateRQ.PaymentProcessingPaymentCardType card && (string.IsNullOrWhiteSpace(card.CardholderAddress?.PostalCode)) == true && card.CardholderAddress?.PostalCode is not null))
                return [new() { IsDynamicMessage = false, ErrorCode = "708_22" }];

            if (paymentFunctions.Any(x => x?.PaymentProcessingDetails?.PaymentMethod?.Item is Domain.OrderCreateRQ.PaymentProcessingPaymentCardType card && !string.IsNullOrEmpty(card.CardNumber) && !Mod10Check(card.CardNumber)))
                return [new() { IsDynamicMessage = false, ErrorCode = "708_3" }];

            return [];
        }

        public static IEnumerable<ErrorDetail> ValidateCard(PaymentFunctionsChangeType[] paymentFunctions)
        {
            if (paymentFunctions.Any(x => x?.PaymentProcessingDetails?.PaymentMethod?.Item is Domain.OrderChangeRQ.PaymentProcessingPaymentCardType card && !string.IsNullOrEmpty(card.CardNumber) && !Mod10Check(card.CardNumber)))
                return [new() { IsDynamicMessage = false, ErrorCode = "708_3" }];

            return [];
        }

        public static bool Mod10Check(string cardNumber)
        {
            if (cardNumber == null || cardNumber.Where(Char.IsDigit).Reverse().Select((e, i) => (e - 48) * (i % 2 == 0 ? 1 : 2)).Sum((e) => e / 10 + e % 10) % 10 != 0)
                return false;

            return true;
        }

        public static IEnumerable<ErrorDetail> ValidateSecurePaymentVersion2(PaymentFunctionsChangeType[] paymentFunctions)
        {
            if (paymentFunctions?.Any(x => x?.PaymentProcessingDetails?.PaymentMethod?.Item is Domain.OrderChangeRQ.PaymentProcessingPaymentCardType card && card.SecurePaymentVersion2 == null) == true)
            {
                return
                [
                    new()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "912_43"
                    }
                ];
            }

            return [];
        }
    }
}
