using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.FlownPassenger;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.RetroCredit;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.DataModels;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.FlownDB.Interface
{
    public interface IFlownDbServices
    {
        public Task<RetroCreditFlownDetails> GetUniquePassengerFlownDetailsForRetroCreditAsync(RetroCreditRequest retroCreditRequest);
        public Task<bool> UpdateFlownDetailsForEligibleRetroAsync(string uniqueIdentificationNumber, RetroCreditRequest retroCreditRequest);
        public Task<bool> UpdateFlownDetailsWithFeedbackAsync(RetroFeedbackRequest retroFeedbackRequest);
        public Task<bool> ResetPendingFeedbackFlownDetailsAsync(string Days);
        public Task<bool> UpdateTicketStatus(int id, string ticketNumber, string claimStatus, string? memberNumber, string? loyaltyProgrammeCode, string? postingTypeCode);
        public Task<IReadOnlyCollection<FlownPassengerRequest>> InsertFlownPassengerDetailsAsync(IEnumerable<FlownPassengerRequest> flownPassengerRequest, CancellationToken cancellationToken = default);
        public Task<IEnumerable<AutoAccrualFlownDetails>> GetFlownDetailsForAutoAccrualAsync(AutoAccrualPayload autoAccrualRequest, string ffpCarrier, CancellationToken cancellationToken);
        public Task<IEnumerable<AutoAccrualFlownDetails>> GetFlownDetailsForAutoAccrualFallbackAsync(AutoAccrualFallbackCriteria criteria, CancellationToken cancellationToken);
        public Task<bool> UpdateFlownRecordWithAutoAccrualFeedbackAsync(DistanceBasedAccrualFeedbackPayload message);
    }
}
