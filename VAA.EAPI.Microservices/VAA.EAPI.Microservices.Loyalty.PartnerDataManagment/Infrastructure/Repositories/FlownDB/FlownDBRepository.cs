using Microsoft.Extensions.Logging;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.FlownPassenger;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.RetroCredit;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.DataModels;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.FlownDB.Interface;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.FlownDB
{
    public class FlownDBRepository : IFlownDbRepository
    {
        private readonly IFlownDbServices _flownDBServices;

        private readonly ILogger<FlownDBRepository> _logger;

        public FlownDBRepository(IFlownDbServices flownDBservice, ILogger<FlownDBRepository> logger)
        {
            _flownDBServices = flownDBservice ?? throw new ArgumentNullException(nameof(IFlownDbServices));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public async Task<IEnumerable<AutoAccrualFlownDetails>> GetFlownDetailsForAutoAccrualAsync(AutoAccrualPayload autoAccrualRequest, string ffpCarrier, CancellationToken cancellationToken)
        {
            return await _flownDBServices.GetFlownDetailsForAutoAccrualAsync(autoAccrualRequest, ffpCarrier, cancellationToken);
        }

        public Task<bool> UpdateFlownRecordWithAutoAccrualFeedbackAsync(DistanceBasedAccrualFeedbackPayload message) =>
            _flownDBServices.UpdateFlownRecordWithAutoAccrualFeedbackAsync(message);

        public async Task<RetroCreditFlownDetails> GetFlownDetailsForRetroCreditAsync(RetroCreditRequest retroCreditRequest)
        {

            var getPassengerResponse = await _flownDBServices.GetUniquePassengerFlownDetailsForRetroCreditAsync(retroCreditRequest);

            return getPassengerResponse;
        }



        public async Task<bool> ResetPendingFeedbackAutoRetroAsync(string Days)
        {
            var getPassengerResponse = await _flownDBServices.ResetPendingFeedbackFlownDetailsAsync(Days);

            return getPassengerResponse;
        }

        public async Task<bool> UpdateFlownDetailsForEligibleRetroAsync(RetroCreditRequest retroCreditRequest, string uniqueIdentificationNumber)
        {
            bool updateResponse = await _flownDBServices.UpdateFlownDetailsForEligibleRetroAsync(uniqueIdentificationNumber, retroCreditRequest);

            return updateResponse;
        }

        public async Task<bool> UpdateFlownDetailsWithRetroFeedbackAsync(RetroFeedbackRequest retroFeedbackRequest)
        {
            bool getfeedbackResponse = await _flownDBServices.UpdateFlownDetailsWithFeedbackAsync(retroFeedbackRequest);

            return getfeedbackResponse;
        }

        public async Task<bool> UpdateTicketStatus(string id, string ticketNumber, string ticketStatus, string? memberNumber, string? loyaltyProgrammeCode, string? postingTypeCode)
        {
            return await _flownDBServices.UpdateTicketStatus(Convert.ToInt32(id), ticketNumber, ticketStatus, memberNumber, loyaltyProgrammeCode, postingTypeCode);
        }
        public async Task<IReadOnlyCollection<FlownPassengerRequest>> InsertFlownPassengerDetailsAsync(IEnumerable<FlownPassengerRequest> flownPassengerRequest, CancellationToken cancellationToken = default)
        {
            return await _flownDBServices.InsertFlownPassengerDetailsAsync(flownPassengerRequest, cancellationToken);
        }

        public async Task<IEnumerable<AutoAccrualFlownDetails>> GetFlownDetailsForAutoAccrualFallbackAsync(AutoAccrualFallbackCriteria criteria, CancellationToken cancellationToken)
        {
            return await _flownDBServices.GetFlownDetailsForAutoAccrualFallbackAsync(criteria, cancellationToken);
        }
    }
}
