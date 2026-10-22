using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagement.Infrastructure.Repositories.VSRetro.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Constants;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.VSRetro;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagement.Application.Query.VSRetro
{
    public class VSRetroFlownDetailsQueryHandler : IRequestHandler<VSRetroFlownDetailsQuery, Result<Response<VSFlownDetailsResponse>>>
    {
        private readonly ILogger<VSRetroFlownDetailsQueryHandler> _logger;
        private readonly IVSRetroFlownDBRepository _flownDBRepository;

        public VSRetroFlownDetailsQueryHandler(
            ILogger<VSRetroFlownDetailsQueryHandler> logger,
            IVSRetroFlownDBRepository flownDBRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _flownDBRepository = flownDBRepository ?? throw new ArgumentNullException(nameof(flownDBRepository));
        }

        public async Task<Result<Response<VSFlownDetailsResponse>>> Handle(VSRetroFlownDetailsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var hasTicketNumber = !request.TicketNumber.IsNullOrWhiteSpace();
            var hasPnrAndFlightNumber = !request.Pnr.IsNullOrWhiteSpace() && !request.FlightNumber.IsNullOrWhiteSpace();

            var hasAnyPnrOrFlight = !request.Pnr.IsNullOrWhiteSpace() || !request.FlightNumber.IsNullOrWhiteSpace();

            // Enforce: either ticketNumber OR (pnr + flightNumber)
            // - ticketNumber mode: ticketNumber provided AND pnr/flightNumber omitted
            // - pnr/flightNumber mode: ticketNumber omitted AND both pnr and flightNumber provided
            if ((hasTicketNumber && hasAnyPnrOrFlight) || (!hasTicketNumber && !hasPnrAndFlightNumber))
            {
                return CreateRetroRequestErrorResponse(ErrorCodes.RetroRequestValidationError);
            }

            try
            {
                var response = await _flownDBRepository.GetVSRetroFlownDetails(request.TicketNumber ?? string.Empty, request.Pnr ?? string.Empty, request.FlightNumber ?? string.Empty);
                return Result.Success(
                    new Response<VSFlownDetailsResponse>
                    {
                        Value = response,
                        Status = Status.OK
                    });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Business exception while fetching VS Retro Flown Details: {Message}", ex.Message);
                return CreateRetroRequestErrorResponse(ErrorCodes.RetroRequestNoData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching VS Retro Flown Details: {Message} {InnerException}", ex.Message, ex.InnerException);
                throw;
            }
        }

        private static Result<Response<VSFlownDetailsResponse>> CreateRetroRequestErrorResponse(string errorCode)
        {
            return new Response<VSFlownDetailsResponse>
            {
                Status = Status.BusinessException,
                Errors = ErrorMapper.MapBusinessExceptionToEAPIException(
                    new Exception(errorCode),
                    Guid.NewGuid().ToString(),
                    VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
            };
        }
    }
}
