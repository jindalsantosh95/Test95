using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System.Web.Http;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Constants;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPrivate.interfaces;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic.interfaces;
using MSSchema = VAA.EAPI.Microservices.Retail.Trip.Domain.UpsellDetails;

namespace VAA.EAPI.Microservices.Retail.Trip.Application
{
    public class UpsellOfferDetailsService : IUpsellOfferDetailsService
    {
        private readonly IDeltaUpsellOfferDetailsService _deltaUpsellOfferDetailsService;
        private readonly IDeltaUpsellOfferDetailsPrivateService _deltaUpsellOfferDetailsPrivateService;
        private readonly ILogger<UpsellOfferDetailsService> _logger;
        private readonly bool _switchToPrivateEndpoints = ConfigReader.Try<bool>(Constant.Environment.SwitchToPrivateEndpoints);

        public UpsellOfferDetailsService(IDeltaUpsellOfferDetailsService deltaUpsellOfferDetailsService, IDeltaUpsellOfferDetailsPrivateService deltaUpsellOfferDetailsPrivateService, ILogger<UpsellOfferDetailsService> logger)
        {
            _deltaUpsellOfferDetailsService = deltaUpsellOfferDetailsService ?? throw new ArgumentNullException(nameof(deltaUpsellOfferDetailsService));
            _deltaUpsellOfferDetailsPrivateService = deltaUpsellOfferDetailsPrivateService ?? throw new ArgumentNullException(nameof(deltaUpsellOfferDetailsPrivateService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<MSSchema.UpsellOfferResponse>>> GetUpsellOfferDetails(string tripBookingID)
        {
            try
            {
                var dlResponse = _switchToPrivateEndpoints ? await _deltaUpsellOfferDetailsPrivateService.RetrieveUpsellOfferDetails(tripBookingID) : await _deltaUpsellOfferDetailsService.RetrieveUpsellOfferDetails(tripBookingID);

                return new Response<MSSchema.UpsellOfferResponse>()
                {
                    Status = Status.OK,
                    Value = (MSSchema.UpsellOfferResponse)dlResponse,
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Exception occurred while retrieving upsell offer details.{Json.Serialize(ex.Data["Response"])} | Trip reservation id : {tripBookingID}");

                var dlError = Json.Deserialize<UpsellErrorResponse>(ex.Data["Response"]?.ToString() ?? string.Empty);

                return new Response<MSSchema.UpsellOfferResponse>()
                {
                    Status = Status.BusinessException,
                    Errors =
                    [
                     new ()
                        {
                         Code = dlError?.Errors?.FirstOrDefault()?.Code,
                         Message = dlError?.Errors?.FirstOrDefault()?.MoreInfo ?.FirstOrDefault() ?.Message,
                         DeveloperMessage =$"{dlError ?.Errors ?.FirstOrDefault() ?.MoreInfo ?.FirstOrDefault() ?.Message}",
                         ErrorSource = "Delta",
                        }
                    ]
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Unexpected exception occurred.| Trip reservation id : {tripBookingID}");

                var dlError = Json.Deserialize<UpsellErrorResponse>(await ((HttpResponseException)ex.InnerException)?.Response?.Content?.ReadAsStringAsync());

                if (dlError != null)
                    return new Response<MSSchema.UpsellOfferResponse>()
                    {
                        Status = Status.BusinessException,
                        Errors =
                        [
                         new ()
                        {
                         Code = dlError?.Errors?.FirstOrDefault()?.Code,
                         Message = dlError?.Errors?.FirstOrDefault()?.MoreInfo ?.FirstOrDefault() ?.Message,
                         DeveloperMessage =$"{dlError?.Errors?.FirstOrDefault() ?.MoreInfo ?.FirstOrDefault() ?.Message} | ErrorID: {AsyncContext.Current.CorrelationId.ToString()} | TimeStamp: {DateTime.UtcNow}",
                         ErrorSource = "Delta",
                        }
                        ]
                    };

                else
                    return new Response<MSSchema.UpsellOfferResponse>()
                    {
                        Status = Status.BusinessException,
                        Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.TechnicalError, AsyncContext.Current.CorrelationId.ToString(), Constants.ErrorCodesPath)
                    };
            }
        }
    }
}
