using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System.Web.Http;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Constants;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPrivate.interfaces;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic.interfaces;
using DlSchema = VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo;
using MSSchema = VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails;

namespace VAA.EAPI.Microservices.Retail.Trip.Application
{
    public class PassengerDetailsService : IPassengerDetailsService
    {
        private readonly IDeltaPassengerDetailsService _deltaPassengerDetailsService;
        private readonly IDeltaPassengerDetailsPrivateService _deltaPassengerDetailsPrivateService;
        private readonly bool _switchToPrivateEndpoints = ConfigReader.Try<bool>(Constant.Environment.SwitchToPrivateEndpoints);
        private readonly ILogger<PassengerDetailsService> _logger;

        public PassengerDetailsService(IDeltaPassengerDetailsService deltaPassengerDetailsService, IDeltaPassengerDetailsPrivateService deltaPassengerDetailsPrivateService, ILogger<PassengerDetailsService> logger)
        {
            _deltaPassengerDetailsService = deltaPassengerDetailsService ?? throw new ArgumentNullException(nameof(deltaPassengerDetailsService));
            _deltaPassengerDetailsPrivateService = deltaPassengerDetailsPrivateService ?? throw new ArgumentNullException(nameof(deltaPassengerDetailsPrivateService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<MSSchema.PassengerDetails>>> GetPassengerDetails(string tripBookingID)
        {
            try
            {
                var dlResponse = _switchToPrivateEndpoints ? await _deltaPassengerDetailsPrivateService.RetrievePassengerDetails(tripBookingID) : await _deltaPassengerDetailsService.RetrievePassengerDetails(tripBookingID);

                return new Response<MSSchema.PassengerDetails>()
                {
                    Status = Status.OK,
                    Value = (MSSchema.PassengerDetails)dlResponse,
                };
            }

            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {Json.Serialize(ex.Data["Response"])} | Trip reservation id : {tripBookingID}");

                var exInner = (HttpResponseException)ex?.InnerException;
                var dlError = Json.Deserialize<DlSchema.PassengerDetailsErrorResponse>(await exInner.Response.Content.ReadAsStringAsync());

                return new Response<MSSchema.PassengerDetails>()
                {
                    Status = Status.BusinessException,
                    Errors =
                    [
                     new ()
                        {
                         Code = dlError?.Errors?.FirstOrDefault()?.Code,
                         Message = dlError.Errors?.FirstOrDefault()?.MoreInfo ?.FirstOrDefault() ?.Message,
                         DeveloperMessage =$"{dlError.Errors?.FirstOrDefault()?.MoreInfo?.FirstOrDefault()?.Message}",
                         ErrorSource = "Delta",
                        }
                    ]
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {ex} | Trip reservation id : {tripBookingID}");

                var dlError = Json.Deserialize<PassengerDetailsErrorResponse>(await ((HttpResponseException)ex.InnerException)?.Response?.Content?.ReadAsStringAsync());

                if (dlError != null)
                    return new Response<MSSchema.PassengerDetails>()
                    {
                        Status = Status.BusinessException,
                        Errors =
                        [
                         new ()
                        {
                         Code = dlError?.Errors?.FirstOrDefault()?.Code,
                         Message = dlError?.Errors?.FirstOrDefault()?.MoreInfo ?.FirstOrDefault() ?.Message,
                         DeveloperMessage =$"{dlError?.Errors?.FirstOrDefault()?.MoreInfo?.FirstOrDefault()?.Message} | ErrorID: {AsyncContext.Current.CorrelationId.ToString()} | TimeStamp: {DateTime.UtcNow}",
                         ErrorSource = "Delta",
                        }
                        ]
                    };

                else
                    return new Response<MSSchema.PassengerDetails>()
                    {
                        Status = Status.BusinessException,
                        Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.TechnicalError, AsyncContext.Current.CorrelationId.ToString(), Constants.ErrorCodesPath)

                    };
            }
        }

        public async Task<Result<Response<PassengerDetails>>> UpatePassengerDetails(PassengerDetails passengerDetails, string tripBookingId)
        {
            if (passengerDetails == null)
                throw new ArgumentNullException(nameof(passengerDetails));

            try
            {
                var dlResponse = _switchToPrivateEndpoints ? await _deltaPassengerDetailsPrivateService.UpdatePassengerDetails((PassengerInformation)(passengerDetails.PassengerInformation, tripBookingId)) : await _deltaPassengerDetailsService.UpdatePassengerDetails((PassengerInformation)(passengerDetails.PassengerInformation, tripBookingId));

                return new Response<MSSchema.PassengerDetails>()
                {
                    Status = Status.OK,
                    Value = (MSSchema.PassengerDetails)dlResponse,
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {Json.Serialize(ex.Data["Response"])} | Trip reservation id : {tripBookingId}");

                var exInner = (HttpResponseException)ex?.InnerException;
                var dlError = Json.Deserialize<DlSchema.PassengerDetailsErrorResponse>(await exInner.Response.Content.ReadAsStringAsync());

                return new Response<MSSchema.PassengerDetails>()
                {
                    Status = Status.BusinessException,
                    Errors =
                    [
                     new ()
                        {
                         Code = dlError?.Errors?.FirstOrDefault()?.Code,
                         Message = dlError?.Errors?.FirstOrDefault()?.MoreInfo ?.FirstOrDefault() ?.Message,
                         DeveloperMessage =$"{dlError.Errors?.FirstOrDefault()?.MoreInfo?.FirstOrDefault()?.Message}",
                         ErrorSource = "Delta",
                        }
                    ]
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {Json.Serialize(ex.Data["Response"])} | Trip reservation id : {tripBookingId}");

                var dlError = Json.Deserialize<PassengerDetailsErrorResponse>(await ((HttpResponseException)ex.InnerException)?.Response?.Content?.ReadAsStringAsync());

                if (dlError != null)
                    return new Response<MSSchema.PassengerDetails>()
                    {
                        Status = Status.BusinessException,
                        Errors =
                        [
                         new ()
                        {
                         Code = dlError?.Errors?.FirstOrDefault()?.Code,
                         Message = dlError?.Errors?.FirstOrDefault()?.MoreInfo ?.FirstOrDefault() ?.Message,
                         DeveloperMessage =$"{dlError?.Errors?.FirstOrDefault()?.MoreInfo?.FirstOrDefault()?.Message} | ErrorID: {AsyncContext.Current.CorrelationId.ToString()} | TimeStamp: {DateTime.UtcNow}",
                         ErrorSource = "Delta",
                        }
                        ]
                    };

                else
                    return new Response<MSSchema.PassengerDetails>()
                    {
                        Status = Status.BusinessException,
                        Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.TechnicalError, AsyncContext.Current.CorrelationId.ToString(), Constants.ErrorCodesPath)

                    };
            }
        }
    }
}
