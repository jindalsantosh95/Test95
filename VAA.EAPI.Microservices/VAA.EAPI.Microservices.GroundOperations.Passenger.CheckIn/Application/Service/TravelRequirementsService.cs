using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Application.Interface;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.DeltaTrm;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.TravelRequirements;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Helper;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Application.Service
{
    public class TravelRequirementsService(IDeltaTrmService deltaTrmService, ILogger<TravelRequirementsService> logger) : ITravelRequirementsService
    {
        private readonly IDeltaTrmService _deltaTrmService = deltaTrmService ?? throw new ArgumentNullException(nameof(deltaTrmService));
        private readonly ILogger<TravelRequirementsService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<Result<Response<TravelRequirementsResponse>>> GetTravelRequirementsAsync(TravelRequirementsRequest travelRequirementsRequest)
        {
            ArgumentNullException.ThrowIfNull(travelRequirementsRequest);
            ArgumentException.ThrowIfNullOrEmpty(travelRequirementsRequest.PnrLocator);
            ArgumentException.ThrowIfNullOrEmpty(travelRequirementsRequest.GivenNames);
            ArgumentException.ThrowIfNullOrEmpty(travelRequirementsRequest.Surname);

            TravelRequirementsResponse responseObj;

            try
            {
                var trmResponse = await _deltaTrmService.PostRequirementsAsync((RequirementsRequest)travelRequirementsRequest);

                responseObj = (TravelRequirementsResponse)trmResponse;
            }
            catch (HttpRequestException ex)
            {
                var error = ErrorHelper.TravelRequirementsErrorMapper(ex, _logger);

                _logger.LogError("Travel requirements retrieval operation failed with error code {ErrorCode}.", error.Code);

                return Result.Success(new Response<TravelRequirementsResponse>
                {
                    Status = EAPI.Domain.Entities.Common.Status.BusinessException,
                    Errors = [error]
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Travel requirements retrieval operation failed for {PnrLocator}.", travelRequirementsRequest?.PnrLocator);

                return Result.Failure<Response<TravelRequirementsResponse>>(
                    $"Travel requirements retrieval operation failed for {travelRequirementsRequest?.PnrLocator}.");
            }

            return Result.Success(new Response<TravelRequirementsResponse> { Status = EAPI.Domain.Entities.Common.Status.OK, Value = responseObj });
        }

        public async Task<Result<Response<TravelRequirementsResponse>>> SavePassportAsync(SavePassportRequest savePassportRequest)
        {
            ArgumentNullException.ThrowIfNull(savePassportRequest);

            TravelRequirementsResponse responseObj;

            try
            {
                var trmResponse = await _deltaTrmService.PatchSingleRequirementsAsync(savePassportRequest.RequirementsId!, (RequirementFormRequest)savePassportRequest);

                responseObj = (TravelRequirementsResponse)trmResponse;

                var passportError = responseObj.Passengers.FirstOrDefault(x => x.PassengerId.Is(savePassportRequest.PassengerId))?.SliceRequirements?.SelectMany(x => x.PassportErrors ?? []) ?? [];

                foreach (var error in passportError)
                {
                    if (Constants.Application.PassportValidationErrors.Contains(error.Code))
                    {
                        _logger.LogError("Save passport for travel requirements failed for {RequirementsId} with error code {ErrorCode}.", savePassportRequest?.RequirementsId, error?.Code);

                        return Result.Success(new Response<TravelRequirementsResponse> { Status = EAPI.Domain.Entities.Common.Status.BusinessException, Errors = [(ErrorDetails)error] });
                    }
                }                         
            }

            catch (HttpRequestException ex)
            {
                var error = ErrorHelper.TravelRequirementsErrorMapper(ex, _logger);

                _logger.LogError("Save passport for travel requirements failed for {RequirementsId} with error code {ErrorCode}.", savePassportRequest?.RequirementsId, error?.Code);

                return Result.Success(new Response<TravelRequirementsResponse> { Status = EAPI.Domain.Entities.Common.Status.BusinessException, Errors = [error]});
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Save passport for travel requirements failed for {RequirementsId}.", savePassportRequest?.RequirementsId);

                return Result.Failure<Response<TravelRequirementsResponse>>($"Save Passport for Travel Requirements failed for {savePassportRequest?.RequirementsId}.");
            }

            return Result.Success(new Response<TravelRequirementsResponse> { Status = EAPI.Domain.Entities.Common.Status.OK, Value = responseObj });
        }

        public async Task<Result<Response<TravelRequirementsResponse>>> SaveReturnDateAsync(SaveReturnDateRequest saveReturnDateRequest)
        {
            ArgumentNullException.ThrowIfNull(saveReturnDateRequest);

            TravelRequirementsResponse responseObj;

            try
            {
                var trmResponse = await _deltaTrmService.PatchSingleRequirementsAsync(saveReturnDateRequest.RequirementsId!, (RequirementFormRequest)saveReturnDateRequest);

                responseObj = (TravelRequirementsResponse)trmResponse;
            }
            catch (HttpRequestException ex)
            {
                var error = ErrorHelper.TravelRequirementsErrorMapper(ex, _logger);

                _logger.LogError("Save Return Date for Travel Requirements failed for {RequirementsId} with error code {ErrorCode}.", saveReturnDateRequest.RequirementsId, error.Code);

                return Result.Success(new Response<TravelRequirementsResponse> { Status = EAPI.Domain.Entities.Common.Status.BusinessException, Errors = [error] });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Save Return Date for Travel Requirements failed for {RequirementsId}.", saveReturnDateRequest?.RequirementsId);

                return Result.Failure<Response<TravelRequirementsResponse>>($"Save Return Date for Travel Requirements failed for {saveReturnDateRequest?.RequirementsId}.");
            }

            return Result.Success(new Response<TravelRequirementsResponse> { Status = EAPI.Domain.Entities.Common.Status.OK, Value = responseObj });
        }

        public async Task<Result<Response<TravelRequirementsResponse>>> SaveEmergencyContactAsync(SaveEmergencyContactRequest saveEmergencyContactRequest)
        {
            ArgumentNullException.ThrowIfNull(saveEmergencyContactRequest);

            TravelRequirementsResponse responseObj;

            try
            {
                var trmPatchRequest = (RequirementFormRequest)saveEmergencyContactRequest;

                var trmResponse = await _deltaTrmService.PatchBatchRequirementsAsync(saveEmergencyContactRequest.RequirementsId!, trmPatchRequest);

                responseObj = (TravelRequirementsResponse)trmResponse;
            }
            catch (HttpRequestException ex)
            {
                var error = ErrorHelper.TravelRequirementsErrorMapper(ex, _logger);

                _logger.LogError("Save Emergency Contact for Travel Requirements failed for {RequirementsId} with error code {ErrorCode}.", saveEmergencyContactRequest?.RequirementsId, error?.Code);

                return Result.Success(new Response<TravelRequirementsResponse> { Status = EAPI.Domain.Entities.Common.Status.BusinessException, Errors = [error] });
            }
            catch (Exception ex) when (ex is not ValidationException)
            {
                _logger.LogError(ex, "Save Emergency Contact for Travel Requirements failed for {RequirementsId}.", saveEmergencyContactRequest?.RequirementsId);

                return Result.Failure<Response<TravelRequirementsResponse>>($"Save Emergency Contact for Travel Requirements failed for {saveEmergencyContactRequest?.RequirementsId}.");
            }

            return Result.Success(new Response<TravelRequirementsResponse> { Status = EAPI.Domain.Entities.Common.Status.OK, Value = responseObj });
        }

        public async Task<Result<Response<TravelRequirementsResponse>>> SaveContactTracingAsync(SaveContactTracingRequest saveContactTracingRequest)
        {
            ArgumentNullException.ThrowIfNull(saveContactTracingRequest);

            TravelRequirementsResponse responseObj;

            try
            {
                var trmPatchRequest = (RequirementFormRequest)saveContactTracingRequest;

                var trmResponse = await _deltaTrmService.PatchBatchRequirementsAsync(saveContactTracingRequest.RequirementsId!, trmPatchRequest);

                responseObj = (TravelRequirementsResponse)trmResponse;
            }
            catch (HttpRequestException ex)
            {
                var error = ErrorHelper.TravelRequirementsErrorMapper(ex, _logger);

                _logger.LogError("Save Contact Tracing for Travel Requirements failed for {RequirementsId} with error code {ErrorCode}.", saveContactTracingRequest.RequirementsId, error.Code);

                return Result.Success(new Response<TravelRequirementsResponse> { Status = EAPI.Domain.Entities.Common.Status.BusinessException, Errors = [error] });
            }
            catch (Exception ex) when (ex is not ValidationException)
            {
                _logger.LogError(ex, "Save Contact Tracing for Travel Requirements failed for {RequirementsId}.", saveContactTracingRequest?.RequirementsId);

                return Result.Failure<Response<TravelRequirementsResponse>>($"Save Contact Tracing for Travel Requirements failed for {saveContactTracingRequest?.RequirementsId}.");
            }

            return Result.Success(new Response<TravelRequirementsResponse> { Status = EAPI.Domain.Entities.Common.Status.OK, Value = responseObj });
        }

        public async Task<Result<Response<TravelRequirementsResponse>>> SaveGovernmentFormsAttestationAsync(SaveGovernmentFormsAttestationRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var trmResponse = await _deltaTrmService.PatchSingleRequirementsAsync(
                    request.RequirementsId,
                    (RequirementFormRequest)request);

                return Result.Success(new Response<TravelRequirementsResponse>
                {
                    Status = EAPI.Domain.Entities.Common.Status.OK,
                    Value = (TravelRequirementsResponse)trmResponse
                });
            }
            catch (HttpRequestException ex)
            {
                var error = ErrorHelper.TravelRequirementsErrorMapper(ex, _logger);

                _logger.LogError(
                    "Save Government Forms attestation failed for Travel Requirements with error code {ErrorCode}.",
                    error.Code);

                return Result.Success(new Response<TravelRequirementsResponse>
                {
                    Status = EAPI.Domain.Entities.Common.Status.BusinessException,
                    Errors = [error]
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Save Government Forms attestation failed for Travel Requirements.");

                return Result.Failure<Response<TravelRequirementsResponse>>(
                    "Save Government Forms attestation failed for Travel Requirements.");
            }
        }

        public async Task<Result<Response<TravelRequirementsResponse>>> SaveSecondaryDocumentAttestationAsync(SaveSecondaryDocumentAttestationRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var trmResponse = await _deltaTrmService.PatchSingleRequirementsAsync(
                    request.RequirementsId,
                    (RequirementFormRequest)request);

                return Result.Success(new Response<TravelRequirementsResponse>
                {
                    Status = EAPI.Domain.Entities.Common.Status.OK,
                    Value = (TravelRequirementsResponse)trmResponse
                });
            }
            catch (HttpRequestException ex)
            {
                var error = ErrorHelper.TravelRequirementsErrorMapper(ex, _logger);

                _logger.LogError(
                    "Save secondary document attestation failed for Travel Requirements with error code {ErrorCode}.",
                    error.Code);

                return Result.Success(new Response<TravelRequirementsResponse>
                {
                    Status = EAPI.Domain.Entities.Common.Status.BusinessException,
                    Errors = [error]
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Save secondary document attestation failed for Travel Requirements.");

                return Result.Failure<Response<TravelRequirementsResponse>>(
                    "Save secondary document attestation failed for Travel Requirements.");
            }
        }
    }
}
