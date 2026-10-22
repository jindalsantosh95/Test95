using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Constants;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.FlownDB.Interface;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.VSRetro
{
    public class UpdateVATicketStatusCommandHandler : IRequestHandler<UpdateVATicketStatusCommand, Result<Response<string>>>
    {
        private readonly ILogger<UpdateVATicketStatusCommandHandler> _logger;
        private readonly IFlownDbRepository _flownDBRepository;

        public UpdateVATicketStatusCommandHandler(IFlownDbRepository flownDBRepository, ILogger<UpdateVATicketStatusCommandHandler> logger)
        {
            _flownDBRepository = flownDBRepository ?? throw new ArgumentNullException(nameof(flownDBRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(UpdateVATicketStatusCommand request, CancellationToken cancellationToken)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));

            try
            {
                var updateSucceeded = await _flownDBRepository.UpdateTicketStatus(request.TicketId, request.TicketNumber, request.TicketStatus, request.MemberNumber, request.LoyaltyProgrammeCode, request.PostingTypeCode);

                if (!updateSucceeded)
                {
                    _logger.LogWarning("VS retro claim flown-data update was rejected for record {FlownRecordId}", request.TicketId);
                }

                if (updateSucceeded)
                    _logger.LogInformation("VS retro claim flown-data update completed for record {FlownRecordId}", request.TicketId);

                return new Response<string>
                {
                    Value = updateSucceeded ? Constants.Application.SuccessStatusUpdate : $"{Constants.Application.FailureStatusUpdate}against Ticket no:{request.TicketNumber}",
                    Status = Status.OK
                };
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while updating VS retro claim flown data for record {FlownRecordId}", request.TicketId);

                return new Response<string>() { Status = Status.BusinessException, Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.UpdateTicketStatusError), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath) };

            }
        }
    }
}
