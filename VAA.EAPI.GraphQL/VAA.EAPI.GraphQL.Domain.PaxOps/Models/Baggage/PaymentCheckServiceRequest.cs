using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Baggage
{
    public class PaymentCheckServiceRequest
    {
        public string TravelPartyId { get; set; }
        public bool? PaymentPending { get; set; }

        public static async Task<PaymentCheckServiceRequest> CreateRequestAsync(string checkInId, bool? paymentPending)
        {
            return new PaymentCheckServiceRequest
            {
                TravelPartyId = await CheckInIdHelper.GetTravelPartyId(checkInId),
                PaymentPending = paymentPending
            };
        }
    }
}
