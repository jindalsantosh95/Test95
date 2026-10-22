using System;
using VAA.EAPI.NDC.Order.Constants;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;

namespace VAA.EAPI.NDC.Order.Helper
{
    public static class CardEnrolledStatus
    {
        public static CardEnrollStatus GetCardEnrolledStatus(PaymentProcessingPaymentCardType processingPaymentCardType)
        {
            var enrollStatusStr = processingPaymentCardType?.SecureProgram?.EnrollmentStatusText?.Trim();
            
            enrollStatusStr = string.IsNullOrEmpty(enrollStatusStr) ? null : enrollStatusStr.IndexOf('.') > 0 ? enrollStatusStr.Substring(0, enrollStatusStr.IndexOf('.')) : enrollStatusStr;
            
            var cardEnrolledStatus = (CardEnrollStatus)Enum.Parse(typeof(CardEnrollStatus), enrollStatusStr);
            
            return cardEnrolledStatus;
        }
    }
}
