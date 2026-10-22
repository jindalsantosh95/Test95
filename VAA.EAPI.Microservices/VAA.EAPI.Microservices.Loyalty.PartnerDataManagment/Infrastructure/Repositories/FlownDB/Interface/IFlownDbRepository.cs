using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.FlownPassenger;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.RetroCredit;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.DataModels;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.FlownDB.Interface
{
    public interface IFlownDbRepository
    {
        public Task<RetroCreditFlownDetails> GetFlownDetailsForRetroCreditAsync(RetroCreditRequest retroCreditRequest);
        public Task<bool> UpdateFlownDetailsForEligibleRetroAsync(RetroCreditRequest retroCreditRequest, string uniqueNumber);
        public Task<bool> UpdateFlownDetailsWithRetroFeedbackAsync(RetroFeedbackRequest retroFeedbackRequest);
        public Task<bool> ResetPendingFeedbackAutoRetroAsync(string Days);
        public Task<bool> UpdateTicketStatus(string id, string ticketNumber, string ticketStatus, string? memberNumber, string? loyaltyProgrammeCode, string? postingTypeCode);
        public Task<IReadOnlyCollection<FlownPassengerRequest>> InsertFlownPassengerDetailsAsync(IEnumerable<FlownPassengerRequest> flownPassengerRequest, CancellationToken cancellationToken = default);
        public Task<bool> UpdateFlownRecordWithAutoAccrualFeedbackAsync(DistanceBasedAccrualFeedbackPayload message);
        public Task<IEnumerable<AutoAccrualFlownDetails>> GetFlownDetailsForAutoAccrualAsync(AutoAccrualPayload autoAccrualRequest, string ffpCarrier, CancellationToken cancellationToken);
        public Task<IEnumerable<AutoAccrualFlownDetails>> GetFlownDetailsForAutoAccrualFallbackAsync(AutoAccrualFallbackCriteria criteria, CancellationToken cancellationToken);
    }
}
