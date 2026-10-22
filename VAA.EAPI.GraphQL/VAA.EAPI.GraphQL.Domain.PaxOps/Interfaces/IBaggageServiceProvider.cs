using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.BaggageMS;


namespace VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces
{
    public interface IBaggageServiceProvider
    {
        Task<BaggageResponse> GetBaggageAsync(string checkInId, bool? hidePaidBaggage);
        Task<SaveBaggageResponse> SaveBaggageAsync(Models.Baggage.SaveBaggageServiceRequest request);
        Task<PaymentEligibilityCheckResponse> PaymentEligibilityCheckAsync(Models.Baggage.PaymentCheckServiceRequest request);
        Task<ConfirmBaggageResponse> ReviewPaymentDetails(Models.Baggage.ReviewPayDetailsRequest request);
    }
}
