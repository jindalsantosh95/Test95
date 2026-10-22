using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta.SSR;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.DeltaPrivate.Interfaces;
using VAA.EAPI.Microservices.Flight.Services.Interface;
using VAA.EAPI.Microservices.Flight.Services.Models.SSR;
using DeltaSchema = VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta.SSR;

namespace VAA.EAPI.Microservices.Flight.Services.Application.Service
{
    public class SpecialServiceRequest : ISpecialServiceRequest
    {
        private readonly IDeltaSSRDetailsService _deltaSSRDetailsService;
        private readonly IDeltaSSRDetailsPrivateService _deltaSSRDetailsPrivateService;
        private readonly bool _switchToPrivateEndpoints = ConfigReader.Try<bool>(Constants.Environment.SwitchToPrivateEndpoints);
        private readonly ILogger<SpecialServiceRequest> _logger;

        public SpecialServiceRequest(IDeltaSSRDetailsService deltaSSRDetailsService, IDeltaSSRDetailsPrivateService deltaSSRDetailsPrivateService, ILogger<SpecialServiceRequest> logger)
        {
            _deltaSSRDetailsService = deltaSSRDetailsService ?? throw new ArgumentNullException(nameof(deltaSSRDetailsService));
            _deltaSSRDetailsPrivateService = deltaSSRDetailsPrivateService ?? throw new ArgumentNullException(nameof(deltaSSRDetailsPrivateService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<PassengerSSRResponse>>> GetSSRDetailsAsync(string tripReservationID)
        {
            try
            {
                var ssrData = _switchToPrivateEndpoints ? await _deltaSSRDetailsPrivateService.RetrieveSSRDetails(tripReservationID) : await _deltaSSRDetailsService.RetrieveSSRDetails(tripReservationID);

                var passengerResponse = (PassengerSSRResponse)ssrData;

                return new Response<PassengerSSRResponse>
                {
                    Status = Status.OK,
                    Value = passengerResponse
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "SpecialServiceRequest.GetSSRDetailsAsync failed for {TripReservationId}.", tripReservationID);

                var exInner = (HttpResponseException)ex?.InnerException;
                var errorResponse = Json.Deserialize<DeltaSSRResponse>(await exInner?.Response?.Content?.ReadAsStringAsync());

                return new Response<PassengerSSRResponse>
                {
                    Status = Status.BusinessException,
                    Errors =
                    [
                       new ErrorDetails
                       {
                         Code = errorResponse?.Errors?.FirstOrDefault()?.Code,
                         Message = errorResponse?.Errors?.FirstOrDefault()?.Message,
                         ErrorSource = "ExternalSSR"
                       }
                    ]
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SpecialServiceRequest.GetSSRDetailsAsync failed for {TripReservationId}.", tripReservationID);

                return new Response<PassengerSSRResponse>
                {
                    Status = Status.BusinessException,
                    Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                };
            }
        }

        public async Task<Result<Response<PassengerSSRResponse>>> ManageSSRDetailsAsync(Models.SSR.SSRUpsertRequest ssrUpsertRequest, string tripResevationID)
        {
            try
            {
                var ssrData = await _deltaSSRDetailsService.ManageSSRDetailsAsync((DeltaSchema.SSRUpsertRequest)ssrUpsertRequest, tripResevationID);
                var passengerResponse = (PassengerSSRResponse)ssrData;

                return new Response<PassengerSSRResponse>
                {
                    Status = Status.OK,
                    Value = passengerResponse
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "SpecialServiceRequest.ManageSSRDetailsAsync failed for {TripReservationId}.", tripResevationID);

                var exInner = (HttpResponseException)ex?.InnerException;
                var errorResponse = Json.Deserialize<DeltaSSRResponse>(await exInner?.Response?.Content?.ReadAsStringAsync());

                return new Response<PassengerSSRResponse>
                {
                    Status = Status.BusinessException,
                    Errors =
                    [
                       new ErrorDetails
                       {
                         Code = errorResponse?.Errors?.FirstOrDefault()?.Code,
                         Message = errorResponse?.Errors?.FirstOrDefault()?.Message,
                         DeveloperMessage = errorResponse?.Errors?.FirstOrDefault()?.Message,
                         ErrorSource = "Delta"
                       }
                    ]
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SpecialServiceRequest.ManageSSRDetailsAsync failed for {TripReservationId}.", tripResevationID);

                return new Response<PassengerSSRResponse>
                {
                    Status = Status.BusinessException,
                    Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                };
            }
        }
    }
}
