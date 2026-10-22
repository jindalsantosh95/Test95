using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagement.Infrastructure.Repositories.VSRetro.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Constants;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.VSRetro;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.VSRetro
{
    public class UpdateVSRetroFlownDetailsCommandHandler : IRequestHandler<UpdateVSRetroFlownDetailsCommand, Result<Response<UpdateVSFlownDetailsResponse>>>
    {
        private readonly IVSRetroFlownDBRepository _flownDbRepository;
        private readonly ILogger<UpdateVSRetroFlownDetailsCommandHandler> _logger;

        public UpdateVSRetroFlownDetailsCommandHandler(
            IVSRetroFlownDBRepository flownDbRepository,
            ILogger<UpdateVSRetroFlownDetailsCommandHandler> logger)
        {
            _flownDbRepository = flownDbRepository ?? throw new ArgumentNullException(nameof(flownDbRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<UpdateVSFlownDetailsResponse>>> Handle(UpdateVSRetroFlownDetailsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                if (request.Request.OriginAirport.IsNullOrWhiteSpace()
                    || request.Request.DestinationAirport.IsNullOrWhiteSpace()
                    || request.Request.Status.IsNullOrWhiteSpace() || request.Request.TicketNumber.IsNullOrWhiteSpace())
                {
                    return new Response<UpdateVSFlownDetailsResponse>
                    {
                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(
                        ErrorCodes.UpdateFlownDataError,
                        Guid.NewGuid().ToString(),
                        EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
                        Status = Status.BadRequest
                    };
                }

                var updateSucceeded = await _flownDbRepository.UpdateVSRetroFlownDetailsAsync(request.Request, cancellationToken);

                if (!updateSucceeded)
                {
                    return new Response<UpdateVSFlownDetailsResponse>
                    {
                        Status = Status.BusinessException,
                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(
                            new Exception(ErrorCodes.RetroRequestNoData),
                            Guid.NewGuid().ToString(),
                            VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                    };
                }

                return new Response<UpdateVSFlownDetailsResponse>
                {
                    Status = Status.OK,
                    Value = new UpdateVSFlownDetailsResponse
                    {
                        SuccessMessage = Constants.Application.FlownSuccessStatusUpdate,
                        ErrorMessage = string.Empty
                    }
                };
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while updating VS Retro flown details");

                return new Response<UpdateVSFlownDetailsResponse>
                {
                    Status = Status.Error,
                    Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(
                         ErrorCodes.UpdateTicketStatusError,
                        Guid.NewGuid().ToString(),
                        VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                };
            }
        }
    }
}
