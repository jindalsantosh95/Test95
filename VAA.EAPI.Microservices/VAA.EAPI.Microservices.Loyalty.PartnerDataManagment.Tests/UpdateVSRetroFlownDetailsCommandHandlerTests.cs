using Microsoft.Extensions.Logging.Abstractions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagement.Infrastructure.Repositories.VSRetro.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.VSRetro;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.VSRetro;
using Xunit;
using PartnerDataManagementConstants = VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Constants;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Tests
{
    public class UpdateVSRetroFlownDetailsCommandHandlerTests
    {
        [Fact]
        public async Task Handle_PassesAllRequestValuesToRepository_WhenUpdateSucceeds()
        {
            var repository = new RecordingVSRetroFlownDbRepository { UpdateSucceeded = true };
            var handler = new UpdateVSRetroFlownDetailsCommandHandler(repository, NullLogger<UpdateVSRetroFlownDetailsCommandHandler>.Instance);
            var updateRequest = CreateRequest();

            using var cancellationTokenSource = new CancellationTokenSource();
            var result = await handler.Handle(new UpdateVSRetroFlownDetailsCommand(updateRequest), cancellationTokenSource.Token);

            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal(PartnerDataManagementConstants.Application.FlownSuccessStatusUpdate, result.Value.Value.SuccessMessage);
            Assert.Equal(string.Empty, result.Value.Value.ErrorMessage);
            Assert.Same(updateRequest, repository.Request);
            Assert.Equal("LHR", repository.Request!.OriginAirport);
            Assert.Equal("JFK", repository.Request.DestinationAirport);
            Assert.Equal("9321234567890", repository.Request.TicketNumber);
            Assert.Equal("Allocated", repository.Request.Status);
            Assert.Equal("VS", repository.Request.VALoyaltyProgramCode);
            Assert.Equal("1074840875", repository.Request.MemberNumber);
            Assert.Equal(cancellationTokenSource.Token, repository.ReceivedCancellationToken);
        }

        [Fact]
        public async Task Handle_ReturnsBusinessException_WhenNoFlownRecordIsUpdated()
        {
            var repository = new RecordingVSRetroFlownDbRepository { UpdateSucceeded = false };
            var handler = new UpdateVSRetroFlownDetailsCommandHandler(repository, NullLogger<UpdateVSRetroFlownDetailsCommandHandler>.Instance);

            var result = await handler.Handle(new UpdateVSRetroFlownDetailsCommand(CreateRequest()), CancellationToken.None);

            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.Null(result.Value.Value);
            var error = Assert.Single(result.Value.Errors);
            Assert.Equal(PartnerDataManagementConstants.ErrorCodes.RetroRequestNoData, error.Code);
        }

        [Fact]
        public async Task Handle_AllowsEmptyOptionalLoyaltyFields()
        {
            var repository = new RecordingVSRetroFlownDbRepository { UpdateSucceeded = true };
            var handler = new UpdateVSRetroFlownDetailsCommandHandler(repository, NullLogger<UpdateVSRetroFlownDetailsCommandHandler>.Instance);
            var updateRequest = CreateRequest();
            updateRequest.VALoyaltyProgramCode = string.Empty;
            updateRequest.MemberNumber = string.Empty;

            await handler.Handle(new UpdateVSRetroFlownDetailsCommand(updateRequest), CancellationToken.None);

            Assert.Equal(string.Empty, repository.Request!.VALoyaltyProgramCode);
            Assert.Equal(string.Empty, repository.Request.MemberNumber);
        }

        [Fact]
        public async Task Handle_ReturnsTechnicalError_WhenRepositoryThrows()
        {
            var repository = new RecordingVSRetroFlownDbRepository
            {
                ExceptionToThrow = new InvalidOperationException("Database unavailable")
            };
            var handler = new UpdateVSRetroFlownDetailsCommandHandler(repository, NullLogger<UpdateVSRetroFlownDetailsCommandHandler>.Instance);

            var result = await handler.Handle(new UpdateVSRetroFlownDetailsCommand(CreateRequest()), CancellationToken.None);

            Assert.Equal(Status.Error, result.Value.Status);
            Assert.Null(result.Value.Value);
            var error = Assert.Single(result.Value.Errors);
            Assert.Equal(PartnerDataManagementConstants.ErrorCodes.UpdateTicketStatusError, error.Code);
        }

        [Fact]
        public async Task Handle_RethrowsOperationCanceledException_WhenRepositoryOperationIsCancelled()
        {
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            var repository = new RecordingVSRetroFlownDbRepository
            {
                ExceptionToThrow = new OperationCanceledException(cancellationTokenSource.Token)
            };
            var handler = new UpdateVSRetroFlownDetailsCommandHandler(repository, NullLogger<UpdateVSRetroFlownDetailsCommandHandler>.Instance);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                handler.Handle(new UpdateVSRetroFlownDetailsCommand(CreateRequest()), cancellationTokenSource.Token));

            Assert.Equal(cancellationTokenSource.Token, repository.ReceivedCancellationToken);
        }

        [Theory]
        [InlineData(nameof(UpdateVSFlownDetailsRequest.OriginAirport))]
        [InlineData(nameof(UpdateVSFlownDetailsRequest.DestinationAirport))]
        [InlineData(nameof(UpdateVSFlownDetailsRequest.TicketNumber))]
        [InlineData(nameof(UpdateVSFlownDetailsRequest.Status))]
        public async Task Handle_ReturnsBadRequestWithoutUpdating_WhenRequiredInputIsMissing(string missingProperty)
        {
            var repository = new RecordingVSRetroFlownDbRepository { UpdateSucceeded = true };
            var handler = new UpdateVSRetroFlownDetailsCommandHandler(repository, NullLogger<UpdateVSRetroFlownDetailsCommandHandler>.Instance);
            var updateRequest = CreateRequest();

            switch (missingProperty)
            {
                case nameof(UpdateVSFlownDetailsRequest.OriginAirport):
                    updateRequest.OriginAirport = string.Empty;
                    break;
                case nameof(UpdateVSFlownDetailsRequest.DestinationAirport):
                    updateRequest.DestinationAirport = string.Empty;
                    break;
                case nameof(UpdateVSFlownDetailsRequest.TicketNumber):
                    updateRequest.TicketNumber = string.Empty;
                    break;
                case nameof(UpdateVSFlownDetailsRequest.Status):
                    updateRequest.Status = string.Empty;
                    break;
            }

            var result = await handler.Handle(new UpdateVSRetroFlownDetailsCommand(updateRequest), CancellationToken.None);

            Assert.Equal(Status.BadRequest, result.Value.Status);
            var error = Assert.Single(result.Value.Errors);
            Assert.Equal(PartnerDataManagementConstants.ErrorCodes.UpdateFlownDataError, error.Code);
            Assert.Null(repository.Request);
        }

        private static UpdateVSFlownDetailsRequest CreateRequest() => new()
        {
            OriginAirport = "LHR",
            DestinationAirport = "JFK",
            TicketNumber = "9321234567890",
            Status = "Allocated",
            VALoyaltyProgramCode = "VS",
            MemberNumber = "1074840875"
        };

        private sealed class RecordingVSRetroFlownDbRepository : IVSRetroFlownDBRepository
        {
            public bool UpdateSucceeded { get; init; }
            public Exception? ExceptionToThrow { get; init; }
            public UpdateVSFlownDetailsRequest? Request { get; private set; }
            public CancellationToken ReceivedCancellationToken { get; private set; }

            public Task<bool> UpdateVSRetroFlownDetailsAsync(UpdateVSFlownDetailsRequest request, CancellationToken cancellationToken)
            {
                Request = request;
                ReceivedCancellationToken = cancellationToken;

                if (ExceptionToThrow is not null)
                {
                    throw ExceptionToThrow;
                }

                return Task.FromResult(UpdateSucceeded);
            }

            public Task<VSFlownDetailsResponse> GetVSFlownDetailsByTicketNumber(string ticketNumber) => throw new NotSupportedException();
            public Task<VSFlownDetailsResponse> GetVSRetroFlownDetails(string ticketNumber, string pnr, string flightNumber) => throw new NotSupportedException();
        }
    }
}
