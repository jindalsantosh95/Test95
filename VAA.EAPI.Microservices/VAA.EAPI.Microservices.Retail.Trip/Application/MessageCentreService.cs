using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System.Web.Http;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Constants;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPrivate.interfaces;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic.interfaces;
using MessageServiceErrorDetails = VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMessageCentre.ErrorDetails;
using MSSchema = VAA.EAPI.Microservices.Retail.Trip.Domain.MessageDetails;

namespace VAA.EAPI.Microservices.Retail.Trip.Application
{
    public class MessageCentreService : IMessageCentreService
    {
        IDeltaMessageDetailsService _deltaMessageDetailsService;
        IDeltaMessageDetailsPrivateService _deltaMessageDetailsPrivateService;
        private readonly bool _switchToPrivateEndpoints = ConfigReader.Try<bool>(Constant.Environment.SwitchToPrivateEndpoints);
        private readonly ILogger<MessageCentreService> _logger;

        public MessageCentreService(IDeltaMessageDetailsService deltaMessageDetailsService, IDeltaMessageDetailsPrivateService deltaMessageDetailsPrivateService, ILogger<MessageCentreService> logger)
        {
            _deltaMessageDetailsService = deltaMessageDetailsService ?? throw new ArgumentNullException(nameof(deltaMessageDetailsService));
            _deltaMessageDetailsPrivateService = deltaMessageDetailsPrivateService ?? throw new ArgumentNullException(nameof(deltaMessageDetailsPrivateService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<MSSchema.MessageResponse>>> GetMessageDetails(string tripBookingID)
        {
            try
            {
                var dlResponse = _switchToPrivateEndpoints ? await _deltaMessageDetailsPrivateService.GetMessageDetailsAsync(tripBookingID) : await _deltaMessageDetailsService.GetMessageDetailsAsync(tripBookingID);

                return new Response<MSSchema.MessageResponse>()
                {
                    Status = Status.OK,
                    Value = (MSSchema.MessageResponse)dlResponse,
                };
            }

            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {Json.Serialize(ex.Data["Response"])}| Trip reservation id : {tripBookingID}");

                var exInner = (HttpResponseException)ex?.InnerException;
                var dlError = Json.Deserialize<MessageServiceErrorDetails>(await exInner.Response.Content.ReadAsStringAsync());

                return new Response<MSSchema.MessageResponse>()
                {
                    Status = Status.BusinessException,
                    Errors =
                    [
                     new ()
                        {
                         Code = dlError.Errors?.FirstOrDefault()?.Code,
                         Message = dlError.Errors?.FirstOrDefault()?.Message,
                         DeveloperMessage =$"{dlError.Errors?.FirstOrDefault()?.DeveloperMessage.Developermessage}",
                         ErrorSource = "Delta",
                        }
                    ]
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {ex} | Trip reservation id : {tripBookingID}");

                var dlError = Json.Deserialize<MessageServiceErrorDetails>(await (ex.InnerException as HttpResponseException)?.Response?.Content?.ReadAsStringAsync());

                if (dlError != null)
                    return new Response<MSSchema.MessageResponse>()
                    {
                        Status = Status.BusinessException,
                        Errors =
                        [
                         new ()
                        {
                         Code = dlError?.Errors?.FirstOrDefault()?.Code,
                         Message = dlError?.Errors?.FirstOrDefault()?.Message,
                         DeveloperMessage =$"{dlError?.Errors?.FirstOrDefault()?.DeveloperMessage.Developermessage} | ErrorID: {AsyncContext.Current.CorrelationId.ToString()} | TimeStamp: {DateTime.UtcNow}",
                         ErrorSource = "Delta",
                        }
                        ]
                    };

                else
                    return new Response<MSSchema.MessageResponse>()
                    {
                        Status = Status.BusinessException,
                        Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.TechnicalError, AsyncContext.Current.CorrelationId.ToString(), Constants.ErrorCodesPath)
                    };
            }
        }
    }
}
