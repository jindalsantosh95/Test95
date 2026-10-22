using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Web.Http;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Constants;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPrivate.interfaces;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic.interfaces;

namespace VAA.EAPI.Microservices.Retail.Trip.Application
{
    public class TripService : ITripService
    {
        private readonly IDeltaTripsDetailsService _deltaTripsDetailsService;
        private readonly IDeltaTripsDetailsPrivateService _deltaTripsDetailsPrivateService;
        private readonly ILogger<TripService> _logger;
        private readonly bool _switchToPrivateEndpoints = ConfigReader.Try<bool>(Constant.Environment.SwitchToPrivateEndpoints);

        public TripService(IDeltaTripsDetailsService deltaTripsDetailsService, IDeltaTripsDetailsPrivateService deltaTripsDetailsPrivateService, ILogger<TripService> logger)
        {
            _deltaTripsDetailsService = deltaTripsDetailsService ?? throw new ArgumentNullException(nameof(deltaTripsDetailsService));
            _deltaTripsDetailsPrivateService = deltaTripsDetailsPrivateService ?? throw new ArgumentNullException(nameof(deltaTripsDetailsPrivateService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<TripsDetailsResponse>>> GetTripsDetails(TripsDetailsRequest request)
        {
            try
            {
                var dlResponse = _switchToPrivateEndpoints ? await _deltaTripsDetailsPrivateService.RetrieveTripsDetails((DeltaTripsDetailsRequest)request) : await _deltaTripsDetailsService.RetrieveTripsDetails((DeltaTripsDetailsRequest)request);

                return new Response<TripsDetailsResponse>()
                {
                    Status = Status.OK,
                    Value = (TripsDetailsResponse)(dlResponse)
                };
            }

            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {Json.Serialize(ex.Data["Response"])} | Booking reference : {request?.ConfirmationNum ?? request?.TicketNum}");

                var exInner = (HttpResponseException)ex?.InnerException;
                var dlError = Json.Deserialize<DeltaTripsDetailsResponse>(await exInner?.Response.Content.ReadAsStringAsync());

                return new Response<TripsDetailsResponse>()
                {
                    Status = exInner.Response.StatusCode == System.Net.HttpStatusCode.BadRequest ? Status.BadRequest : Status.BusinessException,
                    Errors =
                    [
                     new ()
                         {
                          Code = dlError.Errors?.FirstOrDefault()?.MoreInfo?.FirstOrDefault()?.Code,
                          Message = dlError.Errors?.FirstOrDefault()?.MoreInfo ?.FirstOrDefault() ?.Message,
                          DeveloperMessage =$"{dlError.Errors?.FirstOrDefault()?.MoreInfo?.FirstOrDefault()?.DeveloperMessage}",
                          ErrorSource = "Delta",
                         }
                    ]
                };

            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {ex} | Booking reference : {request.ConfirmationNum ?? request.TicketNum}");

                if (ex.InnerException is HttpResponseException)
                {
                    var httpEx = (HttpResponseException)ex.InnerException;
                    var dlError = Json.Deserialize<DeltaTripsDetailsResponse>(await (httpEx?.Response?.Content?.ReadAsStringAsync()));
                    if (dlError != null)
                        return new Response<TripsDetailsResponse>()
                        {
                            Status = httpEx.Response.StatusCode == System.Net.HttpStatusCode.BadRequest ? Status.BadRequest : Status.BusinessException,
                            Errors =
                            [
                                 new ()
                                    {
                                        Code = dlError?.Errors?.FirstOrDefault()?.MoreInfo?.FirstOrDefault()?.Code,
                                        Message = dlError?.Errors?.FirstOrDefault()?.MoreInfo ?.FirstOrDefault() ?.Message,
                                        DeveloperMessage =$"{dlError?.Errors?.FirstOrDefault()?.MoreInfo?.FirstOrDefault()?.DeveloperMessage} | ErrorID: {AsyncContext.Current.CorrelationId.ToString()} | TimeStamp: {DateTime.UtcNow}",
                                        ErrorSource = "Delta",
                                    }
                            ]
                        };
                    else
                    {
                        return new Response<TripsDetailsResponse>()
                        {
                            Status = Status.Error,
                            Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.TechnicalError, AsyncContext.Current.CorrelationId.ToString(), Constants.ErrorCodesPath)
                        };
                    }

                }
                else
                {
                    return new Response<TripsDetailsResponse>()
                    {
                        Status = Status.Error,
                        Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.TechnicalError, AsyncContext.Current.CorrelationId.ToString(), Constants.ErrorCodesPath)
                    };
                }
            }


        }

        public async Task<Result<Response<TripListResponseStore>>> GetTripsDetailsList(IEnumerable<TripsDetailsRequest> tripDetailsRequestList)
        {
            ConcurrentBag<Tuple<TripsDetailsRequest, TripsDetailsResponse>> tripListResponse = [];
            await Parallel.ForEachAsync(tripDetailsRequestList, async (tripRequest, index) =>
            {
                try
                {
                    var result = _switchToPrivateEndpoints ? await _deltaTripsDetailsPrivateService.RetrieveTripsDetails((DeltaTripsDetailsRequest)tripRequest) : await _deltaTripsDetailsService.RetrieveTripsDetails((DeltaTripsDetailsRequest)tripRequest);

                    tripListResponse.Add(Tuple.Create(tripRequest, (TripsDetailsResponse)result));
                }

                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} | Search parameter - {tripRequest.ConfirmationNum ?? tripRequest.TicketNum} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {ex} | Booking reference : {tripRequest.ConfirmationNum ?? tripRequest.TicketNum}");

                    TripsDetailsResponse response = new();
                    if (ex.InnerException is HttpResponseException)
                    {
                        var httpEx = (HttpResponseException)ex.InnerException;
                        var dlError = Json.Deserialize<DeltaTripsDetailsResponse>(await (httpEx)?.Response?.Content?.ReadAsStringAsync());

                        if (dlError != null)
                        {
                            response = new TripsDetailsResponse()
                            {
                                Status = httpEx.Response.StatusCode,
                                Errors =
                            [
                                 new ()
                                    {
                                        Code = dlError?.Errors?.FirstOrDefault()?.MoreInfo?.FirstOrDefault()?.Code,
                                        Message = dlError?.Errors?.FirstOrDefault()?.MoreInfo ?.FirstOrDefault() ?.Message,
                                        DeveloperMessage =$"{dlError?.Errors?.FirstOrDefault()?.MoreInfo?.FirstOrDefault()?.DeveloperMessage} | ErrorID: {AsyncContext.Current.CorrelationId.ToString()} | TimeStamp: {DateTime.UtcNow}",
                                        ErrorSource = "Delta",
                                    }
                            ]
                            };

                        }
                    }

                    else
                    {
                        response = new TripsDetailsResponse()
                        {
                            Status = System.Net.HttpStatusCode.InternalServerError,
                            Errors =
                            [
                                 new ()
                                    {
                                        Message = ex.Message,
                                        DeveloperMessage =$"{ex.Message} | ErrorID: {AsyncContext.Current.CorrelationId.ToString()} | TimeStamp: {DateTime.UtcNow}",
                                    }
                            ]
                        };

                    }

                    tripListResponse.Add(Tuple.Create(tripRequest, response));
                }
            });

            return Result.Success(new Response<TripListResponseStore>
            {
                Status = Status.OK,
                Value = (TripListResponseStore)tripListResponse
            });
        }
    }
}
