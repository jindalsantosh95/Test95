using HotChocolate;
using System.Web;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS
{
    [GraphQLName("tripsBasketPaymentSession")]
    public class PaymentSession
    {
        public decimal AmountToPay { get; set; }
        public string PaymentLink { get; set; }
        public string PaymentReferenceId { get; set; }
        public string HppPaymentStatus { get; set; }

        public static explicit operator PaymentSession((PaymentRefIdResponseMs response, PaymentRequestMs request) source)
        {
            var (response, request) = source;
            Uri deltaHPPPaymentUri = new Uri(response.HPPLink);
            var paymentReferenceId = HttpUtility.ParseQueryString(deltaHPPPaymentUri.Query).Get("paymentReferenceId");

            return new()
            {
                AmountToPay = request.SeatDetails.Sum(a => Convert.ToDecimal(a.PendingSeatPriceAmount)),
                PaymentLink = response.HPPLink,
                HppPaymentStatus = "PENDING",
                PaymentReferenceId = paymentReferenceId
            };
        }
    }
}