using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    public sealed class HolidayPaymentResponse
    {
        public decimal AmountToPay { get; set; }
        public string Status { get; set; }     
        public string PaymentLink { get; set; }
        public string RemoveLink { get; set; }      

        public static explicit operator HolidayPaymentResponse(HolidayPaymentResponseMs source)
        {
            if (source == null) return null;

            return new HolidayPaymentResponse
            {
                AmountToPay = source.AmountToPay,
                Status = source.Status,
                PaymentLink = source.PaymentLink,
                RemoveLink = source.RemoveLink
            };
        }
    }
}
