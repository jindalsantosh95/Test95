using Microsoft.Extensions.Logging.Abstractions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.VSRetro;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.FlownPassenger;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.RetroCredit;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.VSRetro;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.FlownDB.Interface;
using Xunit;
using PartnerDataManagementConstants = VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Constants;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Tests
{
    public sealed class UpdateVATicketStatusCommandHandlerTests
    {
        [Fact]
        public async Task Handle_UpdatesLoyaltyFields_WhenDatabaseAcceptsTheClaim()
        {
            var repository = new RecordingFlownDbRepository { UpdateSucceeded = true };
            var handler = new UpdateVATicketStatusCommandHandler(repository, NullLogger<UpdateVATicketStatusCommandHandler>.Instance);

            var result = await handler.Handle(CreateCommand(), CancellationToken.None);

            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal(PartnerDataManagementConstants.Application.SuccessStatusUpdate, result.Value.Value);
            Assert.Equal("42", repository.FlownRecordId);
            Assert.Equal("9321234567890", repository.TicketNumber);
            Assert.Equal("Allocated", repository.TicketStatus);
            Assert.Equal("1074840875", repository.MemberNumber);
            Assert.Equal("VS", repository.LoyaltyProgrammeCode);
            Assert.Equal("RT", repository.PostingTypeCode);
        }

        [Fact]
        public async Task Handle_ReturnsExistingFailureMessage_WhenDatabaseUpdateFails()
        {
            var repository = new RecordingFlownDbRepository { UpdateSucceeded = false };
            var handler = new UpdateVATicketStatusCommandHandler(repository, NullLogger<UpdateVATicketStatusCommandHandler>.Instance);

            var result = await handler.Handle(CreateCommand(), CancellationToken.None);

            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal($"{PartnerDataManagementConstants.Application.FailureStatusUpdate}against Ticket no:9321234567890", result.Value.Value);
        }

        [Fact]
        public async Task Handle_AllowsExistingPayloadWithoutLoyaltyFields()
        {
            var repository = new RecordingFlownDbRepository { UpdateSucceeded = true };
            var handler = new UpdateVATicketStatusCommandHandler(repository, NullLogger<UpdateVATicketStatusCommandHandler>.Instance);
            var command = new UpdateVATicketStatusCommand(new TicketStatusRequest
            {
                FlownRecordId = "42",
                TicketNumber = "9321234567890",
                TicketStatus = "Allocated"
            });

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal(PartnerDataManagementConstants.Application.SuccessStatusUpdate, result.Value.Value);
            Assert.Null(repository.MemberNumber);
            Assert.Null(repository.LoyaltyProgrammeCode);
            Assert.Null(repository.PostingTypeCode);
        }

        private static UpdateVATicketStatusCommand CreateCommand() => new(new TicketStatusRequest
        {
            FlownRecordId = "42",
            TicketNumber = "9321234567890",
            TicketStatus = "Allocated",
            MemberNumber = "1074840875",
            LoyaltyProgrammeCode = "VS",
            PostingTypeCode = "RT"
        });

        private sealed class RecordingFlownDbRepository : IFlownDbRepository
        {
            public bool UpdateSucceeded { get; init; }
            public string? FlownRecordId { get; private set; }
            public string? TicketNumber { get; private set; }
            public string? TicketStatus { get; private set; }
            public string? MemberNumber { get; private set; }
            public string? LoyaltyProgrammeCode { get; private set; }
            public string? PostingTypeCode { get; private set; }

            public Task<bool> UpdateTicketStatus(string id, string ticketNumber, string ticketStatus, string? memberNumber, string? loyaltyProgrammeCode, string? postingTypeCode)
            {
                FlownRecordId = id;
                TicketNumber = ticketNumber;
                TicketStatus = ticketStatus;
                MemberNumber = memberNumber;
                LoyaltyProgrammeCode = loyaltyProgrammeCode;
                PostingTypeCode = postingTypeCode;
                return Task.FromResult(UpdateSucceeded);
            }

            public Task<Infrastructure.DataModels.RetroCreditFlownDetails> GetFlownDetailsForRetroCreditAsync(RetroCreditRequest retroCreditRequest) => throw new NotSupportedException();
            public Task<bool> UpdateFlownDetailsForEligibleRetroAsync(RetroCreditRequest retroCreditRequest, string uniqueNumber) => throw new NotSupportedException();
            public Task<bool> UpdateFlownDetailsWithRetroFeedbackAsync(RetroFeedbackRequest retroFeedbackRequest) => throw new NotSupportedException();
            public Task<bool> ResetPendingFeedbackAutoRetroAsync(string days) => throw new NotSupportedException();
            public Task<IReadOnlyCollection<FlownPassengerRequest>> InsertFlownPassengerDetailsAsync(IEnumerable<FlownPassengerRequest> flownPassengerRequest, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public Task<IEnumerable<AutoAccrualFlownDetails>> GetFlownDetailsForAutoAccrualAsync(AutoAccrualPayload autoAccrualRequest, string ffpCarrier, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task<IEnumerable<AutoAccrualFlownDetails>> GetFlownDetailsForAutoAccrualFallbackAsync(AutoAccrualFallbackCriteria criteria, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task<bool> UpdateFlownRecordWithAutoAccrualFeedbackAsync(DistanceBasedAccrualFeedbackPayload message) => throw new NotSupportedException();
        }
    }
}
