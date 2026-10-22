using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS.PaymentDeails;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Seats;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Payment
{
    [GraphQLName("tripsPaymentResponse")]
    public class PaymentResponse
    {
        public string PaymentReferenceId { get; set; }
        public string HppLink { get; set; }
        public CurrencyAmount Amount { get; set; }
        public IEnumerable<PaymentTransactionDetails> PaymentDetails { get; set; }
        public string ParentPaymentReferenceId { get; set; }
        public string PaymentURL { get; set; }

        public static explicit operator PaymentResponse(Basket basket)
        {
            var selectedSeats = basket.FlightPassengerSeatMap.Where(seat => !string.IsNullOrWhiteSpace(seat.SeatNumber));
            var paymentSession = basket.PaymentSessions?.FirstOrDefault();

            return new PaymentResponse()
            {
                PaymentReferenceId = paymentSession?.PaymentReferenceId,
                HppLink = paymentSession?.PaymentLink,
                Amount = new CurrencyAmount()
                {
                    MonetaryAmount = selectedSeats?.Sum(seat => seat.Price != null ? Convert.ToDecimal(seat.Price?.Amount) : 0m) ?? 0m,
                    BaseCurrency = selectedSeats?.FirstOrDefault()?.Price?.Currency ?? ""
                },
            };
        }

        public static explicit operator PaymentResponse((Basket basket, PaymentRefIdResponseMs refIdResponseMs) source)
        {
            var (basket, refIdResponseMs) = source;

            var selectedSeats = basket.FlightPassengerSeatMap.Where(seat => !string.IsNullOrWhiteSpace(seat.SeatNumber));
            var paymentSession = basket.PaymentSessions?.FirstOrDefault();

            return new PaymentResponse()
            {
                PaymentReferenceId = paymentSession?.PaymentReferenceId,
                HppLink = paymentSession?.PaymentLink,
                Amount = new CurrencyAmount()
                {
                    MonetaryAmount = selectedSeats?.Sum(seat => seat.Price != null ? Convert.ToDecimal(seat.Price?.Amount) : 0m) ?? 0m,
                    BaseCurrency = selectedSeats?.FirstOrDefault()?.Price?.Currency ?? ""
                },
                PaymentURL = refIdResponseMs?.PaymentURL,
            };
        }
    }
}