using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.DeltaSsci;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.UpdateBags
{
    public class CheckPaymentRequest
    {
        public string TravelPartyId { get; set; }
        public bool PaymentPending { get; set; }

        public static explicit operator UpdateBagsRequest(CheckPaymentRequest request)
        {
            return new UpdateBagsRequest
            {
                PendingPayment = request.PaymentPending
            };
        }
    }
}
