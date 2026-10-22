using Microsoft.Extensions.Logging.Abstractions;
using System.Globalization;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.EventHandlers;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Exceptions;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.FlownPassenger;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.RetroCredit;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.DataModels;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.FlownDB.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Tests
{
    public class AutoAccrualFeedbackOutboundEventHandlerTests
    {
        [Fact]
        public async Task Handle_ReturnsTrue_WhenValidFeedbackIsPersisted()
        {
            var flownDbRepository = new FakeFlownDbRepository { UpdateResult = true };
            var handler = CreateHandler(flownDbRepository);
            var message = CreateValidMessage();

            var result = await handler.Handle(message);

            Assert.True(result);
            Assert.Same(message, flownDbRepository.UpdatedMessage);
        }

        [Fact]
        public async Task Handle_ThrowsCannotCorrelateException_WhenFlownDbDoesNotUpdateRecord()
        {
            var flownDbRepository = new FakeFlownDbRepository { UpdateResult = false };
            var handler = CreateHandler(flownDbRepository);

            await Assert.ThrowsAsync<AutoAccrualFeedbackCannotBeCorrelatedException>(
                () => handler.Handle(CreateValidMessage()));
        }

        [Fact]
        public async Task Handle_Rethrows_WhenFlownDbThrows()
        {
            var flownDbRepository = new FakeFlownDbRepository { ExceptionToThrow = new InvalidOperationException("Flown DB is unavailable.") };
            var handler = CreateHandler(flownDbRepository);

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(CreateValidMessage()));
        }

        private static AutoAccrualFeedbackOutboundEventHandler CreateHandler(FakeFlownDbRepository flownDbRepository) =>
            new(NullLogger<AutoAccrualFeedbackOutboundEventHandler>.Instance, flownDbRepository);

        private static DistanceBasedAccrualFeedbackPayload CreateValidMessage() => new()
        {
            SchemaId = Constants.Application.FeedbackDbaSchemaId,
            SchemaVersion = Constants.Application.FeedbackDbaSchemaVersion,
            Producer = "VN",
            Consumer = "VS",
            RequestType = Constants.Application.AutoAccrualRequestType,
            AccrualType = Constants.Application.DistanceBasedAccrualType,
            UniqueIdentification = new UniqueIdentification { UniqueIdentificationNumber = "123", AirlineCode = "VS" },
            PostingDetails = new PostingDetails { PostingStatus = "Y", PostingDateTime = CreateUtcTimestamp() },
            MileDetails = new MileDetails { BaseMiles = 1000 },
            ProductCode = "FLT"
        };

        private static string CreateUtcTimestamp() =>
            DateTime.UtcNow.ToString(Constants.Application.EventTimeFormat, CultureInfo.InvariantCulture);

        private sealed class FakeFlownDbRepository : IFlownDbRepository
        {
            public bool UpdateResult { get; set; }
            public Exception? ExceptionToThrow { get; set; }
            public DistanceBasedAccrualFeedbackPayload? UpdatedMessage { get; private set; }

            public Task<bool> UpdateFlownRecordWithAutoAccrualFeedbackAsync(DistanceBasedAccrualFeedbackPayload message)
            {
                UpdatedMessage = message;
                return ExceptionToThrow is null ? Task.FromResult(UpdateResult) : Task.FromException<bool>(ExceptionToThrow);
            }

            public Task<RetroCreditFlownDetails> GetFlownDetailsForRetroCreditAsync(RetroCreditRequest retroCreditRequest) => throw new NotSupportedException();
            public Task<bool> UpdateFlownDetailsForEligibleRetroAsync(RetroCreditRequest retroCreditRequest, string uniqueIdentificationNumber) => throw new NotSupportedException();
            public Task<bool> UpdateFlownDetailsWithRetroFeedbackAsync(RetroFeedbackRequest retroFeedbackRequest) => throw new NotSupportedException();
            public Task<bool> ResetPendingFeedbackAutoRetroAsync(string days) => throw new NotSupportedException();
            public Task<IReadOnlyCollection<FlownPassengerRequest>> InsertFlownPassengerDetailsAsync(IEnumerable<FlownPassengerRequest> flownPassengerRequest, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public Task<bool> UpdateTicketStatus(string id, string ticketNumber, string ticketStatus, string? memberNumber, string? loyaltyProgrammeCode, string? postingTypeCode) => throw new NotSupportedException();
            public Task<IEnumerable<AutoAccrualFlownDetails>> GetFlownDetailsForAutoAccrualAsync(AutoAccrualPayload autoAccrualRequest, string ffpCarrier, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task<IEnumerable<AutoAccrualFlownDetails>> GetFlownDetailsForAutoAccrualFallbackAsync(AutoAccrualFallbackCriteria criteria, CancellationToken cancellationToken) => throw new NotSupportedException();
        }

    }
}
